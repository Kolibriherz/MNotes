
[CmdletBinding()]
param(

    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string] $PackageDirectory,

    [Parameter(Mandatory)]
    [ValidatePattern('^[0-9A-Za-z.-]+$')]
    [string] $PackageVersion,

    [Parameter(Mandatory)]
    [ValidatePattern('^[0-9a-fA-F]{40}$')]
    [string] $Commit,

    # Result Get-PackageHealth.ps1
    #
    #
    #   {
    #       "Health": "auto-ready",
    #       "ScanOk": true,
    #       "AuditUnavailableCount": 0,
    #       "AuditUnavailableCodes": [],
    #       "VulnerabilityCount": 0,
    #       "OutdatedCount": 0,
    #       "DeprecatedCount": 0
    #   }

    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string] $HealthJson
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'


$repositoryRoot = (
    Resolve-Path -LiteralPath (
        Join-Path $PSScriptRoot '../..'
    )
).Path

$resolvedCommit = [string] (
    & git `
        -C $repositoryRoot `
        rev-parse `
        --verify `
        "$Commit^{commit}" `
        2>$null
)


if (
    $LASTEXITCODE -ne 0 -or
    [string]::IsNullOrWhiteSpace($resolvedCommit)
) {
    throw "Commit does not exist in this repository: $Commit"
}


$resolvedCommit =
    $resolvedCommit.Trim().ToLowerInvariant()

if ($resolvedCommit -notmatch '^[0-9a-f]{40}$') {
    throw "Git returned an invalid commit SHA: $resolvedCommit"
}


$packageProjectManifestPath = Join-Path `
    $repositoryRoot `
    'eng/package-projects.json'

if (
    -not (
        Test-Path `
            -LiteralPath $packageProjectManifestPath `
            -PathType Leaf
    )
) {
    throw (
        "Package project manifest does not exist: " +
        $packageProjectManifestPath
    )
}

try {
    $packageProjectManifest =
        Get-Content `
            -Raw `
            -LiteralPath $packageProjectManifestPath |
        ConvertFrom-Json `
            -ErrorAction Stop
}
catch {
    throw (
        "Package project manifest is not valid JSON: " +
        $_.Exception.Message
    )
}


$packageProjectsProperty =
    $packageProjectManifest.PSObject.Properties['packageProjects']


if (
    $null -eq $packageProjectsProperty -or
    @($packageProjectsProperty.Value).Count -eq 0
) {
    throw (
        "Package project manifest contains no packageProjects: " +
        $packageProjectManifestPath
    )
}


$packageIds = @(
    foreach (
        $packageProject in
            @($packageProjectManifest.packageProjects)
    ) {
        $packageId = [string] $packageProject.id

        # Jeder Package-Projekteintrag muss eine nicht leere ID besitzen.
        if ([string]::IsNullOrWhiteSpace($packageId)) {
            throw (
                "A packageProjects entry has no valid package ID in: " +
                $packageProjectManifestPath
            )
        }

        $packageId
    }
)

$uniquePackageIds = @(
    $packageIds |
        Sort-Object -Unique
)


if ($uniquePackageIds.Count -ne $packageIds.Count) {
    throw 'Duplicate package IDs were found in package-projects.json.'
}


$expectedPackageFileNames = @(
    @(
        foreach ($packageId in $packageIds) {
            "$packageId.$PackageVersion.nupkg"
            "$packageId.$PackageVersion.snupkg"
        }
    ) |
        Sort-Object
)

$absolutePackageDirectory = [IO.Path]::GetFullPath(
    (
        Join-Path `
            $repositoryRoot `
            $PackageDirectory
    )
)

if (
    -not (
        Test-Path `
            -LiteralPath $absolutePackageDirectory `
            -PathType Container
    )
) {
    throw (
        "Package directory does not exist: " +
        $absolutePackageDirectory
    )
}


$files = @(
    Get-ChildItem `
        -LiteralPath $absolutePackageDirectory `
        -File |
    Where-Object {
        $_.Extension -in @('.nupkg', '.snupkg')
    } |
    Sort-Object Name
)

$actualPackageFileNames = @(
    $files |
        ForEach-Object Name
)

$missingPackageFiles = @(
    $expectedPackageFileNames |
        Where-Object {
            $actualPackageFileNames -notcontains $_
        }
)

$unexpectedPackageFiles = @(
    $actualPackageFileNames |
        Where-Object {
            $expectedPackageFileNames -notcontains $_
        }
)


if ($missingPackageFiles.Count -gt 0) {
    throw (
        "Expected package files are missing: " +
        ($missingPackageFiles -join ', ')
    )
}


if ($unexpectedPackageFiles.Count -gt 0) {
    throw (
        "Unexpected package files were found: " +
        ($unexpectedPackageFiles -join ', ')
    )
}

if ($files.Count -ne $expectedPackageFileNames.Count) {
    throw (
        "Unexpected package file count. Expected " +
        "$($expectedPackageFileNames.Count), found $($files.Count)."
    )
}


$fileRecords = @(
    foreach ($file in $files) {
     
        $hash = Get-FileHash `
            -LiteralPath $file.FullName `
            -Algorithm SHA256

        [ordered]@{
            Name = $file.Name
            Length = $file.Length
            Sha256 = $hash.Hash.ToLowerInvariant()
        }
    }
)


$checksumLines = @(
    $fileRecords |
        ForEach-Object {
            "$($_.Sha256)  $($_.Name)"
        }
)


Set-Content `
    -LiteralPath (
        Join-Path `
            $absolutePackageDirectory `
            'SHA256SUMS'
    ) `
    -Value $checksumLines `
    -Encoding utf8


try {
    $health =
        $HealthJson |
        ConvertFrom-Json `
            -ErrorAction Stop
}
catch {
    throw (
        "HealthJson is not valid JSON: " +
        $_.Exception.Message
    )
}


$requiredHealthProperties = @(
    'Health',
    'ScanOk',
    'AuditUnavailableCount',
    'AuditUnavailableCodes',
    'VulnerabilityCount',
    'OutdatedCount',
    'DeprecatedCount'
)


foreach ($propertyName in $requiredHealthProperties) {
    $property =
        $health.PSObject.Properties[$propertyName]

    if ($null -eq $property) {
        throw (
            "Required health property is missing: " +
            $propertyName
        )
    }
}

$allowedHealthValues = @(
    'security-blocked',
    'review-required',
    'auto-ready'
)


if (
    $allowedHealthValues -notcontains
        [string] $health.Health
) {
    throw (
        "Unexpected health policy value: " +
        [string] $health.Health
    )
}

if ($health.ScanOk -isnot [bool]) {
    throw 'Health property ScanOk must be a Boolean.'
}

$countPropertyNames = @(
    'AuditUnavailableCount',
    'VulnerabilityCount',
    'OutdatedCount',
    'DeprecatedCount'
)


foreach ($propertyName in $countPropertyNames) {
    $value = $health.$propertyName

    $isInteger =
        $value -is [int] -or
        $value -is [long]

    if (-not $isInteger -or [long] $value -lt 0) {
        throw (
            "Health property $propertyName must be " +
            "a non-negative integer."
        )
    }
}


# ============================================================================
# RELEASE-MANIFEST
# ============================================================================


$manifest = [ordered]@{

    PackageVersion = $PackageVersion
    Commit = $resolvedCommit
    GeneratedAtUtc = [DateTimeOffset]::UtcNow.ToString(
        'yyyy-MM-ddTHH:mm:ssZ',
        [Globalization.CultureInfo]::InvariantCulture
    )
    Health = $health
    Files = $fileRecords
}

$manifest |
    ConvertTo-Json `
        -Depth 10 |
    Set-Content `
        -LiteralPath (
            Join-Path `
                $absolutePackageDirectory `
                'release-manifest.json'
        ) `
        -Encoding utf8

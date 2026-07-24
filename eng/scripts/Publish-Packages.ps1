
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string] $PackageDirectory,

   
    [Parameter(Mandatory)]
    [ValidatePattern('^[0-9A-Za-z.-]+$')]
    [string] $PackageVersion
)


Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($env:NUGET_API_KEY)) {
    throw (
        'NUGET_API_KEY is not available. ' +
        'The workflow must obtain a short-lived key through ' +
        'NuGet Trusted Publishing before publishing.'
    )
}


$repositoryRoot = (
    Resolve-Path -LiteralPath (
        Join-Path $PSScriptRoot '../..'
    )
).Path

$packageProjectsPath = Join-Path `
    $repositoryRoot `
    'eng/package-projects.json'


if (
    -not (
        Test-Path `
            -LiteralPath $packageProjectsPath `
            -PathType Leaf
    )
) {
    throw "Package project manifest does not exist: $packageProjectsPath"
}

try {
    $packageProjectsManifest =
        Get-Content `
            -Raw `
            -LiteralPath $packageProjectsPath |
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
    $packageProjectsManifest.PSObject.Properties['packageProjects']


if (
    $null -eq $packageProjectsProperty -or
    @($packageProjectsProperty.Value).Count -eq 0
) {
    throw 'package-projects.json contains no packageProjects.'
}


$packageIds = @(
    foreach (
        $packageProject in
            @($packageProjectsManifest.packageProjects)
    ) {
        $packageId = [string] $packageProject.id

        if ([string]::IsNullOrWhiteSpace($packageId)) {
            throw 'A packageProjects entry contains no valid package ID.'
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


$absolutePackageDirectory = if (
    [IO.Path]::IsPathRooted($PackageDirectory)
) {
    [IO.Path]::GetFullPath($PackageDirectory)
}
else {
    [IO.Path]::GetFullPath(
        (
            Join-Path `
                $repositoryRoot `
                $PackageDirectory
        )
    )
}

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

$packagesRoot = [IO.Path]::GetFullPath(
    (
        Join-Path `
            $repositoryRoot `
            'artifacts/packages'
    )
)

$relativePackagePath = [IO.Path]::GetRelativePath(
    $packagesRoot,
    $absolutePackageDirectory
)


$escapesPackagesRoot =
    $relativePackagePath -eq '..' -or
    [IO.Path]::IsPathRooted($relativePackagePath) -or
    $relativePackagePath.StartsWith(
        '../',
        [StringComparison]::Ordinal
    ) -or
    $relativePackagePath.StartsWith(
        '..\',
        [StringComparison]::Ordinal
    )


if ($escapesPackagesRoot) {
    throw (
        "Package directory is outside artifacts/packages: " +
        $absolutePackageDirectory
    )
}

$trimmedPackageDirectory =
    $absolutePackageDirectory.TrimEnd(
        [IO.Path]::DirectorySeparatorChar,
        [IO.Path]::AltDirectorySeparatorChar
    )


$packageDirectoryName =
    [IO.Path]::GetFileName($trimmedPackageDirectory)


if ($packageDirectoryName -cne $PackageVersion) {
    throw (
        "Package directory name does not match PackageVersion. " +
        "Directory: $packageDirectoryName; " +
        "Version: $PackageVersion"
    )
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


$checksumPath = Join-Path `
    $absolutePackageDirectory `
    'SHA256SUMS'


if (
    -not (
        Test-Path `
            -LiteralPath $checksumPath `
            -PathType Leaf
    )
) {
    throw 'SHA256SUMS is missing.'
}

$checksumMap = @{}


$lineNumber = 0


foreach ($line in Get-Content -LiteralPath $checksumPath) {
    $lineNumber++

    if ($line -notmatch '^([0-9a-fA-F]{64})  (.+)$') {
        throw (
            "Invalid checksum line $lineNumber`: $line"
        )
    }


    $expectedHash =
        $Matches[1].ToLowerInvariant()

    $fileName =
        $Matches[2]

    if (
        $fileName.Contains('/') -or
        $fileName.Contains('\') -or
        [IO.Path]::IsPathRooted($fileName) -or
        [IO.Path]::GetFileName($fileName) -ne $fileName
    ) {
        throw (
            "Checksum entry is not a plain file name: " +
            $fileName
        )
    }

    if ($checksumMap.ContainsKey($fileName)) {
        throw "Duplicate checksum entry: $fileName"
    }


    $checksumMap[$fileName] = $expectedHash
}


$missingChecksumEntries = @(
    $expectedPackageFileNames |
        Where-Object {
            -not $checksumMap.ContainsKey($_)
        }
)

$unexpectedChecksumEntries = @(
    $checksumMap.Keys |
        Where-Object {
            $expectedPackageFileNames -notcontains $_
        }
)


if ($missingChecksumEntries.Count -gt 0) {
    throw (
        "SHA256SUMS is missing entries for: " +
        ($missingChecksumEntries -join ', ')
    )
}


if ($unexpectedChecksumEntries.Count -gt 0) {
    throw (
        "SHA256SUMS contains unexpected entries: " +
        ($unexpectedChecksumEntries -join ', ')
    )
}


$releaseManifestPath = Join-Path `
    $absolutePackageDirectory `
    'release-manifest.json'


if (
    -not (
        Test-Path `
            -LiteralPath $releaseManifestPath `
            -PathType Leaf
    )
) {
    throw 'release-manifest.json is missing.'
}


try {
    $releaseManifest =
        Get-Content `
            -Raw `
            -LiteralPath $releaseManifestPath |
        ConvertFrom-Json `
            -ErrorAction Stop
}
catch {
    throw (
        "release-manifest.json is not valid JSON: " +
        $_.Exception.Message
    )
}


$manifestVersionProperty =
    $releaseManifest.PSObject.Properties['PackageVersion']

$manifestCommitProperty =
    $releaseManifest.PSObject.Properties['Commit']

$manifestHealthProperty =
    $releaseManifest.PSObject.Properties['Health']

$manifestFilesProperty =
    $releaseManifest.PSObject.Properties['Files']


if ($null -eq $manifestVersionProperty) {
    throw 'release-manifest.json contains no PackageVersion.'
}

if ($null -eq $manifestCommitProperty) {
    throw 'release-manifest.json contains no Commit.'
}

if ($null -eq $manifestHealthProperty) {
    throw 'release-manifest.json contains no Health object.'
}

if ($null -eq $manifestFilesProperty) {
    throw 'release-manifest.json contains no Files list.'
}


if (
    [string] $releaseManifest.PackageVersion -cne
        $PackageVersion
) {
    throw (
        "Release manifest version does not match. " +
        "Manifest: $($releaseManifest.PackageVersion); " +
        "Expected: $PackageVersion"
    )
}


if (
    [string] $releaseManifest.Commit -notmatch
        '^[0-9a-fA-F]{40}$'
) {
    throw (
        "Release manifest contains an invalid commit SHA: " +
        [string] $releaseManifest.Commit
    )
}


$healthStateProperty =
    $releaseManifest.Health.PSObject.Properties['Health']


if ($null -eq $healthStateProperty) {
    throw 'Release manifest Health object contains no Health state.'
}


$healthState =
    [string] $releaseManifest.Health.Health


switch ($healthState) {
    'auto-ready' {
    }

    'review-required' {
        Write-Host (
            'Package health requires review. ' +
            'Proceeding because the workflow reached the publish step.'
        )
    }

    'security-blocked' {
        throw (
            'Publishing is forbidden because package health is ' +
            'security-blocked.'
        )
    }

    default {
        throw "Unknown package health state: $healthState"
    }
}


$releaseManifestFileMap = @{}


foreach ($fileRecord in @($releaseManifest.Files)) {
    $nameProperty =
        $fileRecord.PSObject.Properties['Name']

    $lengthProperty =
        $fileRecord.PSObject.Properties['Length']

    $hashProperty =
        $fileRecord.PSObject.Properties['Sha256']


    if (
        $null -eq $nameProperty -or
        $null -eq $lengthProperty -or
        $null -eq $hashProperty
    ) {
        throw (
            'A release manifest file record is missing ' +
            'Name, Length or Sha256.'
        )
    }


    $fileName =
        [string] $fileRecord.Name

    $manifestHash =
        ([string] $fileRecord.Sha256).ToLowerInvariant()


    if (
        [string]::IsNullOrWhiteSpace($fileName) -or
        $fileName.Contains('/') -or
        $fileName.Contains('\')
    ) {
        throw (
            "Invalid file name in release manifest: " +
            $fileName
        )
    }


    if ($manifestHash -notmatch '^[0-9a-f]{64}$') {
        throw (
            "Invalid SHA-256 value in release manifest for: " +
            $fileName
        )
    }


    $lengthValue =
        $fileRecord.Length


    $isIntegerLength =
        $lengthValue -is [int] -or
        $lengthValue -is [long]


    if (
        -not $isIntegerLength -or
        [long] $lengthValue -lt 0
    ) {
        throw (
            "Invalid file length in release manifest for: " +
            $fileName
        )
    }


    if ($releaseManifestFileMap.ContainsKey($fileName)) {
        throw (
            "Duplicate file record in release manifest: " +
            $fileName
        )
    }


    $releaseManifestFileMap[$fileName] = [ordered]@{
        Length = [long] $lengthValue
        Sha256 = $manifestHash
    }
}


$missingManifestEntries = @(
    $expectedPackageFileNames |
        Where-Object {
            -not $releaseManifestFileMap.ContainsKey($_)
        }
)


$unexpectedManifestEntries = @(
    $releaseManifestFileMap.Keys |
        Where-Object {
            $expectedPackageFileNames -notcontains $_
        }
)


if ($missingManifestEntries.Count -gt 0) {
    throw (
        "Release manifest is missing package records for: " +
        ($missingManifestEntries -join ', ')
    )
}


if ($unexpectedManifestEntries.Count -gt 0) {
    throw (
        "Release manifest contains unexpected package records: " +
        ($unexpectedManifestEntries -join ', ')
    )
}


$verifiedPackagePaths = @{}


foreach ($fileName in $expectedPackageFileNames) {
    $filePath = Join-Path `
        $absolutePackageDirectory `
        $fileName

    if (
        -not (
            Test-Path `
                -LiteralPath $filePath `
                -PathType Leaf
        )
    ) {
        throw "Expected package file is missing: $filePath"
    }


    $fileInfo = Get-Item -LiteralPath $filePath

    $actualHash = (
        Get-FileHash `
            -LiteralPath $filePath `
            -Algorithm SHA256
    ).Hash.ToLowerInvariant()


    $checksumHash =
        [string] $checksumMap[$fileName]

    $manifestRecord =
        $releaseManifestFileMap[$fileName]

    if ($actualHash -ne $checksumHash) {
        throw (
            "Checksum mismatch against SHA256SUMS: " +
            $fileName
        )
    }

    if ($actualHash -ne [string] $manifestRecord.Sha256) {
        throw (
            "Checksum mismatch against release manifest: " +
            $fileName
        )
    }

    if ($fileInfo.Length -ne [long] $manifestRecord.Length) {
        throw (
            "File length mismatch against release manifest: " +
            $fileName
        )
    }

    $verifiedPackagePaths[$fileName] =
        $filePath
}


$actualPackageFileNames = @(
    Get-ChildItem `
        -LiteralPath $absolutePackageDirectory `
        -File |
    Where-Object {
        $_.Extension -in @('.nupkg', '.snupkg')
    } |
    ForEach-Object Name
)


$unexpectedLocalPackageFiles = @(
    $actualPackageFileNames |
        Where-Object {
            $expectedPackageFileNames -notcontains $_
        }
)


if ($unexpectedLocalPackageFiles.Count -gt 0) {
    throw (
        "Unexpected local package files were found: " +
        ($unexpectedLocalPackageFiles -join ', ')
    )
}


$nugetSource =
    'https://api.nuget.org/v3/index.json'


foreach ($packageId in $packageIds) {
    $packageFileName =
        "$packageId.$PackageVersion.nupkg"

    $symbolFileName =
        "$packageId.$PackageVersion.snupkg"

    $packagePath =
        [string] $verifiedPackagePaths[$packageFileName]

    $symbolPath =
        [string] $verifiedPackagePaths[$symbolFileName]

    if (
        [string]::IsNullOrWhiteSpace($packagePath) -or
        [string]::IsNullOrWhiteSpace($symbolPath)
    ) {
        throw (
            "Verified package paths are incomplete for: " +
            $packageId
        )
    }


    Write-Host "Publishing $packageId $PackageVersion"
    Write-Host "  Package: $packagePath"
    Write-Host "  Symbols: $symbolPath"


    & dotnet nuget push $packagePath `
        --api-key $env:NUGET_API_KEY `
        --symbol-api-key $env:NUGET_API_KEY `
        --source $nugetSource `
        --skip-duplicate `
        --timeout 600


    if ($LASTEXITCODE -ne 0) {
        throw (
            "Publishing failed for $packageId. " +
            "Exit code: $LASTEXITCODE"
        )
    }
}

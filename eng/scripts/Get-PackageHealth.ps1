
[CmdletBinding()]
param(
    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [string] $Solution = 'MNotes.sln',

    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string] $ReportsPath,

    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string] $RestoreLogPath
)


Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'


function Invoke-DotnetJsonReport {
    param(
        
        [Parameter(Mandatory)]
        [string[]] $Arguments,

        [Parameter(Mandatory)]
        [string] $JsonPath,

        [Parameter(Mandatory)]
        [string] $ErrorPath
    )

    $startInfo = [Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = 'dotnet'

    $startInfo.UseShellExecute = $false
    $startInfo.RedirectStandardOutput = $true

    $startInfo.RedirectStandardError = $true

    foreach ($argument in $Arguments) {

        [void] $startInfo.ArgumentList.Add($argument)
    }

    $process = [Diagnostics.Process]::new()
    $process.StartInfo = $startInfo

    try {

        [void] $process.Start()

        $standardOutputTask =
            $process.StandardOutput.ReadToEndAsync()

        $standardErrorTask =
            $process.StandardError.ReadToEndAsync()

        [void] ($process.WaitForExitAsync()).GetAwaiter().GetResult()

        $standardOutput =
            $standardOutputTask.GetAwaiter().GetResult()

        $standardError =
            $standardErrorTask.GetAwaiter().GetResult()

        $exitCode = $process.ExitCode
    }
    finally {
 
        $process.Dispose()
    }

    Set-Content `
        -LiteralPath $JsonPath `
        -Value $standardOutput `
        -Encoding utf8

    Set-Content `
        -LiteralPath $ErrorPath `
        -Value $standardError `
        -Encoding utf8

    if ($exitCode -ne 0) {
        return [ordered]@{
            Success  = $false
            Document = $null
        }
    }


    try {

        $document = $standardOutput | ConvertFrom-Json

        return [ordered]@{
            Success  = $true
            Document = $document
        }
    }
    catch {

        Add-Content `
            -LiteralPath $ErrorPath `
            -Value "JSON parsing failed: $($_.Exception.Message)" `
            -Encoding utf8

        return [ordered]@{
            Success  = $false
            Document = $null
        }
    }
}



function Get-PackageEntries {
    param(

        [Parameter(Mandatory)]
        $Document
    )

    $entries = [Collections.Generic.List[object]]::new()
    foreach ($project in @($Document.projects)) {


        $frameworksProperty = $project.PSObject.Properties['frameworks']

        if ($null -eq $frameworksProperty) {
            continue
        }

        foreach ($framework in @($frameworksProperty.Value)) {

            foreach ($groupName in @(
                'topLevelPackages',
                'transitivePackages'
            )) {

                $property =
                    $framework.PSObject.Properties[$groupName]

                if ($null -ne $property) {
                    foreach ($package in @($property.Value)) {

                        if ($null -ne $package) {
                            $entries.Add($package)
                        }
                    }
                }
            }
        }
    }

    return $entries
}


$repositoryRoot = (
    Resolve-Path -LiteralPath (
        Join-Path $PSScriptRoot '../..'
    )
).Path

$absoluteReportsPath = [IO.Path]::GetFullPath(
    (Join-Path $repositoryRoot $ReportsPath)
)

$absoluteRestoreLog = [IO.Path]::GetFullPath(
    (Join-Path $repositoryRoot $RestoreLogPath)
)


New-Item `
    -ItemType Directory `
    -Force `
    -Path $absoluteReportsPath |
    Out-Null


$commonArguments = @(
    'package',
    'list',
    '--project',
    $Solution,
    '--no-restore',
    '--format',
    'json',
    '--output-version',
    '1'
)

Push-Location $repositoryRoot

try {

    $outdated = Invoke-DotnetJsonReport `
        -Arguments @($commonArguments + '--outdated') `
        -JsonPath (
            Join-Path $absoluteReportsPath 'outdated.json'
        ) `
        -ErrorPath (
            Join-Path $absoluteReportsPath 'outdated.stderr.log'
        )

    $vulnerable = Invoke-DotnetJsonReport `
        -Arguments @($commonArguments + '--vulnerable' + '--include-transitive') `
        -JsonPath (
            Join-Path $absoluteReportsPath 'vulnerable.json'
        ) `
        -ErrorPath (
            Join-Path $absoluteReportsPath 'vulnerable.stderr.log'
        )

    $deprecated = Invoke-DotnetJsonReport `
        -Arguments @($commonArguments + '--deprecated' + '--include-transitive') `
        -JsonPath (
            Join-Path $absoluteReportsPath 'deprecated.json'
        ) `
        -ErrorPath (
            Join-Path $absoluteReportsPath 'deprecated.stderr.log'
        )

    $scanOk = [bool] (
        $outdated.Success -and
        $vulnerable.Success -and
        $deprecated.Success
    )

    $outdatedCount = 0

    if ($outdated.Success) {

        foreach (
            $entry in @(
                Get-PackageEntries -Document $outdated.Document
            )
        ) {

            $latest =
                $entry.PSObject.Properties['latestVersion']

            $resolved =
                $entry.PSObject.Properties['resolvedVersion']

            if (
                $null -ne $latest -and
                -not [string]::IsNullOrWhiteSpace(
                    [string] $latest.Value
                ) -and
                (
                    $null -eq $resolved -or
                    [string] $latest.Value -ne
                    [string] $resolved.Value
                )
            ) {
                $outdatedCount++
            }
        }
    }

    $vulnerabilityCount = 0


    if ($vulnerable.Success) {

        foreach (
            $entry in @(
                Get-PackageEntries -Document $vulnerable.Document
            )
        ) {

            $property =
                $entry.PSObject.Properties['vulnerabilities']


            if ($null -ne $property) {
                $vulnerabilityCount +=
                    @($property.Value).Count
            }
        }
    }

    $deprecatedCount = 0

    if ($deprecated.Success) {


        foreach (
            $entry in @(
                Get-PackageEntries -Document $deprecated.Document
            )
        ) {
            $hasReasons =
                $null -ne
                $entry.PSObject.Properties['deprecationReasons']

            $hasAlternative =
                $null -ne
                $entry.PSObject.Properties['alternativePackage']

            $isDeprecated =
                $entry.PSObject.Properties['isDeprecated']

            if (
                $hasReasons -or
                $hasAlternative -or
                (
                    $null -ne $isDeprecated -and
                    [bool] $isDeprecated.Value
                )
            ) {
                $deprecatedCount++
            }
        }
    }

    $restoreText = if (
        Test-Path `
            -LiteralPath $absoluteRestoreLog `
            -PathType Leaf
    ) {
        Get-Content `
            -Raw `
            -LiteralPath $absoluteRestoreLog
    }
    else {
        $scanOk = $false
        ''
    }

    $auditUnavailableMatches = @(
        [regex]::Matches(
            $restoreText,
            '\bNU190(?:0|5)\b'
        ) |
            ForEach-Object Value |
            Sort-Object -Unique
    )


    $auditVulnerabilityMatches = @(
        [regex]::Matches(
            $restoreText,
            '\bNU190[1-4]\b'
        ) |
            ForEach-Object Value |
            Sort-Object -Unique
    )

    if (
        $auditVulnerabilityMatches.Count -gt 0 -and
        $vulnerabilityCount -eq 0
    ) {
        $vulnerabilityCount =
            $auditVulnerabilityMatches.Count
    }

    $policy = (
        & (
            Join-Path `
                $PSScriptRoot `
                'Get-PackagePolicy.ps1'
        ) `
            -ScanOk $scanOk `
            -AuditUnavailableCount `
                $auditUnavailableMatches.Count `
            -VulnerabilityCount $vulnerabilityCount `
            -OutdatedCount $outdatedCount `
            -DeprecatedCount $deprecatedCount
    ).Trim()


    [ordered]@{

        Health = $policy
        ScanOk = $scanOk
        AuditUnavailableCount =
            $auditUnavailableMatches.Count
        AuditUnavailableCodes =
            $auditUnavailableMatches
        VulnerabilityCount =
            $vulnerabilityCount
        OutdatedCount =
            $outdatedCount
        DeprecatedCount =
            $deprecatedCount
    } |
        ConvertTo-Json `
            -Depth 5 `
            -Compress
}
finally {

    Pop-Location
}

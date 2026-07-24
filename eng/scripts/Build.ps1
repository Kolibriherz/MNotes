[CmdletBinding()]
param(
    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',

    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string] $VersionSuffix,

    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string] $InformationalVersion
)


Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'


function Invoke-NativeCommand {
    param(
        [Parameter(Mandatory)]
        [string] $FilePath,

        [Parameter(Mandatory)]
        [string[]] $Arguments,

        [Parameter(Mandatory)]
        [string] $FailureMessage
    )

    & $FilePath @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "$FailureMessage Exit code: $LASTEXITCODE"
    }
}

$repositoryRoot = (
    Resolve-Path -LiteralPath (
        Join-Path $PSScriptRoot '../..'
    )
).Path

$manifestPath = Join-Path $repositoryRoot 'eng/package-projects.json'
$manifest =
    Get-Content -Raw -LiteralPath $manifestPath |
    ConvertFrom-Json

$buildLogPath = Join-Path $repositoryRoot 'artifacts/logs/build'
$testResultPath = Join-Path $repositoryRoot 'artifacts/test-results'

New-Item `
    -ItemType Directory `
    -Force `
    -Path $buildLogPath |
    Out-Null

New-Item `
    -ItemType Directory `
    -Force `
    -Path $testResultPath |
    Out-Null

Push-Location $repositoryRoot

try {

    Invoke-NativeCommand `
        -FilePath 'dotnet' `
        -Arguments @(
            'format',
            'MNotes.sln',
            '--verify-no-changes',
            '--no-restore'
        ) `
        -FailureMessage 'Formatting verification failed.'

    foreach ($project in $manifest.buildProjects) {
        $safeName =
            ([string] $project.name) `
                -replace '[^A-Za-z0-9._-]', '_'

        $binLog = Join-Path $buildLogPath "$safeName.binlog"

        Write-Host "Building $($project.name): $($project.path)"

        Invoke-NativeCommand `
            -FilePath 'dotnet' `
            -Arguments @(
                'build',
                [string] $project.path,
                '--configuration',
                $Configuration,
                '--no-restore',
                '--no-dependencies',
                '--verbosity',
                'normal',
                "-p:VersionSuffix=$VersionSuffix",
                "-p:InformationalVersion=$InformationalVersion",
                "-bl:$binLog"
            ) `
            -FailureMessage "Build failed for $($project.name)."
    }

    foreach (
        $project in @(
            $manifest.buildProjects |
                Where-Object kind -eq 'test'
        )
    ) {

        $safeName =
            ([string] $project.name) `
                -replace '[^A-Za-z0-9._-]', '_'

        $diagnosticLog = Join-Path `
            $testResultPath `
            "$safeName-diagnostic.log"

        Write-Host "Testing $($project.name): $($project.path)"

        Invoke-NativeCommand `
            -FilePath 'dotnet' `
            -Arguments @(
                'test',

                [string] $project.path,
                '--configuration',
                $Configuration,
                '--no-build',
                '--no-restore',
                '--results-directory',
                $testResultPath,
                '--logger',
                "trx;LogFileName=$safeName.trx",
                '--collect',
                'XPlat Code Coverage'
            ) `
            -FailureMessage "Tests failed for $($project.name)."
    }
}
finally {

    Pop-Location
}

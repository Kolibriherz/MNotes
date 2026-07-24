
[CmdletBinding()]
param(

    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',

    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string] $PackageVersion,

    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string] $VersionSuffix,

    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string] $InformationalVersion
)


Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ($PackageVersion -notmatch '^[0-9A-Za-z.-]+$') {
    throw "Unsafe package version: $PackageVersion"
}

$repositoryRoot = (
    Resolve-Path -LiteralPath (
        Join-Path $PSScriptRoot '../..'
    )
).Path

$manifest =
    Get-Content `
        -Raw `
        -LiteralPath (
            Join-Path `
                $repositoryRoot `
                'eng/package-projects.json'
        ) |
    ConvertFrom-Json

$packageOutput = Join-Path `
    $repositoryRoot `
    "artifacts/packages/$PackageVersion"


if (Test-Path -LiteralPath $packageOutput) {

    $existingEntries = @(
        Get-ChildItem `
            -LiteralPath $packageOutput `
            -Force
    )

    if ($existingEntries.Count -gt 0) {
        throw "Package output is not empty: $packageOutput"
    }
}


New-Item `
    -ItemType Directory `
    -Force `
    -Path $packageOutput |
    Out-Null


Push-Location $repositoryRoot

try {

    foreach ($package in $manifest.packageProjects) {
        Write-Host "Packing $($package.id): $($package.path)"

        & dotnet pack ([string] $package.path) `
            --configuration $Configuration `
            --no-build `
            --no-restore `
            --output $packageOutput `
            --verbosity normal `
            "-p:PackageVersion=$PackageVersion" `
            "-p:VersionSuffix=$VersionSuffix" `
            "-p:InformationalVersion=$InformationalVersion"


        if ($LASTEXITCODE -ne 0) {
            throw "Pack failed for $($package.id)."
        }

        $nupkg = Join-Path `
            $packageOutput `
            "$($package.id).$PackageVersion.nupkg"

        $snupkg = Join-Path `
            $packageOutput `
            "$($package.id).$PackageVersion.snupkg"
        if (
            -not (
                Test-Path `
                    -LiteralPath $nupkg `
                    -PathType Leaf
            )
        ) {
            throw "Expected package is missing: $nupkg"
        }

        if (
            -not (
                Test-Path `
                    -LiteralPath $snupkg `
                    -PathType Leaf
            )
        ) {
            throw "Expected symbol package is missing: $snupkg"
        }
    }


    $nupkgCount = @(
        Get-ChildItem `
            -LiteralPath $packageOutput `
            -Filter '*.nupkg' `
            -File
    ).Count

    $snupkgCount = @(
        Get-ChildItem `
            -LiteralPath $packageOutput `
            -Filter '*.snupkg' `
            -File
    ).Count


    if ($nupkgCount -ne 4 -or $snupkgCount -ne 4) {
        throw (
            "Expected four nupkg and four snupkg files; " +
            "found $nupkgCount and $snupkgCount."
        )
    }
}
finally {

    Pop-Location
}

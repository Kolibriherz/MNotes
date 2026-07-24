[CmdletBinding()]
param(
    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [string] $Commit = 'HEAD',

    [Parameter(Mandatory)]
    [ValidateRange(1, [int]::MaxValue)]
    [int] $RunNumber
)


Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = (Resolve-Path -LiteralPath (
    Join-Path $PSScriptRoot '../..'
)).Path

$versionProject = Join-Path $repositoryRoot `
    'src/MNoteProvider.Common.Abstractions/MNoteProvider.Common.Abstractions.csproj'

Push-Location $repositoryRoot

try {

    $versionPrefix = (& dotnet msbuild $versionProject `
        -getProperty:VersionPrefix `
        -nologo).Trim()

    if ($LASTEXITCODE -ne 0) {
        throw 'VersionPrefix could not be read.'
    }

    if ($versionPrefix -notmatch `
        '^(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)$') {
        throw "VersionPrefix is not a SemVer core: $versionPrefix"
    }

    $prereleaseLabel = (& dotnet msbuild $versionProject `
        -getProperty:MNotePrereleaseLabel `
        -nologo).Trim()

    if ($LASTEXITCODE -ne 0 -or
        $prereleaseLabel -notmatch '^[0-9A-Za-z-]+$') {
        throw "Invalid prerelease label: $prereleaseLabel"
    }

    $commitIso = (& git show -s --format=%cI $Commit).Trim()

    if ($LASTEXITCODE -ne 0 -or
        [string]::IsNullOrWhiteSpace($commitIso)) {
        throw "Commit time could not be read for $Commit."
    }

    $commitTime = [DateTimeOffset]::Parse(
        $commitIso,
        [Globalization.CultureInfo]::InvariantCulture)

    $utc = $commitTime.UtcDateTime
    $date = $utc.ToString(
        'yyyyMMdd',
        [Globalization.CultureInfo]::InvariantCulture)
    $time = $utc.ToString(
        'HHmm',
        [Globalization.CultureInfo]::InvariantCulture)

    $fullSha = (& git rev-parse $Commit).Trim()
    if ($LASTEXITCODE -ne 0 -or
        $fullSha -notmatch '^[0-9a-fA-F]{40}$') {
        throw "Commit SHA could not be resolved for $Commit."
    }
    $shortSha = $fullSha.Substring(0, 12).ToLowerInvariant()

    $versionSuffix = "$prereleaseLabel.$RunNumber.d$date.t$time"
    $packageVersion = "$versionPrefix-$versionSuffix"
    $informationalVersion = "$packageVersion+sha.$shortSha"

    [ordered]@{
        VersionPrefix        = $versionPrefix
        VersionSuffix        = $versionSuffix
        PackageVersion       = $packageVersion
        InformationalVersion = $informationalVersion
        Commit               = $fullSha.ToLowerInvariant()
        CommitTimeUtc        = $utc.ToString(
            'yyyy-MM-ddTHH:mm:ssZ',
            [Globalization.CultureInfo]::InvariantCulture)
        Tag                  = "mnoteprovider-v$packageVersion"

    } | ConvertTo-Json -Compress
}
finally {
    Pop-Location
}

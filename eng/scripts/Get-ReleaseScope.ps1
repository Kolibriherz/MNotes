[CmdletBinding()]
param(
    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [string] $Commit = 'HEAD',

    [Parameter()]
    [switch] $ForcePackageBuild
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = (
    Resolve-Path -LiteralPath (
        Join-Path $PSScriptRoot '../..'
    )
).Path

$packagePathPatterns = @(

    '^src/MNoteProvider\.Common\.Abstractions/',
    '^src/MNoteProvider\.Common/',
    '^src/MNoteProvider\.ClientService\.Abstractions/',
    '^src/MNoteProvider\.ClientService/',
    '^Directory\.Build\.props$',
    '^Directory\.Build\.targets$',
    '^Directory\.Packages\.props$',
    '^global\.json$',
    '^NuGet\.Config$',
    '^LICENSE\.txt$',
    '^docs/nuget/',
    '^eng/'
)


Push-Location $repositoryRoot

try {

    & git rev-parse --verify $Commit *> $null

    if ($LASTEXITCODE -ne 0) {
        throw "Commit does not exist: $Commit"
    }

    $tags = @(
        & git tag `
            --list 'mnoteprovider-v*' `
            --merged $Commit `
            --sort=-creatordate
    )

    if ($LASTEXITCODE -ne 0) {
        throw 'Release tags could not be listed.'
    }

    $lastReleaseTag = if ($tags.Count -gt 0) {

        [string] $tags[0]
    }
    else {

        $null
    }

    $changedFiles = if ($null -eq $lastReleaseTag) {
        @(
            & git ls-tree -r --name-only $Commit
        )
    }
    else {
        @(
            & git diff --name-only "$lastReleaseTag..$Commit"
        )
    }
    if ($LASTEXITCODE -ne 0) {
        throw 'Changed files could not be determined.'
    }


    $normalizedFiles = @(
        $changedFiles |
            ForEach-Object {
                ([string] $_).Replace('\', '/')
            } |

            Where-Object {
                -not [string]::IsNullOrWhiteSpace($_)
            }
    )

    $packageFiles = @(
        $normalizedFiles |

            Where-Object {

                $file = $_
                @(
                    $packagePathPatterns |
                        Where-Object {
                            $file -match $_
                        }
                ).Count -gt 0
            }
    )

    $packageChanged =
        $ForcePackageBuild.IsPresent -or
        $packageFiles.Count -gt 0

    [ordered]@{

        Commit = (
            & git rev-parse $Commit
        ).Trim().ToLowerInvariant()

        LastReleaseTag = $lastReleaseTag
        PackageChanged = $packageChanged
        ChangedFiles = $normalizedFiles
        PackageRelevantFiles = $packageFiles

    } | ConvertTo-Json -Depth 5 -Compress
}
finally {

    Pop-Location
}

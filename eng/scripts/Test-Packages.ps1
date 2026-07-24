[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string] $PackageDirectory,

    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string] $PackageVersion
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ($PackageVersion -notmatch '^[0-9A-Za-z.-]+$') {
    throw "Unsafe package version: $PackageVersion"
}

function Get-RequiredNodeText {
    param(

        [Parameter(Mandatory)]
        [System.Xml.XmlNode] $Parent,

        [Parameter(Mandatory)]
        [string] $LocalName
    )

    $node = $Parent.SelectSingleNode(
        "*[local-name()='$LocalName']"
    )

    if (
        $null -eq $node -or
        [string]::IsNullOrWhiteSpace($node.InnerText)
    ) {
        throw "Required nuspec element is missing: $LocalName"
    }

    return $node.InnerText.Trim()
}


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

$absolutePackageDirectory = [IO.Path]::GetFullPath(
    (
        Join-Path `
            $repositoryRoot `
            $PackageDirectory
    )
)


# Prüft, ob der Paketordner existiert und tatsächlich ein Verzeichnis ist.
#
# -PathType Container bedeutet:
#
#   Es muss sich um einen Ordner handeln.
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


$packageIds = @(
    'MNoteProvider.Common.Abstractions',
    'MNoteProvider.Common',
    'MNoteProvider.ClientService.Abstractions',
    'MNoteProvider.ClientService'
)



$requiredInternalDependencies = @{
    'MNoteProvider.Common.Abstractions' = @()

    'MNoteProvider.Common' = @(
        'MNoteProvider.Common.Abstractions'
    )

    'MNoteProvider.ClientService.Abstractions' = @(
        'MNoteProvider.Common'
    )

    'MNoteProvider.ClientService' = @(
        'MNoteProvider.ClientService.Abstractions'
    )
}


$requiredExternalDependencies = @{
    'MNoteProvider.Common.Abstractions' = @()
    'MNoteProvider.Common' = @()
    'MNoteProvider.ClientService.Abstractions' = @(
        'OneOf'
    )

    'MNoteProvider.ClientService' = @(
        'Microsoft.AspNetCore.SignalR.Client',
        'Microsoft.Extensions.DependencyInjection.Abstractions',
        'Microsoft.Extensions.Hosting.Abstractions',
        'Microsoft.Extensions.Http',
        'Microsoft.Extensions.Logging.Abstractions'
    )
}

Add-Type -AssemblyName System.IO.Compression.FileSystem


foreach ($packageId in $packageIds) {

    $packagePath = Join-Path `
        $absolutePackageDirectory `
        "$packageId.$PackageVersion.nupkg"

    $symbolPath = Join-Path `
        $absolutePackageDirectory `
        "$packageId.$PackageVersion.snupkg"

    if (
        -not (
            Test-Path `
                -LiteralPath $packagePath `
                -PathType Leaf
        )
    ) {
        throw "Expected package is missing: $packagePath"
    }

    if (
        -not (
            Test-Path `
                -LiteralPath $symbolPath `
                -PathType Leaf
        )
    ) {
        throw "Expected symbol package is missing: $symbolPath"
    }

    $archive = [IO.Compression.ZipFile]::OpenRead($packagePath)

    try {

        #   README.md
        #   LICENSE.txt
        #   lib/net10.0/MNoteProvider.Common.dll
        #   MNoteProvider.Common.nuspec
        $entryNames = @(
            $archive.Entries |
                ForEach-Object FullName
        )

        $nuspecEntries = @(
            $archive.Entries |
                Where-Object {
                    $_.FullName -like '*.nuspec'
                }
        )

        if ($nuspecEntries.Count -ne 1) {
            throw (
                "Expected exactly one nuspec in $packagePath; " +
                "found $($nuspecEntries.Count)."
            )
        }

        $nuspecEntry = $nuspecEntries[0]
        $reader = [IO.StreamReader]::new(
            $nuspecEntry.Open()
        )

        try {
            [xml] $nuspec = $reader.ReadToEnd()
        }
        finally {
            $reader.Dispose()
        }

        $metadata = $nuspec.SelectSingleNode(
            "/*[local-name()='package']" +
            "/*[local-name()='metadata']"
        )

        if ($null -eq $metadata) {
            throw "Nuspec metadata is missing in $packagePath."
        }

        $actualId = Get-RequiredNodeText `
            -Parent $metadata `
            -LocalName 'id'

        $actualVersion = Get-RequiredNodeText `
            -Parent $metadata `
            -LocalName 'version'

        $actualAuthors = Get-RequiredNodeText `
            -Parent $metadata `
            -LocalName 'authors'

        $actualReadme = Get-RequiredNodeText `
            -Parent $metadata `
            -LocalName 'readme'

        if ($actualId -ne $packageId) {
            throw "Unexpected package ID: $actualId"
        }

        if ($actualVersion -ne $PackageVersion) {
            throw (
                "Unexpected package version in " +
                "$packageId`: $actualVersion"
            )
        }

        if ($actualAuthors -ne 'M. Albert') {
            throw (
                "Unexpected package author in " +
                "$packageId`: $actualAuthors"
            )
        }

        if ($actualReadme -ne 'README.md') {
            throw (
                "Unexpected package readme in " +
                "$packageId`: $actualReadme"
            )
        }

        $license = $metadata.SelectSingleNode(
            "*[local-name()='license']"
        )

        if (
            $null -eq $license -or
            $license.GetAttribute('type') -ne 'expression' -or
            $license.InnerText.Trim() -ne 'MIT'
        ) {
            throw "Expected MIT license expression in $packageId."
        }

        $repository = $metadata.SelectSingleNode(
            "*[local-name()='repository']"
        )

        if (
            $null -eq $repository -or
            $repository.GetAttribute('type') -ne 'git' -or
            $repository.GetAttribute('url') -ne
                'https://github.com/Kolibriherz/MNotes.git'
        ) {
            throw "Unexpected repository metadata in $packageId."
        }

        $requiredEntries = @(

            'README.md',
            'LICENSE.txt',
            'THIRD-PARTY-NOTICES.md',
            "lib/net10.0/$packageId.dll",
            "lib/net10.0/$packageId.xml"
        )

        foreach ($requiredEntry in $requiredEntries) {
            if ($entryNames -notcontains $requiredEntry) {
                throw (
                    "Missing package entry in " +
                    "$packageId`: $requiredEntry"
                )
            }
        }

        $forbiddenEntry = $entryNames |
            Where-Object {
                $_ -match (
                    '(?i)' +
                    '(appsettings|' +
                    'testresults|' +
                    'artifacts|' +
                    '\.user$|' +
                    '\.pfx$|' +
                    '\.snk$)'
                )
            } |
            Select-Object -First 1


        if ($null -ne $forbiddenEntry) {
            throw (
                "Forbidden package entry in " +
                "$packageId`: $forbiddenEntry"
            )
        }

        $dependencyNodes = @(
            $nuspec.SelectNodes(
                "//*[local-name()='dependency']"
            )
        )

        $dependencyMap = @{}


        foreach ($dependencyNode in $dependencyNodes) {
            $dependencyId =
                $dependencyNode.GetAttribute('id')

            $dependencyVersion =
                $dependencyNode.GetAttribute('version')

            if (
                $dependencyMap.ContainsKey($dependencyId) -and
                [string] $dependencyMap[$dependencyId] -ne
                    $dependencyVersion
            ) {
                throw (
                    "Conflicting dependency versions in $packageId " +
                    "for $dependencyId."
                )
            }

            $dependencyMap[$dependencyId] =
                $dependencyVersion
        }

        $expectedDependencies = @(
            @($requiredInternalDependencies[$packageId]) +
            @($requiredExternalDependencies[$packageId])
        )

        $missingDependencies = @(
            $expectedDependencies |
                Where-Object {
                    -not $dependencyMap.ContainsKey($_)
                }
        )


        if ($missingDependencies.Count -gt 0) {
            throw (
                "Missing dependencies in $packageId`: " +
                ($missingDependencies -join ', ')
            )
        }

        $unexpectedDependencies = @(
            $dependencyMap.Keys |
                Where-Object {
                    $expectedDependencies -notcontains $_
                }
        )


        if ($unexpectedDependencies.Count -gt 0) {
            throw (
                "Unexpected dependencies in $packageId`: " +
                ($unexpectedDependencies -join ', ')
            )
        }

        foreach (
            $dependencyId in
                $requiredInternalDependencies[$packageId]
        ) {
            $range = [string] $dependencyMap[$dependencyId]

            $allowedRanges = @(
                $PackageVersion,
                "[$PackageVersion]",
                "[$PackageVersion,)"
            )

            if ($allowedRanges -notcontains $range) {
                throw (
                    "Unexpected internal version range in " +
                    "$packageId for $dependencyId`: $range"
                )
            }
        }
    }
    finally {

        $archive.Dispose()
    }
}


$smokeRoot = Join-Path `
    $repositoryRoot `
    "artifacts/package-smoke/$PackageVersion"

if (Test-Path -LiteralPath $smokeRoot) {
    throw "Smoke-test directory already exists: $smokeRoot"
}


New-Item `
    -ItemType Directory `
    -Path $smokeRoot |
    Out-Null


Set-Content `
    -LiteralPath (
        Join-Path $smokeRoot 'Directory.Build.props'
    ) `
    -Value '<Project />' `
    -Encoding utf8

Set-Content `
    -LiteralPath (
        Join-Path $smokeRoot 'Directory.Build.targets'
    ) `
    -Value '<Project />' `
    -Encoding utf8

Set-Content `
    -LiteralPath (
        Join-Path $smokeRoot 'Directory.Packages.props'
    ) `
    -Value '<Project />' `
    -Encoding utf8

$escapedPackageDirectory =
    [Security.SecurityElement]::Escape(
        $absolutePackageDirectory
    )

$nugetConfig = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="candidate" value="$escapedPackageDirectory" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>

  <packageSourceMapping>
    <packageSource key="candidate">
      <package pattern="MNoteProvider.*" />
    </packageSource>

    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
"@

$smokeConfig = Join-Path `
    $smokeRoot `
    'NuGet.Config'

Set-Content `
    -LiteralPath $smokeConfig `
    -Value $nugetConfig `
    -Encoding utf8


$consumerPath = Join-Path `
    $smokeRoot `
    'Consumer'

Invoke-NativeCommand `
    -FilePath 'dotnet' `
    -Arguments @(
        'new',
        'console',
        '--name', 'Consumer',
        '--output', $consumerPath,
        '--framework', 'net10.0',
        '--no-restore'
    ) `
    -FailureMessage 'Smoke consumer creation failed.'

$consumerProject = Join-Path `
    $consumerPath `
    'Consumer.csproj'


Invoke-NativeCommand `
    -FilePath 'dotnet' `
    -Arguments @(
        'package',
        'add',
        'MNoteProvider.ClientService',
        '--project', $consumerProject,
        '--version', $PackageVersion,
        '--no-restore'
    ) `
    -FailureMessage 'Adding the candidate package failed.'

$program = @'
using Microsoft.Extensions.DependencyInjection;
using MNoteProvider.ClientService;

IServiceCollection services = new ServiceCollection();

services.AddMNoteClientService(
    new Uri("https://localhost:6015/"));

Console.WriteLine(services.Count);
'@


Set-Content `
    -LiteralPath (
        Join-Path $consumerPath 'Program.cs'
    ) `
    -Value $program `
    -Encoding utf8

$oldNuGetPackages = $env:NUGET_PACKAGES

$env:NUGET_PACKAGES = Join-Path `
    $smokeRoot `
    'global-packages'


try {

    Invoke-NativeCommand `
        -FilePath 'dotnet' `
        -Arguments @(
            'restore',
            $consumerProject,
            '--configfile', $smokeConfig,
            '--force'
        ) `
        -FailureMessage 'Candidate restore failed.'

    $assetsPath = Join-Path `
        $consumerPath `
        'obj/project.assets.json'

    $assetsText = Get-Content `
        -Raw `
        -LiteralPath $assetsPath

    foreach ($packageId in $packageIds) {
        $assetIdentity = "$packageId/$PackageVersion"

        if (
            $assetsText -notmatch
                [regex]::Escape($assetIdentity)
        ) {
            throw (
                "Candidate was not resolved at the expected version: " +
                $assetIdentity
            )
        }
    }


    Invoke-NativeCommand `
        -FilePath 'dotnet' `
        -Arguments @(
            'build',
            $consumerProject,
            '--configuration', 'Release',
            '--no-restore'
        ) `
        -FailureMessage 'Candidate consumer build failed.'
}
finally {

    $env:NUGET_PACKAGES = $oldNuGetPackages
}

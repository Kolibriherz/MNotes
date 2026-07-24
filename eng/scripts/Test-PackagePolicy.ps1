[CmdletBinding()]
param()


Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$policyScript = Join-Path `
    $PSScriptRoot `
    'Get-PackagePolicy.ps1'

if (
    -not (
        Test-Path `
            -LiteralPath $policyScript `
            -PathType Leaf
    )
) {
    throw "Package policy script does not exist: $policyScript"
}

$allowedPolicyStates = @(
    'auto-ready',
    'review-required',
    'security-blocked'
)

$cases = @(

    [pscustomobject]@{
        Name       = 'Clean scan is automatically ready'
        ScanOk     = $true
        Audit      = 0
        Vulnerable = 0
        Outdated   = 0
        Deprecated = 0
        Expected   = 'auto-ready'
    },


    [pscustomobject]@{
        Name       = 'Outdated package requires review'
        ScanOk     = $true
        Audit      = 0
        Vulnerable = 0
        Outdated   = 1
        Deprecated = 0
        Expected   = 'review-required'
    },

    [pscustomobject]@{
        Name       = 'Deprecated package requires review'
        ScanOk     = $true
        Audit      = 0
        Vulnerable = 0
        Outdated   = 0
        Deprecated = 1
        Expected   = 'review-required'
    },


    [pscustomobject]@{
        Name       = 'Vulnerability requires review'
        ScanOk     = $true
        Audit      = 0
        Vulnerable = 1
        Outdated   = 0
        Deprecated = 0
        Expected   = 'review-required'
    },


    [pscustomobject]@{
        Name       = 'Multiple findings still require review'
        ScanOk     = $true
        Audit      = 0
        Vulnerable = 2
        Outdated   = 3
        Deprecated = 1
        Expected   = 'review-required'
    },

    [pscustomobject]@{
        Name       = 'Unavailable audit data blocks publishing'
        ScanOk     = $true
        Audit      = 1
        Vulnerable = 0
        Outdated   = 0
        Deprecated = 0
        Expected   = 'security-blocked'
    },

    [pscustomobject]@{
        Name       = 'Failed scan blocks publishing'
        ScanOk     = $false
        Audit      = 0
        Vulnerable = 0
        Outdated   = 0
        Deprecated = 0
        Expected   = 'security-blocked'
    },

    [pscustomobject]@{
        Name       = 'Audit failure overrides package findings'
        ScanOk     = $true
        Audit      = 1
        Vulnerable = 1
        Outdated   = 1
        Deprecated = 1
        Expected   = 'security-blocked'
    },


    [pscustomobject]@{
        Name       = 'Scan failure overrides package findings'
        ScanOk     = $false
        Audit      = 0
        Vulnerable = 1
        Outdated   = 1
        Deprecated = 1
        Expected   = 'security-blocked'
    }
)



foreach ($testCase in $cases) {
    $output = @(
        & $policyScript `
            -ScanOk $testCase.ScanOk `
            -AuditUnavailableCount $testCase.Audit `
            -VulnerabilityCount $testCase.Vulnerable `
            -OutdatedCount $testCase.Outdated `
            -DeprecatedCount $testCase.Deprecated
    )

    if ($output.Count -ne 1) {
        throw (
            "Policy case '$($testCase.Name)' expected exactly one " +
            "output item, but received $($output.Count)."
        )
    }

    $actual = [string] $output[0]

    if ($allowedPolicyStates -cnotcontains $actual) {
        throw (
            "Policy case '$($testCase.Name)' returned an unknown " +
            "policy state: '$actual'."
        )
    }

    if ($actual -cne $testCase.Expected) {
        throw (
            "Policy case '$($testCase.Name)' failed. " +
            "Expected '$($testCase.Expected)', but got '$actual'. " +
            "Inputs: ScanOk=$($testCase.ScanOk), " +
            "Audit=$($testCase.Audit), " +
            "Vulnerable=$($testCase.Vulnerable), " +
            "Outdated=$($testCase.Outdated), " +
            "Deprecated=$($testCase.Deprecated)."
        )
    }

    Write-Host "PASS: $($testCase.Name) -> $actual"
}

Write-Host (
    "All $($cases.Count) package policy cases passed. " +
    "The three policy states and their priority rules are valid."
)

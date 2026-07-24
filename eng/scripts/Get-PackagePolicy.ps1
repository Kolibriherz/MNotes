[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [bool] $ScanOk,

    [Parameter(Mandatory)]
    [ValidateRange(0, [int]::MaxValue)]
    [int] $AuditUnavailableCount,

    [Parameter(Mandatory)]
    [ValidateRange(0, [int]::MaxValue)]
    [int] $VulnerabilityCount,

    [Parameter(Mandatory)]
    [ValidateRange(0, [int]::MaxValue)]
    [int] $OutdatedCount,


    [Parameter(Mandatory)]
    [ValidateRange(0, [int]::MaxValue)]
    [int] $DeprecatedCount
)


Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not $ScanOk -or $AuditUnavailableCount -gt 0) {

    'security-blocked'
}


elseif (

    $VulnerabilityCount -gt 0 -or
    $OutdatedCount -gt 0 -or
    $DeprecatedCount -gt 0
) {

    'review-required'
}


else {

    'auto-ready'
}


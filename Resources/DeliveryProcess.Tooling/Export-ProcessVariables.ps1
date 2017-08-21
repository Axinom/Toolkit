# This script lets you easily move VSTS/TFS process variables from build to release in bulk.
# It publishes them in .txt files, which can be published as build artifacts then imported at release time.

[CmdletBinding()]
param(
    # Names of the VSTS process variables to export.
    [Parameter(Mandatory = $true)]
    [string[]]$names,

    # Directory where to export the variables.
    [Parameter(Mandatory = $true)]
    [string]$path
)

$ErrorActionPreference = "Stop"

# Ensure the directory exists.
New-Item -ItemType Directory -Path $path -ErrorAction Ignore | Out-Null

foreach ($name in $names) {
    $value = Get-ChildItem "env:$name"
    Set-Content -Path (Join-Path $path "$name.txt") -Value $value.Value
}
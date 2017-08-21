# This script lets you easily move VSTS/TFS process variables from build to release in bulk.
# It imports them from .txt files published by Export-ProcessVariables.

[CmdletBinding()]
param(
    # Directory where to import the variables from.
    # Every .txt file is assumed to be a variable to import.
    [Parameter(Mandatory = $true)]
    [string]$path
)

$ErrorActionPreference = "Stop"

foreach ($item in Get-ChildItem "$path\*.txt") {
    $value = Get-Content $item
    $name = [IO.Path]::GetFileNameWithoutExtension($item.Name)

    Write-Host "Importing $name"
    Write-Host "##vso[task.setvariable variable=$name;]$value"
}
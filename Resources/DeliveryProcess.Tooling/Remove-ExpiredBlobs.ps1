# This script cleans an Azure Storage container of blobs that were created "too long" ago.
#
# Requires AzureRM PowerShell module.

[CmdletBinding()]
param(
    # Name of the storage container to clean up.
    # You may also use a pattern like "*-stable"
    [Parameter(Mandatory = $True)]
    [string]$containerName,

    # Azure Storage connection string.
    [Parameter(Mandatory = $True)]
    [string]$connectionString,

    # How many days old blobs are allowed to be to survive.
    [Parameter(Mandatory = $True)]
    [int]$maxAgeDays
)

$ErrorActionPreference = "Stop"

$myDirectoryPath = $PSScriptRoot

if (!$myDirectoryPath) {
    $myDirectoryPath = "."
}

# Report tooling version just in case someone copies a log without more info.
& (Join-Path $myDirectoryPath "Get-ToolingVersion")

$azureContext = New-AzureStorageContext -ConnectionString $connectionString

# Ensure the container exists. This will fail if it does not exist.
Get-AzureStorageContainer $containerName -Context $azureContext | Out-Null

$cutoffMoment = (Get-Date).AddDays( - $maxAgeDays)
Write-Host "Removing blobs created before $($cutoffMoment.ToString(`"o`"))"

Get-AzureStorageContainer -Context $azureContext | ForEach-Object {
    if ($_.Name -notlike $containerName) {
        continue
    }

    Write-Host "Cleaning up $($_.Name)"

    $_ | Get-AzureStorageBlob | ForEach-Object {
        if ($_.LastModified -ge $cutoffMoment) {
            continue
        }

        Write-Host "Removing $($_.Name)"
        $_ | Remove-AzureStorageBlob
    }
}
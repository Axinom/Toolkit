<#
This script removes all Docker containers and any data and configuration associated with them.

The filesystem cleaning process is two-staged so as to preserve log files:
1) Anything other than the directories Infrastructure and Components is deleted from the directory /ComponentHost.
2) Anything other than the directory Logs is deleted from the directories Infrastructure and Components.
#>

[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

# Constants begin.
$componentHostPath = "/ComponentHost"
$componentHostDirectoriesToPreserve = @("Infrastructure", "Components")
# Constants end.

# Remove all containers.
if (docker ps -a -q) {
	Write-Host "Removing containers."
	docker rm -f (docker ps -a -q)
}

Write-Host "Removing Docker images."
docker image prune -a -f

Write-Host "Cleaning filesystem."

# If we have nothing there, we have nothing to delete there.
if (Test-Path $componentHostPath) {
	$componentHostItems = Get-ChildItem $componentHostPath
	$componentHostItemsToPreserve = $componentHostItems | Where-Object { $_.PSIsContainer -and ($componentHostDirectoriesToPreserve -contains $_.Name) }

	$componentHostItemsToDelete = @()
	$componentHostItemsToDelete += $componentHostItems | Where-Object { $componentHostItemsToPreserve -notcontains $_ }

	foreach ($item in $componentHostItemsToPreserve)
	{
		$componentHostItemsToDelete += Get-ChildItem $item.FullName | Where-Object { !($_.PSIsContainer -and $_.Name -eq "Logs") }
	}

	foreach ($item in $componentHostItemsToDelete)
	{
		Write-Output "Removing $($item.FullName)."
	
		Remove-Item -Recurse -Force $item.FullName
	}
}
else {
	Write-Verbose "$componentHostPath does not exist - skipping filesystem clean."
}

Write-Host "All deployed components have been removed."
Write-Host "All configuration used by deployed components has been removed."
Write-Host "All data stored by deployed components has been removed."
Write-Host "All loaded Docker images have been removed."
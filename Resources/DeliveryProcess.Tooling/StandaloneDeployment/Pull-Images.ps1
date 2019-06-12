[CmdletBinding()]
param(
	[Parameter(Mandatory = $true)]
	[string]$deploymentPackagePath,

	# Array of module names for which the images to pull. If not given, pulls images of all modules.
	[Parameter(Mandatory = $false)]
    [array]$moduleNames,

	[Parameter(Mandatory = $true)]
	[string]$registryUrl,
	
	[Parameter(Mandatory = $true)]
	[string]$registryUsername,

	[Parameter(Mandatory = $true)]
	[string]$registryPassword	
)

$ErrorActionPreference = "Stop"

. "$PSScriptRoot\Functions.ps1"

$assetsFolder = Join-Path $deploymentPackagePath "Assets"

Write-Host "Logging in to the registry."

$baseArgs = @()
$args = $baseArgs
$args += "login"
$args += "--username"
$args += $registryUsername
$args += "--password-stdin"
$args += $registryUrl
$registryPassword | & docker $args

if ($moduleNames) {
	$moduleFolders = Get-ChildItem "$assetsFolder/Modules" -Directory | Where-Object Name -In $moduleNames
	# Check if each module name provided to the script actually exists.
	foreach ($moduleName in $moduleNames) {
		if ($moduleName -notin $moduleFolders.Name) {
			Write-Error "You requested to pull images for module '$moduleName' but the package does not contain that module. It contains: $($moduleFolders.Name -join ", ")."
		}
	}
} else {
	$moduleFolders = Get-ChildItem "$assetsFolder/Modules" -Directory
}

$modules = Get-Modules -moduleFolders $moduleFolders

Write-Host "Starting pulling."

foreach ($moduleName in $modules.Keys) {
	foreach ($component in $modules[$moduleName]) {
		$image = $component.Image
		$imageFullName = "$($image.Registry)/$($image.Namespace)/$($image.Name):$($image.Version)"

        & docker pull $imageFullName
	
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Error pulling image $imageFullName"
        }
	}
}

Write-Host "Done"
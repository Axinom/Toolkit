[CmdletBinding()]
param(
	# Array of module names (separated by a comma) for which configuration should be validated. If no module names given, configuration of all modules is validated.
	[Parameter(Mandatory = $false)]
	[string]$moduleNames
)

$ErrorActionPreference = "Stop"

$assetsFolder = Join-Path $PSScriptRoot "Assets"
$configurationPath = Join-Path $PSScriptRoot "Configuration"
$deliveryProcessToolingPath = Join-Path $assetsFolder "DeliveryProcess.Tooling"
$generateConfiguration= Join-Path $deliveryProcessToolingPath "Generate-Configuration.ps1"

if ($moduleNames) {
	$moduleNamesArray = @($moduleNames.Split(";,".ToCharArray())) | ForEach-Object { $_.Trim() }
	$moduleFolders = Get-ChildItem "$assetsFolder/Modules" -Directory | Where-Object Name -In $moduleNamesArray
} else {
	$moduleFolders = Get-ChildItem "$assetsFolder/Modules" -Directory
}

foreach ($moduleFolder in $moduleFolders) {
	$templateFolders = Get-ChildItem $moduleFolder.FullName -Directory | Where-Object { $_.Name -like "ConfigurationTemplates*" }
	foreach ($templateFolderPath in $templateFolders) {
		$componentName = $templateFolderPath.Name.Split("-")[1];
		
		if (!$componentName) {
			$componentName = "app"
		}

		$moduleName = $moduleFolder.Name
		$output = Join-Path $moduleFolder.FullName "TransformedConfiguration/$componentName"
		& $generateConfiguration -values $configurationPath/$moduleName -templates $templateFolderPath.FullName -componentName $componentName -output $output -failIfNoMatch
	}
}

Write-Host "Configuration is valid."

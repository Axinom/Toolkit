[CmdletBinding()]
param(
	# Path to the package.json file to update.
	[Parameter(Mandatory = $True)]
	[string]$packageJsonPath,

	# The full TFS build number string of the build whose output we are publishing.
	[Parameter(Mandatory = $True)]
	[string]$buildNumber,
	
	# If set, marks the published version as a preview version.
	# Cannot be set together with stableVersion.
	# If neither is set, marks the published version as a CB version.
	[Parameter()]
	[switch]$previewVersion,
	
	# If set, marks the published version as a stable version.
	# Cannot be set together with previewVersion.
	# If neither is set, marks the published version as a CB version.
	[Parameter()]
	[switch]$stableVersion
)

$ErrorActionPreference = "Stop"

if ($previewVersion -and $stableVersion)
{
	Write-Error "Cannot set both previewVersion and stableVersion."
	return
}

# Expected input: 1.2.3-XXXXXX-YYYYYYY
$components = $buildNumber -split "-"

if ($components.Length -ne 3)
{
	Write-Error "buildNumber did not consist of the expected 3 components."
	return
}

if (!(Test-Path -PathType Leaf $packageJsonPath))
{
	Write-Error "Unable to find the package.json file at $packageJsonPath"
	return
}

$version = $components[0]

if ($stableVersion)
{
	# All good, that's enough.
}
else
{
	if ($previewVersion)
	{
		$version = $version + "-pre-"
	}
	else
	{
		$version = $version + "-cb-"
	}
	
	$version = $version + $components[1] + "-" + $components[2]
}

Write-Host "NPM package version is $version"

$json = Get-Content -Raw $packageJsonPath

$package = ConvertFrom-Json $json
$package.version = $version
$json = ConvertTo-Json $package

Set-Content -Path $packageJsonPath -Value $json

Write-Host "Finished updating the package.json file"
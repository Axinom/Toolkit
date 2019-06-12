[CmdletBinding()]
param(
	# Namespace of the repository/image. The microsoft in microsoft/iis.
	[Parameter(Mandatory = $True)]
	[string]$imageNamespace,

	# Name of the docker image to tag. The iis in microsoft/iis.
	[Parameter(Mandatory = $True)]
	[string]$imageName,

	# Version/tag to add more versions/tags to.
	[Parameter(Mandatory = $True)]
	[string]$imageVersion,

	# Version/tag to apply to the created image.
	# Multiple values may be specified (the image will be tagged with each).
	[Parameter(Mandatory = $True)]
	[string[]]$add,

	# URL of the registry. Leave empty to use Docker Hub.
	[Parameter(Mandatory = $False)]
	[string]$registryUrl,

	[Parameter(Mandatory = $True)]
	[string]$registryUsername,

	[Parameter(Mandatory = $True)]
	[string]$registryPassword,

	# Docker host that will execute the actual Docker logic. Optional (defaults to local server over pipe).
	[Parameter(Mandatory = $False)]
	[string]$dockerHost
)

$ErrorActionPreference = "Stop"

$myDirectoryPath = $PSScriptRoot

if (!$myDirectoryPath) {
    $myDirectoryPath = "."
}

# Report tooling version just in case someone copies a log without more info.
& (Join-Path $myDirectoryPath "Get-ToolingVersion")

$baseArgs = @()

if ($dockerHost -ne "")
{
	Write-Host "Using Docker host $dockerHost"

	$baseArgs += "--host"
	$baseArgs += $dockerHost
}

if ($registryUrl)
{
	$originalFullName = "$registryUrl/$imageNamespace/$($imageName):$imageVersion"
} else {
	$originalFullName = "$imageNamespace/$($imageName):$imageVersion"
}

# These are the full names to add
$fullNames = @()

# There may be one or more version strings to add
foreach ($imageVersionString in $add)
{
	if ($registryUrl)
	{
		$fullName = "$registryUrl/$imageNamespace/$($imageName):$imageVersionString"
	} else {
		$fullName = "$imageNamespace/$($imageName):$imageVersionString"
	}

	Write-Host "Image shall also be known as $fullName"

	$fullNames += $fullName
}

$args = $baseArgs
$args += "login"
$args += "--username"
$args += $registryUsername
$args += "--password-stdin"
$args += $registryUrl
$registryPassword | & docker $args

if ($LASTEXITCODE -ne 0)
{
	Write-Error "docker login failed with exit code $LASTEXITCODE"
}

Write-Host "Pulling $originalFullName"
$args = $baseArgs
$args += "pull"
$args += $originalFullName
& docker $args

if ($LASTEXITCODE -ne 0)
{
	Write-Error "docker pull failed with exit code $LASTEXITCODE"
}

foreach ($fullName in $fullNames)
{
	Write-Host "Tagging $fullName"
	$args = $baseArgs
	$args += "tag"
	$args += $originalFullName
	$args += $fullName
	& docker $args

	if ($LASTEXITCODE -ne 0)
	{
		Write-Error "docker tag failed with exit code $LASTEXITCODE"
	}

	Write-Host "Pushing $fullName"
	$args = $baseArgs
	$args += "push"
	$args += $fullName
	& docker $args

	if ($LASTEXITCODE -ne 0)
	{
		Write-Error "docker push failed with exit code $LASTEXITCODE"
	}

	# Do not keep garbage on the build machine!
	$args = $baseArgs
	$args += "rmi"
	$args += $fullName
	& docker $args

	if ($LASTEXITCODE -ne 0)
	{
		Write-Error "Removing temporary docker image failed with exit code $LASTEXITCODE"
	}
}

Write-Host "Docker image publishing completed."
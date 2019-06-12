[CmdletBinding()]
param(
	# Path to the dockerfile.
	[Parameter(Mandatory = $True)]
	[string]$dockerfile,

	# Root directory to operate in. Should generally be the solution root.
	[Parameter(Mandatory = $True)]
	[string]$buildContext,

	# Namespace of the repository/image. The microsoft in microsoft/iis.
	[Parameter(Mandatory = $True)]
	[string]$imageNamespace,

	# Name of the image to create. The iis in microsoft/iis.
	[Parameter(Mandatory = $True)]
	[string]$imageName,

	# Version/tag to apply to the created image.
	# Multiple values may be specified (the image will be tagged with each).
	[Parameter(Mandatory = $True)]
	[string[]]$imageVersion,

	# URL of the registry. Leave empty to use Docker Hub.
	[Parameter(Mandatory = $False)]
	[string]$registryUrl,

	[Parameter(Mandatory = $True)]
	[string]$registryUsername,

	[Parameter(Mandatory = $True)]
	[string]$registryPassword,

	# Docker host that will execute the actual Docker logic. Optional (defaults to local server over pipe).
	[Parameter(Mandatory = $False)]
	[string]$dockerHost,

	# Docker build arguments issued via the --build-arg command line option.
	[Parameter(Mandatory = $False)]
	[string]$buildArguments
)

$ErrorActionPreference = "Stop"

$myDirectoryPath = $PSScriptRoot

if (!$myDirectoryPath) {
    $myDirectoryPath = "."
}

# Report tooling version just in case someone copies a log without more info.
& (Join-Path $myDirectoryPath "Get-ToolingVersion")

if (!(Test-Path $buildContext))
{
	Write-Error "Directory does not exist: $buildContext"
}

if (!(Test-Path $dockerfile))
{
	Write-Error "Dockerfile does not exist: $dockerfile"
}

if ($imageVersion.Length -eq 0)
{
	Write-Error "At least one version string must be specified."
}

$baseArgs = @()

if ($dockerHost -ne "")
{
	Write-Host "Using Docker host $dockerHost"

	$baseArgs += "--host"
	$baseArgs += $dockerHost
}

# Switch to absolute paths just to be on the safe side with regard to working directory.
$buildContext = Resolve-Path $buildContext
$dockerfile = Resolve-Path $dockerfile

$fullNames = @()

# There may be one or more version strings
foreach ($imageVersionString in $imageVersion)
{
	if ($registryUrl)
	{
		$fullName = "$registryUrl/$imageNamespace/$($imageName):$imageVersionString"
	} else {
		$fullName = "$imageNamespace/$($imageName):$imageVersionString"
	}

	Write-Host "Docker image shall be identified as $fullName"

	$fullNames += $fullName
}

# Log in to registry first, so that any private base images are available.
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

# Build it!
$args = $baseArgs
$args += "build"

# Always pull any base image we reference, to ensure it is up to date.
$args += "--pull"

foreach ($fullName in $fullNames)
{
	$args += "--tag"
	$args += $fullName
}

$args += "--file"
$args += $dockerfile

# We filter out version strings that are known to be general purpose and do not signify a unique build.
# For now, this just means filter out "cb" as that gets onto every image created by Continuous Build.
# Hopefully there will not be other similar version strings that creep in here in the future.
$hopefullyUniqueVersions = @()

foreach ($imageVersionString in $imageVersion)
{
	if ($imageVersionString -eq "cb")
	{
		continue
	}

	$hopefullyUniqueVersions += $imageVersionString
}

# Just concatenate any remaining version strings together. This may include any number, depending on how the script is used.
# We optimize for the standard case of "cb" plus a meaningful version string but nobody says you can't differ from that.
$labelVersionString = [string]::Join(",", $hopefullyUniqueVersions)

$args += "--label"
$args += "version=`"$labelVersionString`""

# Without this, the intermediate container is left alive on failure.
$args += "--force-rm"

# Starting from delivery process v3, we always use the "axinom" custom network for all operations.
# If using an agent server that does not have an ASDP-compatible Docker installation, this will cause a failure.
$args += "--network"
$args += "axinom"

if ($buildArguments -ne "")
{
	Write-Host "Setting the following build arguments (--build-arg): $buildArguments"

	$args += "--build-arg"
	$args += $buildArguments
}

$args += $buildContext
& docker $args

if ($LASTEXITCODE -ne 0)
{
	Write-Error "docker build failed with exit code $LASTEXITCODE"
}

foreach ($fullName in $fullNames)
{
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
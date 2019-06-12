[CmdletBinding()]
param(
	# Array of image objects.
	[Parameter(Mandatory = $true)]
	[object]$images,

	[Parameter(Mandatory = $true)]
	[string]$registryUrl,
	
	[Parameter(Mandatory = $true)]
	[string]$registryUsername,

	[Parameter(Mandatory = $true)]
	[string]$registryPassword,

	[Parameter(Mandatory = $true)]
	[string]$tarPath,

	# If provided, the target folder will also contain the specified version of all infrastructure services.
	# Must be either "latest" or "cb".
	[Parameter(Mandatory = $false)]
	[string]$includeInfrastructureServicesVersion
)

$ErrorActionPreference = "Stop"

if (@("cb", "latest", "") -notcontains $includeInfrastructureServicesVersion) {
    Write-Error "Infrastructure services version must be either 'cb' or 'latest'."
    return
}

$imageFullNames = @($images | ForEach-Object { "$($_.Registry)/$($_.Namespace)/$($_.Name)`:$($_.Version)" })

if ($includeInfrastructureServicesVersion) {
	$imageFullNames += "registry.axinom.com/infrastructure/http-gateway-linux:$includeInfrastructureServicesVersion"
	$imageFullNames += "registry.axinom.com/infrastructure/deployment-agent-linux:$includeInfrastructureServicesVersion"
	$imageFullNames += "registry.axinom.com/infrastructure/dockermaintenance-linux:$includeInfrastructureServicesVersion"
	$imageFullNames += "registry.axinom.com/infrastructure/docker-stats-exporter-linux:$includeInfrastructureServicesVersion"
	$imageFullNames += "registry.axinom.com/sms/dashboard-linux:$includeInfrastructureServicesVersion"
}

Write-Host "The following images are going to be pulled:"

foreach ($image in $imageFullNames) {
	Write-Host $image
}

Write-Host "Logging in to the registry."

$baseArgs = @()
$args = $baseArgs
$args += "login"
$args += "--username"
$args += $registryUsername
$args += "--password-stdin"
$args += $registryUrl
$registryPassword | & docker $args

Write-Host "Starting pulling."

foreach ($image in $imageFullNames) {
	& docker pull $image
	
	if ($LASTEXITCODE -ne 0) {
		Write-Error "Error pulling image $image"
	}
}

Write-Host "Saving the images to a TAR file."

$parent = Split-Path -Parent $tarPath
if ($parent) {
	New-Item -ItemType Directory -ErrorAction SilentlyContinue -Force -Path $parent | Out-Null
}

& docker save -o $tarPath $imageFullNames

if ($LASTEXITCODE -ne 0) {
	Write-Error "Save failed!"
}

Write-Host "Done."
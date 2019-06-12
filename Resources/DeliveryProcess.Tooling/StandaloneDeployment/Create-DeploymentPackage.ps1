[CmdletBinding()]
param(
        # Path to DeliveryProcess.Tooling.
        [Parameter(Mandatory = $true)]
        [string]$tooling,
        
        # Script that will be executed after the deployment package has been generated, to have optional custom modifications applied to the package.
        [Parameter(Mandatory = $false)]
        [string]$postProcessScript,
        
        # Paths to modules to include in the package.
        [Parameter(Mandatory = $true)]
        [array]$modules,
        
        [Parameter(Mandatory = $false)]
        [string]$registryUrl,

        [Parameter(Mandatory = $false)]
        [string]$registryUsername,

        [Parameter(Mandatory = $false)]
		[string]$registryPassword,

        # Specifies the version of the infrastructure services to include in the images tar, if any.
		[ValidateSet("cb", "latest", "")] 
        [string]$includeInfrastructureServicesVersion,
        
        # URL to the SetupDocker zip file. If specified, will download and place unpacked SetupDocker in the package.
        [Parameter(Mandatory = $false)]
        [string]$setupDockerUrl,
        
        # URL to the SetupComponentHost zip file. If specified, will download and place unpacked SetupComponentHost in the package.
        [Parameter(Mandatory = $false)]
        [string]$setupComponentHostUrl,
        
        # If specified, will write a Release.txt file in Assets folder containing this string.
        [Parameter(Mandatory = $false)]
        [string]$packageVersion,
        
        # Folder that will contain the content of the deployment package in an unpacked form.
        # The folder will be created if it doesn't exist.
        # The whole content of this folder will be cleared if it already exists.
        # It does not have any effect if deploymentPackageTar is specified.
        [Parameter(Mandatory = $false)]
        [string]$output,

        # Path to the deployment package tar file that would be created if specified.
        [Parameter(Mandatory = $false)]
        [string]$deploymentPackageTar,

        # Path to an images tar file that would be created if specified.
        [Parameter(Mandatory = $false)]
		[string]$imagesTar
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

. "$PSScriptRoot\Functions.ps1"

# Returns an array of Image objects from given modules (returned from Get-Modules function).
function Get-AllImages($modules) {
	$images = @()

	foreach ($moduleName in $modules.Keys) {
		foreach ($component in $modules[$moduleName]) {
			$images += $component.Image
		}
	}

	return $images
}

# Enable the use of patterns in paths.
$modules = Get-Item $modules

function Remove-OutputFolder {
	if (Test-path $output -PathType Container) {
        Remove-Item $output -Force -Recurse
    }
}

if (!$output -and !$deploymentPackageTar) {
	Write-Error "Either -output or -deploymentPackageTar must be specified."
}

trap {
    if ($deploymentPackageTar) {
		Remove-OutputFolder
	}
}

# In case the tar is requested, let's still create the unpacked form of the deployment package but with a random folder name.
# It will be deleted afterwards.
if ($deploymentPackageTar) {
    $output = New-Guid
}

Remove-OutputFolder

New-Item $output -ItemType Directory | Out-Null

# Copy the main user facing files from tooling.
$mainUserFacingScripts = @("Install-Components.ps1", "Remove-Components.ps1", "Validate-Configuration.ps1", "Functions.ps1")
$mainUserFacingScripts | ForEach-Object {
	Copy-Item -Path "$tooling/StandaloneDeployment/$_" -Destination $output
}

New-Item -Path $output/Configuration -ItemType Directory | Out-Null

New-Item -Path $output/Assets -ItemType Directory | Out-Null

Copy-Item $tooling -Destination $output/Assets/DeliveryProcess.Tooling/ -Container -Recurse

foreach ($sourceModulePath in $modules) {
	if (!(Test-path $sourceModulePath -PathType Container)) {
		Write-Error "'$sourceModulePath' not found."
	}

	# Convert string to a Path object.
	$sourceModulePath = Get-Item $sourceModulePath

	$destinationModuleFolderName = $sourceModulePath.Name.Split("-")[-1] # Everything before the last hyphen is ignored.
	$destinationModuleFolderPath = "$output/Assets/Modules/$destinationModuleFolderName"

	Copy-Item $sourceModulePath -Destination $destinationModuleFolderPath -Recurse -Container

	# Copy the user facing files of module to root.
	$userFacingScriptsPath = Join-Path $destinationModuleFolderPath "UserFacingScripts"

	if (Test-Path $userFacingScriptsPath) {
		Copy-Item "$userFacingScriptsPath/*" -Destination $output -ErrorAction SilentlyContinue
		Remove-Item -Path $userFacingScriptsPath -Recurse
	}

	# Move the configuration of components into Configuration.
	$configurationFolders = Get-ChildItem $destinationModuleFolderPath -Directory | Where-Object {$_.Name -like "ConfigurationSample*"}
	foreach ($configurationFolder in $configurationFolders) {
		$componentName = $configurationFolder.Name.Split("-")[1]

		if (!$componentName) {
			$componentName = "app"
		}
		
		$moduleName = (Get-Item $destinationModuleFolderPath).Name
		$destinationConfigurationFolderPath = "$output/Configuration/$moduleName/$componentName"
		New-Item $destinationConfigurationFolderPath -ItemType Directory | Out-Null
		Move-Item -Path "$($configurationFolder.FullName)/*" -Destination $destinationConfigurationFolderPath
		Remove-Item -Path $configurationFolder.FullName -Recurse
	}
}

if ($setupDockerUrl) {
	Write-Host "Downloading SetupDocker..."
	
	Invoke-WebRequest -Uri "$setupDockerUrl" -OutFile SetupDocker.zip
	Expand-Archive -Path SetupDocker.zip -DestinationPath "$output/SetupDocker/"
	Remove-Item -Path SetupDocker.zip	
}

if ($setupComponentHostUrl) {
	Write-Host "Downloading SetupComponentHost..."

	Invoke-WebRequest -Uri "$setupComponentHostUrl" -OutFile SetupComponentHost.zip
	Expand-Archive -Path SetupComponentHost.zip -DestinationPath "$output/SetupComponentHost/"
	Remove-Item -Path SetupComponentHost.zip
}

$releaseFile = Join-Path $output "Assets/Release.txt"
"Package version: $packageVersion" | Out-File $releaseFile
if ($includeInfrastructureServicesVersion) {
	"Release type of delivery process and infrastructure services: $includeInfrastructureServicesVersion" | Out-File $releaseFile -Append
}

$moduleFolders = Get-ChildItem -Path "$output/Assets/Modules" -Directory
$allModules = Get-Modules -moduleFolders @($moduleFolders | ForEach-Object { $_.FullName })

# Execute post-process script.
if ($postProcessScript) {
	if (!(Test-Path $postProcessScript)) {
		Write-Error "$postProcessScript not found."
	}
	& $postProcessScript -deploymentPackagePath $output -toolingPath $tooling -modules $allModules
}

function Create-ChecksumFile($from){
	if (!(Test-Path $from)) {
		Write-Error "Cannot create checksum. $from does not exist."
	}
	
	$from = Get-Item $from
	
	$hashFileName = "$($from.Name).sha256sum"
	
	$fileHash = (Get-FileHash -Algorithm SHA256 $from.FullName).Hash
	
	# There is a double-space between the file hash and the file name.
	# This is important as it is expected by the Linux sha256sum tool.
	$formattedFileHash = "$fileHash  $($from.Name)"
	
	$folderWhereToCreateHashFile = Split-Path $from

	$formattedFileHash | Out-File (Join-Path $folderWhereToCreateHashFile $hashFileName)
}

# Create deployment package tar.
if ($deploymentPackageTar) {
    if (!(Get-Command tar -ErrorAction SilentlyContinue)) {
        Write-Error "You have requested a deployment package tar to be created but you are missing tar utility."
    }

    $deploymentPackageTarParent = Split-Path $deploymentPackageTar
    if ($deploymentPackageTarParent) {
        New-Item -Path $deploymentPackageTarParent -ItemType Directory -Force
    }

    $absolutePathToDeploymentPackageTar = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($deploymentPackageTar)

    try {
        Push-Location
        Set-Location -Path $output
        & tar -cf $absolutePathToDeploymentPackageTar *
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Failed to pack the deployment package into a tar file."
        }
    } finally {
        Pop-Location
    }

    Create-ChecksumFile -from $deploymentPackageTar
}

if ($deploymentPackageTar) {
	Write-Host "Deployment package tar created at: $(Resolve-Path -Path $deploymentPackageTar)"
} else {
	Write-Host "Deployment package created at: $(Resolve-Path -Path $output)"
}

# Create images tar.
if ($imagesTar) {
	$images = Get-AllImages -modules $allModules
	& (Join-Path $tooling "StandaloneDeployment/Save-Images.ps1") -images $images -registryUrl $registryUrl -registryUsername $registryUsername -registryPassword $registryPassword -includeInfrastructureServicesVersion $includeInfrastructureServicesVersion -tarPath $imagesTar
	Create-ChecksumFile -from $imagesTar

	Write-Host "Images tar created at: $(Resolve-Path -Path $imagesTar)"
}

if ($deploymentPackageTar) {
	Remove-OutputFolder # No need for the unpacked form of the deployment package.
}
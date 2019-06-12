[CmdletBinding()]
param(
	# Array of module names (separated by a comma) to install from the package. If not provided, all modules are going to be installed.
	[Parameter(Mandatory = $false)]
	[string]$moduleNames,
	
	[Parameter(Mandatory = $false)]
	[string]$unmanagedComponentsRoot = "/opt/axinom",

	# Whether to remove unmanaged components not included in the deployment package before installing the new ones.
	[switch]$removeUnknownComponents
)

$ErrorActionPreference = "Stop"

. "$PSScriptRoot\Functions.ps1"

function Install-ManagedComponent($moduleName, $component)
{
	$image = $component.Image

	if (!$component.Name) {
		Write-Error "Will not install image '$($image.Name)' as a managed component because it does not have a Name attribute set."
	}

	$componentConfigurationPackagePath = ""

	if ($component.AssetsKey) {
		$componentConfigurationPackagePath = Join-Path $transformedConfigurationRoot $component.AssetsKey
		if (!(Test-Path $componentConfigurationPackagePath)) {
			$componentConfigurationPackagePath = ""
		}
	}

	$componentName = $moduleName.ToLower() + "-" + $component.Name

	Write-Host "Attempting to deploy a managed component '$componentName'."

	& $deployComponent -imageNamespace $image.Namespace `
						-imageName $image.Name `
						-imageVersion $image.Version `
						-namespace "managed" `
						-name $componentName `
						-configurationPackagePath $componentConfigurationPackagePath `
						-dataVolumeNames $component.DataVolumes `
						-hasChecks:$component.HasChecks `
						-grpc:$component.EnableGrpc `
						-httpBehavior "Allow" `
						-quickPublish:$component.EnableQuickPublish `
						-hostnames $component.Hostnames `
						-server "localhost" `
						-registryUrl $image.Registry `
						-delayBetweenServers 0 `
						-timeoutWaitingForGreenDashboard 0 `
						-privileged:$component.Privileged
}

function Remove-UnknownUnmanagedComponents {
	# Every component must have a configuration folder on the host. Use this fact to know which components are already installed.
	$configFolders = Get-ChildItem -Path "$unmanagedComponentsRoot/components/config/" -Directory -ErrorAction Ignore
	foreach ($configFolder in $configFolders) {
		$installedModuleName = $configFolder.Name.Split("-")[0]
		$installedComponentName = $configFolder.Name.Split("-")[1]
		$installedContainerName = $configFolder.Name.ToLower()

		$moduleExistsInPackage = $modules.ContainsKey($installedModuleName)

		if (!$moduleExistsInPackage) {
			Write-Host "Uninstalling $installedModuleName-$installedComponentName"

			try {
				& docker rm -f $installedContainerName 2>&1 | Out-Null
			} catch {}

			Remove-Item -Path "$unmanagedComponentsRoot/components/config/$($configFolder.Name)/" -Recurse -Force -ErrorAction Ignore 2>&1 | Out-Null
		} else {
			# If the module exists in the package, check if the component also exists. If not, uninstall it.
			$componentInPackage = $modules[$installedModuleName] | Where-Object Name -eq $installedComponentName

			if ($componentInPackage.Count -eq 0) {
				Write-Host "Installed component $installedModuleName-$installedComponentName is not found from package. Uninstalling."
				
				try {
					& docker rm -f $installedContainerName 2>&1 | Out-Null
				} catch {}

				Remove-Item -Path "$unmanagedComponentsRoot/components/config/$($configFolder.Name)/" -Recurse -Force -ErrorAction Ignore 2>&1 | Out-Null
			}
		}
	}
}

# Executes an external command and returns its stderr and stdout in an object.
function Execute-Command ($command, $commandArguments, $workingDirectory)
{
    try {
        $pinfo = New-Object System.Diagnostics.ProcessStartInfo
        $pinfo.FileName = $command
        $pinfo.RedirectStandardError = $true
        $pinfo.RedirectStandardOutput = $true
        $pinfo.UseShellExecute = $false
        $pinfo.WindowStyle = 'Hidden'
        $pinfo.CreateNoWindow = $True
		$pinfo.Arguments = $commandArguments
		$pinfo.WorkingDirectory = $workingDirectory
		$p = New-Object System.Diagnostics.Process
        $p.StartInfo = $pinfo
        $p.Start() | Out-Null
        $stdout = $p.StandardOutput.ReadToEnd()
        $stderr = $p.StandardError.ReadToEnd()
        $p.WaitForExit()
        $p | Add-Member "stdout" $stdout
        $p | Add-Member "stderr" $stderr
    } catch {
	}
	
    return $p
}

function Duplicate-Files(
    [string]$folder,
    [string]$sourceFilename,
    [string]$destinationFilename
)
{
	$sourceFiles = Get-ChildItem -Path $folder -Name $sourceFilename -Recurse -File

    foreach ($sourceFile in $sourceFiles) {
    	Copy-Item $folder/$sourceFile -Destination $folder/$destinationFilename
    }
}

function Get-FullImageName($image) {
	$fullImageName = ""

	if ($image.Registry) {
		$fullImageName += $image.Registry + "/"
	}

	if ($image.Namespace) {
		$fullImageName += $image.Namespace + "/"
	}

	$fullImageName += $image.Name + ":" + $image.Version

	return $fullImageName
}

function Replace-ModuleTokens($moduleFolder, $components) {
	Duplicate-Files -folder $moduleFolder -sourceFilename "docker-compose.yml" -destinationFilename "docker-compose-transformed.yml"

	$moduleName = (Get-Item $moduleFolder).Name

	$variables = @(
		"data_root=$unmanagedComponentsRoot/components/data",
		"network_name=axinom-unmanaged"
	)

	foreach ($component in $components) {
		$image = $component.Image
		$fullImageNameForThisComponent = Get-FullImageName -image $image

		$componentName = $component.Name

		if ($componentName -eq "app") {
			$variables += "image=$fullImageNameForThisComponent"
			$variables += "container_name=$($moduleName.ToLower())-app"
			$variables += "config_root=$unmanagedComponentsRoot/components/config/$moduleName-app"
			$variables += "logs_root=$unmanagedComponentsRoot/components/logs/$moduleName-app"

			$variables += "app_image=$fullImageNameForThisComponent"
			$variables += "app_container_name=$($moduleName.ToLower())-app"
			$variables += "app_config_root=$unmanagedComponentsRoot/components/config/$moduleName-app"
			$variables += "app_logs_root=$unmanagedComponentsRoot/components/logs/$moduleName-app"
		} else {
			$variables += "$componentName`_image=$fullImageNameForThisComponent"
			$variables += "$componentName`_container_name=$($moduleName.ToLower())-$componentName"
			$variables += "$componentName`_config_root=$unmanagedComponentsRoot/components/config/$moduleName-$componentName"
			$variables += "$componentName`_logs_root=$unmanagedComponentsRoot/components/logs/$moduleName-$componentName"
		}
	}

	& $replaceTokens -path $moduleFolder -filenames "docker-compose-transformed.yml" -recursive -variables $variables 6>$null
}

$assetsFolder = Join-Path $PSScriptRoot "Assets"
$deliveryProcessToolingPath = Join-Path $PSScriptRoot "Assets/DeliveryProcess.Tooling"
$deployComponent = Join-Path $deliveryProcessToolingPath "Deploy-Component.ps1"
$replaceTokens = Join-Path $deliveryProcessToolingPath "Replace-Tokens.ps1"

if ($moduleNames) {
	$moduleNamesArray = @($moduleNames.Split(";,".ToCharArray())) | ForEach-Object { $_.Trim() }
	$moduleFoldersToInstall = Get-ChildItem "$assetsFolder/Modules" -Directory | Where-Object Name -In $moduleNamesArray

	# Check if each module name provided to the script actually exists.
	$allModuleFolders = Get-ChildItem "$assetsFolder/Modules" -Directory
	foreach ($moduleName in $moduleNamesArray) {
		if ($moduleName -notin $moduleFoldersToInstall.Name) {
			Write-Error "You requested to install module '$moduleName' but the package does not contain it. It contains: $($allModuleFolders.Name -join ", ")."
		}
	}
} else {
	# By default, install all modules.
	$moduleFoldersToInstall = Get-ChildItem "$assetsFolder/Modules" -Directory
}

$modules = Get-Modules -moduleFolders $moduleFoldersToInstall

# Make sure the configuration has been generated (and is valid).
& (Join-Path $PSScriptRoot "Validate-Configuration.ps1") -moduleNames $moduleNames 6>$null

# Check if all the images exist in the local system.
foreach ($moduleName in $modules.Keys) {
	foreach ($component in $modules[$moduleName]) {
		$image = $component.Image
		$imageFullName = "$($image.Registry)/$($image.Namespace)/$($image.Name):$($image.Version)"

		& docker inspect --type image $imageFullName | Out-Null

		if ($LASTEXITCODE -ne 0) {
			Write-Error "The image $imageFullName in module $moduleName does not exist in the local system."
		}
	}
}

if ($removeUnknownComponents) {
	Remove-UnknownUnmanagedComponents
}

# Uninstall all unmanaged components from the modules in the package.
foreach ($moduleName in $modules.Keys) {
	$componentsInModule = $modules[$moduleName]
	foreach ($component in ($componentsInModule | Where-Object Managed -eq $false)) {
		$fullComponentName = "$($moduleName.ToLower())-$($component.Name)"
		try {
			& docker rm -f $fullComponentName 2>&1 | Out-Null
		} catch {}
	}
}

New-Item -Path "$unmanagedComponentsRoot/components/data/" -ItemType Directory -Force | Out-Null

foreach ($moduleName in $modules.Keys) {
	Write-Host "Deploying '$moduleName' module."

	$moduleFolder = Join-Path $assetsFolder "Modules/$moduleName"

	$moduleComponents = $modules[$moduleName]

	Replace-ModuleTokens -moduleFolder $moduleFolder -components $moduleComponents

	$transformedConfigurationRoot = Join-Path $moduleFolder "TransformedConfiguration"

	$preInstallStepsFile = Join-Path $moduleFolder "Execute-PreInstallSteps.ps1"

	if (Test-Path $preInstallStepsFile) {
		Write-Host "Executing pre-install steps from file: $preInstallStepsFile"
		& $preInstallStepsFile -components $moduleComponents -transformedConfigurationPath $transformedConfigurationRoot -toolingPath $deliveryProcessToolingPath -unmanagedConfigPath "$unmanagedComponentsRoot/components/config" -unmanagedDataPath "$unmanagedComponentsRoot/components/data"
	}

	# Install managed components.
	foreach ($component in $moduleComponents) {
		if ($component.Managed -ne $false) { # Consider all components to be managed, if not explicitly specified otherwise.
			Install-ManagedComponent -moduleName $moduleName -component $component
		}
	}

	# Copy transformed configuration files of all unmanaged components in this module into the central location.
	foreach ($component in ($moduleComponents | Where-Object Managed -eq $false)) {
		$componentName = $component.Name
		if (Test-Path -Path $transformedConfigurationRoot/$componentName) {
			Write-Verbose "Copying transformed configuration of $moduleName-$componentName."
			$configurationFolderOnHost = "$unmanagedComponentsRoot/components/config/$moduleName-$componentName/"
			New-Item -Path $configurationFolderOnHost -ItemType Directory -Force | Out-Null
			Copy-Item "$transformedConfigurationRoot/$componentName/*" $configurationFolderOnHost -Recurse -Force
			New-Item -Path "$unmanagedComponentsRoot/components/logs/$moduleName-$componentName/" -ItemType Directory -Force | Out-Null
		} else {
			Write-Verbose "$moduleName-$componentName does not have configuration."
		}
	}

    # Execute docker-compose.yml if it exists.
    if (Test-Path $moduleFolder/docker-compose.yml) {
        Write-Host "Executing all components defined in docker-compose.yml."

		# Have to execute it in such way because docker-compose writes normal operational logs to stderr. And there are more reasons.
		$result = Execute-Command -command "docker-compose" -commandArguments "-f docker-compose-transformed.yml up -d --force-recreate --remove-orphans" -workingDirectory $moduleFolder

		Write-Host $result.stdout
		Write-Host $result.stderr

		if ($result.ExitCode -ne 0) {
			Write-Host "Installing components failed. Here is the transformed docker-compose.yml file:"
			Get-Content -Path $moduleFolder/docker-compose-transformed.yml
			exit $result.ExitCode
		}
    }

	$postInstallStepsFile = Join-Path $moduleFolder "Execute-PostInstallSteps.ps1"

	if (Test-Path $postInstallStepsFile) {
		Write-Host "Executing post-install steps."
		& $postInstallStepsFile -components $moduleComponents -transformedConfigurationPath $transformedConfigurationRoot -toolingPath $deliveryProcessToolingPath -unmanagedConfigPath "$unmanagedComponentsRoot/components/config" -unmanagedDataPath "$unmanagedComponentsRoot/components/data"
	}

	Write-Host "Done with the '$moduleName' module."
}

Write-Host "All components have been started."

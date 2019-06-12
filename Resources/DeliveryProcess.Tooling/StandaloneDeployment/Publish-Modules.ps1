# This script takes in module folders and optionally additional full image names and pushes them into a specific namespace in the registry.
# It also updates the namespace references in the module specifications accordingly.

[CmdletBinding()]
param(
    # Paths to modules.
    [Parameter(Mandatory = $false)]
    [array]$modules,

    # Additoinal images as objects.
    [Parameter(Mandatory = $false)]
    [object[]]$images,

    # Namespace where to copy the images to.
    [string]$targetNamespace,

	[Parameter(Mandatory = $true)]
	[string]$registryUrl,
	
	[Parameter(Mandatory = $true)]
	[string]$registryUsername,

	[Parameter(Mandatory = $true)]
    [string]$registryPassword
)

$ErrorActionPreference = "Stop"

. "$PSScriptRoot\Functions.ps1"

if ($null -eq $modules -and $null -eq $images) {
    Write-Error "No module folders or image names specified."
}

function Get-FullImageName([Parameter(ValueFromPipeline)]$image) {
	Process {
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
}

function Push-ImageToNewLocation($image) {
    $fullImageName = Get-FullImageName -image $image

    & docker pull $fullImageName
	
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Error pulling image $fullImageName"
    }

    $imageWithNewNamespace = $image
    $imageWithNewNamespace.Namespace = $targetNamespace
    $newImageName = Get-FullImageName -image $imageWithNewNamespace

    & docker tag $fullImageName $newImageName

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Error tagging image $fullImageName"
    }

    & docker push $newImageName

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Error pushing image $newImageName"
    }

    & docker rmi $fullImageName

    if ($newImageName -ne $fullImageName) {
        & docker rmi $newImageName
    }
}

function LoginTo-Registry($registryUrl, $registryUsername, $registryPassword) {
    $baseArgs = @()
    $args = $baseArgs
    $args += "login"
    $args += "--username"
    $args += $registryUsername
    $args += "--password-stdin"
    $args += $registryUrl
    $registryPassword | & docker $args    
}

function Write-Components($components, $moduleFolder) {
    $components | ConvertTo-Json -Depth 100 | Out-File (Join-Path $moduleFolder "Module.json")
}

if ($modules) {
    $moduleFolders = Get-Item $modules
}

Write-Host "Logging in to the registry."

LoginTo-Registry -registryUrl $registryUrl -registryUsername $registryUsername -registryPassword $registryPassword

foreach ($moduleFolder in $moduleFolders) {
    Write-Host "Publishing module $moduleFolder"

    $components = Get-Components -moduleFolder $moduleFolder

    foreach ($component in $components) {
        $image = $component.Image
        Push-ImageToNewLocation -image $image

        $image.Namespace = $targetNamespace
    }

    Write-Components -components $components -moduleFolder $moduleFolder.FullName
}

foreach ($image in $images) {
    Push-ImageToNewLocation -image $image
}

Write-Host "Done."
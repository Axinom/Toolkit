[CmdletBinding()]
param(
    # Namespace of the component on the server. Any server assets are unique in each namespace and isolated from other namespaces.
    [Parameter(Mandatory = $True)]
    [string]$namespace,

    # Name of the component on the server. If a component with this name already exists in the same namespace, it is replaced.
    [Parameter(Mandatory = $True)]
    [string]$name,

    # Namespace of the repository/image. The microsoft in microsoft/iis.
    [Parameter(Mandatory = $True)]
    [string]$imageNamespace,

    # Name of the docker image to deploy. The iis in microsoft/iis.
    [Parameter(Mandatory = $True)]
    [string]$imageName,

    # Version to deploy.
    [Parameter(Mandatory = $True)]
    [string]$imageVersion,

    # URL of the registry. Leave empty to use Docker Hub.
    [Parameter(Mandatory = $False)]
    [string]$registryUrl,

    # If not provided, will try to use anonymous access.
    [Parameter(Mandatory = $False)]
    [string]$registryUsername,

    # If not provided, will try to use anonymous access.
    [Parameter(Mandatory = $False)]
    [string]$registryPassword,

    # Hostnames or IP addresses of the servers to deploy the image to. The servers must be configured according
    # to the requirements of the Axinom Service Delivery Process. Arbitrary Docker hosts are not supported.
    # If you specify multiple servers, the deploy will be performed to each in sequence, aborting on first failure.
    [Parameter(Mandatory = $True)]
    [Alias("Server")]
    [string[]]$servers,

    # Path to the contents of the configuration package (a directory).
    # If not provided, the component will not be provided any configuration.
    [Parameter(Mandatory = $False)]
    [string]$configurationPackagePath,

    # Semicolon-separated list of hostnames to assign to the component.
    [Parameter(Mandatory = $False)]
    [string]$hostnames,

    # Semicolon-separated list with the names of data volumes to attach.
    # Data volumes are shared between components in the same namespace on the same host.
    [Parameter(Mandatory = $False)]
    [string]$dataVolumeNames,

    # HTTP behavior for the component. Allowed values: Allow, RedirectToHttps, Block.
    [Parameter(Mandatory = $False)]
    [string]$httpBehavior = "Allow",

    # Entry point arguments to provide to the container's entry point executable.
    # This is the full string, already correctly escaped and whatnot. No processing is done on it.
    # Primarily designed for easy compatibility with 3rd party images that use arguments for input.
    #
    # Example value: --a --b "c d e"
    # Executed command: entrypoint.exe --a --b "c d e"
    [Parameter()]
    [string]$entrypointArguments,

    # If true, the delivery process infrastructure will expect this Component to expose a Service Monitoring System
    # compatible checker API using the base URL /api/. The checks from this checker will be exposed in the automatically
    # managed dashboard of the component host, accessible on port 6810.
    [Parameter()]
    [switch]$hasChecks = $false,

    # Timeout in seconds. Per-server.
    [Parameter(Mandatory = $False)]
	[int]$timeout = 1800,

	# Seconds to wait between servers. This lets you ensure that your load balancer notices that a server came
	# up before deployment takes down the next server. Might not be necessary if you have sufficiently many serevrs.
    [Parameter(Mandatory = $False)]
    [int]$delayBetweenServers = 0
)

$ErrorActionPreference = "Stop"

# Each individual request should be very fast, so don't expect no funny business here.
$webRequestTimeoutInSeconds = 60


$myDirectoryPath = $PSScriptRoot

if (!$myDirectoryPath) {
    $myDirectoryPath = "."
}

# Report tooling version just in case someone copies a deployment log without more info.
& (Join-Path $myDirectoryPath "Get-ToolingVersion")

$awaitTaskPath = Join-Path $myDirectoryPath 'Internal-Await-Task.ps1'

if ($registryUrl) {
    $imageFullName = "$registryUrl/$imageNamespace/$($imageName):$imageVersion"
}
else {
    $imageFullName = "$imageNamespace/$($imageName):$imageVersion"
}

Write-Host "Image full name is $imageFullName"

$hasConfigurationPackage = $False

if ($configurationPackagePath) {
    $hasConfigurationPackage = $True

    if (!(Test-Path -PathType Container $configurationPackagePath)) {
        Write-Error "Configuration package directory does not exist."
        return
    }

    # Make absolute, as we will give it over to .NET classes.
    $configurationPackagePath = Resolve-Path $configurationPackagePath
}

# ZipFile comes from here.
Add-Type -AssemblyName System.IO.Compression.FileSystem

# Prepare configuration package
$tempZipFile = (Join-Path $([IO.Path]::GetTempPath()) $(New-Guid)) + ".zip"

try {
    $configurationPackageEncoded = $null

    if ($hasConfigurationPackage) {
        Write-Host "Packaging configuration files"
        [IO.Compression.ZipFile]::CreateFromDirectory($configurationPackagePath, $tempZipFile)

        $configurationPackageEncoded = [Convert]::ToBase64String([IO.File]::ReadAllBytes($tempZipFile))

        if ($configurationPackageEncoded.Length -gt 1024 * 1024 * 2) {
            Write-Error "The configuration package is too large to be submitted. Review the contents and ensure that you did not accidentally include any large data files."
            return
        }
    }

    # Create the component configuration.
    if ($dataVolumeNames) {
        $dataVolumes = @($dataVolumeNames.Split(";,") | ForEach-Object { @{ Name = $_ } })
    }

    if ($hostnames) {
        $hosts = @($hostnames.Split(";,") | ForEach-Object { @{ Name = $_ } })
    }

    if (-not $httpBehavior) {
        # The deployment agent expects it to have a value even if the component does not accept HTTP(S) connections.
        $httpBehavior = "Allow"
    }

    $componentConfiguration = @{
        ImageFullName        = $imageFullName
        ConfigurationPackage = $configurationPackageEncoded
        Hostnames            = $hosts
        DataVolumes          = $dataVolumes
        RegistryUsername     = $registryUsername
        RegistryPassword     = $registryPassword
        HttpBehavior         = $httpBehavior
        HasChecks            = $hasChecks.IsPresent
        EntrypointArguments  = $entrypointArguments
    }

    # Contains secrets. Do not print this out.
    $requestBody = ConvertTo-Json $componentConfiguration

    $lengthMegabytes = ($requestBody.Length / 1024.0 / 1024.0).ToString("F2")
    Write-Host "Task size is $lengthMegabytes MB."


    $serverCount = $servers.Length
    $serverNumber = 1

    # Deploy to all the servers.
    foreach ($server in $servers) {
        $port = 6742

        # Server may contain a port to override the default one (e.g. in special development environment).
        if ($server.Contains(":")) {
            $serverParts = $server -split ":"
            $server = $serverParts[0]
            $port = $serverParts[1]
        }

        Write-Host "Preparing deployment to $server`:$port ($serverNumber of $serverCount)"

        if ($serverNumber -ne 1 -and $delayBetweenServers -ne 0) {
            Write-Host "Will wait $delayBetweenServers seconds before continuing."
			Start-Sleep $delayBetweenServers
		}

        $startUrl = "http://$server`:$port/api/components/$namespace/$name`?timeoutInSeconds=$timeout"
        Write-Host "API URL is $startUrl"

        Write-Host "Starting deployment task."
        $startResponse = Invoke-WebRequest -Uri $startUrl -Method Put -TimeoutSec $webRequestTimeoutInSeconds -ContentType "application/json" -Body $requestBody -UseBasicParsing

        $startResponseObject = ConvertFrom-Json $startResponse.Content
        $taskId = $startResponseObject.TaskId

        Write-Host "Waiting for completion of task $taskId"

        $pollUrl = "http://$server`:$port/api/tasks/$taskId"

        & $awaitTaskPath -url $pollUrl -timeout $timeout

        $serverNumber++
    }
}
finally {
    if ($hasConfigurationPackage) {
        Remove-Item $tempZipFile -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
    }
}

Write-Host "All done."
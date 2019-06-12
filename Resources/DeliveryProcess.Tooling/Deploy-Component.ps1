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
    [int]$delayBetweenServers = 0,

    # Maximum number of seconds to wait for the dashboard to become green after deployment. Might be useful in multi-server
    # deployments if the component has a check that fails until it becomes usable. Value "0" indicates no need to check the
    # dashboard at all (in case there is some other component in a failed state).
    [Parameter(Mandatory = $False)]
    [int]$timeoutWaitingForGreenDashboard = 0,

    # Comma-separated list of ports on the host to be assigned to ports in the component.
    # Syntax for each item is: protocol:external[:internal]
    # Protocol is tcp or udp. Internal port number may be omitted if it is the same as external one.
    # Example: -portAssignments "tcp:1234,udp:2345,tcp:4567:5678"
    [Parameter(Mandatory = $False)]
    [string]$portAssignments,

    # If provided, the component's HTTP port 80 endpoint will be published under http://hostname:80/namespace-name/
    # and allows the same hostname to be shared by other QuickPublish components. This is a convenience feature
    # that enables components to be published without having to dedicate a DNS entry to each one.
    #
    # If no hostname is configured, the component is published when the request does not match any configured hostname.
    # This routing only works with HTTP - to QuickPublish over HTTPS, you must configure the hostname.
    #
    # The same hostname cannot be shared by QuickPublish and non-QuickPublish components.
    #
    # HTTPS is supported (activated if HTTPS certificates for the hostname are present in gateway configuration).
    [Parameter()]
    [switch]$quickPublish,

    # If provided, unencrypted gRPC requests to the component host port 82 will be routed to this component if they
    # have an :authority field matching one of -hostnames or, if -quickPublish is used, the "namespace-name" string.
    #
    # HTTPS is not supported with gRPC routing.
    # If QuickPublish is used with gRPC, hostnames cannot be specified (implementation limitation - poke maintainers).
    [Parameter()]
    [switch]$grpc,

    # If provided, the component will be executed in privileged mode.
    [Parameter()]
    [switch]$privileged,

    # Specifies the startup strategy for the component.
    # Immediately - the component will be immediately started after deployment. (Default action.)
    # OnNextBoot - the component will not be started after deployment. It will be started after the component host is booted.
    [Parameter()]
    [ValidateSet("Immediately", "OnNextBoot", "")]
    [string]$start
)

$ErrorActionPreference = "Stop"
$ProgressPreference = 'SilentlyContinue'

# Each individual request should be very fast, so don't expect no funny business here.
$webRequestTimeoutInSeconds = 60


$myDirectoryPath = $PSScriptRoot

if (!$myDirectoryPath) {
    $myDirectoryPath = "."
}

# Report tooling version just in case someone copies a deployment log without more info.
& (Join-Path $myDirectoryPath "Get-ToolingVersion")

. (Join-Path $myDirectoryPath "Functions.ps1")

$awaitTaskPath = Join-Path $myDirectoryPath 'Internal-Await-Task.ps1'

$awaitGreenDashboardpath = Join-Path $myDirectoryPath 'Internal-Wait-GreenDashboard.ps1'

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

    # We manually split these instead of using array-typed arguments because of how PowerShell handles input.
    # We want to provide a string containing zero or more options as input, basically. PowerShell has problems
    # processing the "zero" variant if you use arrays - you have to manually make an empty array and cannot wrap
    # it in a string AND you cannot even automatically wrap it in an array because that changes parsing to illogical.
    # GRR POWERSHELL!

    if ($dataVolumeNames) {
        $dataVolumes = @($dataVolumeNames.Split(";,".ToCharArray()) | ForEach-Object { @{ Name = $_ } })
    }

    if ($hostnames) {
        $hosts = @($hostnames.Split(";,".ToCharArray()) | ForEach-Object { @{ Name = $_ } })
    }

    if ($portAssignments) {
        $portAssignmentStrings = @($portAssignments.Split(";,".ToCharArray()))
    }

    if (!$httpBehavior) {
        # The deployment agent expects it to have a value even if the component does not accept HTTP(S) connections.
        $httpBehavior = "Allow"
    }

    if (!$start) {
        $start = "Immediately"
    }

    $customProperties = @()

    # Associate some interesting data with the component.
    if ($env:AGENT_MACHINENAME) {
        # Detect if the script is executed by Azure DevOps/TFS.
        $customProperties += @{ Name = "SourceBranch"; Value = $env:BUILD_SOURCEBRANCHNAME }

        foreach ($artifactVariable in Get-ChildItem Env:) {
            if ($artifactVariable.Name -match 'RELEASE_ARTIFACTS_(.*)_BUILDNUMBER') {
                $artifactName = $Matches[1]
                $artifactVersion = $artifactVariable.Value

                $customProperties += @{ Name = "${artifactName}-ArtifactVersion"; Value = $artifactVersion }
            }
        }
    }

    $componentConfiguration = @{
        ImageFullName       = $imageFullName
        Hostnames           = $hosts
        DataVolumes         = $dataVolumes
        RegistryUsername    = $registryUsername
        HttpBehavior        = $httpBehavior
        HasChecks           = $hasChecks.IsPresent
        EntrypointArguments = $entrypointArguments
        PortAssignments     = @()
        StartStrategy       = $start
        CustomProperties    = $customProperties
    }

    foreach ($assignmentString in $portAssignmentStrings) {
        $components = $assignmentString.Split(":")

        if ($components.Length -lt 2 -or $components.Length -gt 3) {
            Write-Error "Port assignment must take the form protocol:external:internal."
            return
        }

        if ($components[0] -inotin @("udp", "tcp")) {
            Write-Error "Port assignment protocol must be UDP or TCP."
            return
        }

        $externalPort = [uint16]$components[1]
        $internalPort = $externalPort

        if ($components.Length -eq 3) {
            $internalPort = [uint16]$components[2]
        }

        $componentConfiguration.PortAssignments += @{
            ExternalPort = $externalPort
            InternalPort = $internalPort
            Type         = $components[0]
        }
    }

    if ($quickPublish) {
        $componentConfiguration.QuickPublish = $true
    }

    if ($grpc) {
        $componentConfiguration.EnableGrpc = $true
    }

    if ($privileged) {
        $componentConfiguration.Privileged = $true
    }

    # Print the request body before adding secrets into it.
    Write-Host "Request body before secrets are inserted:"
    Write-Host (ConvertTo-Json $componentConfiguration -Depth 10)

    # Now add secrets and serialize for real.
    $componentConfiguration.RegistryPassword = $registryPassword
    $componentConfiguration.ConfigurationPackage = $configurationPackageEncoded

    $requestBody = ConvertTo-Json $componentConfiguration -Depth 10

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

        try {
            $startResponse = Invoke-WebRequest -Uri $startUrl -Method Put -TimeoutSec $webRequestTimeoutInSeconds -ContentType "application/json" -Body $requestBody -UseBasicParsing
        }
        catch {
            # We want to see the response body, as it contains valuable error messages.
            $errorObject = $Error[0]
            $responseBody = ParseErrorForResponseBody $errorObject

            Write-Error $responseBody -ErrorAction Continue
            Write-Error $errorObject
        }

        $startResponseObject = ConvertFrom-Json $startResponse.Content
        $taskId = $startResponseObject.TaskId

        Write-Host "Waiting for completion of task $taskId"

        $pollUrl = "http://$server`:$port/api/tasks/$taskId"

        & $awaitTaskPath -url $pollUrl -timeout $timeout

        if ($timeoutWaitingForGreenDashboard -ne 0) {
            Write-Host "Checking whether the dashboard is green or waiting until it is."
            & $awaitGreenDashboardPath -server $server -timeout $timeoutWaitingForGreenDashboard
        }

        $serverNumber++
    }
}
finally {
    if ($hasConfigurationPackage) {
        Remove-Item $tempZipFile -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
    }
}

Write-Host "All done."
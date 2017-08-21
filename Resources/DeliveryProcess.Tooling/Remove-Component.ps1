[CmdletBinding()]
param(
	# Namespace of the component on the server. Any server assets are unique in each namespace and isolated from other namespaces.
	[Parameter(Mandatory = $True)]
	[string]$namespace,

	# Name of the component on the server.
	[Parameter(Mandatory = $True)]
	[string]$name,

	# Hostname or IP address of the server to remove the image from. The server must be configured according
	# to the component hosting principles of the Axinom Service Delivery Process. Arbitrary Docker hosts are not supported.
	[Parameter(Mandatory = $True)]
	[string]$server,

	# Timeout in seconds.
	[Parameter(Mandatory = $False)]
	[int]$timeout = 1800
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


$port = 6742

# Server may contain a port to override the default one (e.g. in special development environment).
if ($server.Contains(":"))
{
	$serverParts = $server -split ":"
	$server = $serverParts[0]
	$port = $serverParts[1]
}

Write-Host "Server is $server`:$port"

$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

$apiUrl = "http://$server`:$port/api/components/$namespace/$name`?timeoutInSeconds=$timeout"
Write-Host "API URL is $apiUrl"

Write-Host "Starting task to remove component."

$startResponse = Invoke-WebRequest -Uri $apiUrl -Method Delete -TimeoutSec $webRequestTimeoutInSeconds -UseBasicParsing

$startResponseObject = ConvertFrom-Json $startResponse.Content
$taskId = $startResponseObject.TaskId

Write-Host "Waiting for completion of task $taskId"

$pollUrl = "http://$server`:$port/api/tasks/$taskId"

& $awaitTaskPath -url $pollUrl -timeout $timeout
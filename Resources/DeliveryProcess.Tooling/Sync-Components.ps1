[CmdletBinding()]
param(
	# Hostname or IP address of the server to synchronize state on. The server must be configured according
	# to the component hosting principles of the Axinom Service Delivery Process. Arbitrary Docker hosts are not supported.
	[Parameter(Mandatory = $True)]
	[string]$server,

	# Timeout in seconds.
	[Parameter(Mandatory = $False)]
	[int]$timeout = 300
)

$ErrorActionPreference = "Stop"

$myDirectoryPath = $PSScriptRoot

# Report tooling version just in case someone copies a deployment log without more info.
& (Join-Path $PSScriptRoot "Get-ToolingVersion")

$awaitTaskPath = Join-Path $PSScriptRoot 'Internal-Await-Task.ps1'

# Each individual request should be very fast, so don't expect no funny business here.
$webRequestTimeoutInSeconds = 30

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

$apiUrl = "http://$server`:$port/api/components/synchronize`?timeoutInSeconds=$timeout"
Write-Host "API URL is $apiUrl"

Write-Host "Starting task to synchronize component state in deployment agent."

$startResponse = Invoke-WebRequest -Uri $apiUrl -Method Post -TimeoutSec $webRequestTimeoutInSeconds -UseBasicParsing

$startResponseObject = ConvertFrom-Json $startResponse.Content
$taskId = $startResponseObject.TaskId

Write-Host "Waiting for completion of task $taskId"

$pollUrl = "http://$server`:$port/api/tasks/$taskId"

& $awaitTaskPath -url $pollUrl -timeout $timeout
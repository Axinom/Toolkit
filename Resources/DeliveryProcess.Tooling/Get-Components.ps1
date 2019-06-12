[CmdletBinding()]
param(
	# Hostname or IP address of the server to remove the image from. The server must be configured according
	# to the component hosting principles of the Axinom Service Delivery Process. Arbitrary Docker hosts are not supported.
	[Parameter(Mandatory = $True)]
	[string]$server,

	# Timeout in seconds.
	[Parameter(Mandatory = $False)]
	[int]$timeout = 60
)

$ErrorActionPreference = "Stop"

$myDirectoryPath = $PSScriptRoot

if (!$myDirectoryPath) {
    $myDirectoryPath = "."
}

# Report tooling version just in case someone copies a deployment log without more info.
& (Join-Path $myDirectoryPath "Get-ToolingVersion")


$port = 6742

# Server may contain a port to override the default one (e.g. in special development environment).
if ($server.Contains(":"))
{
	$serverParts = $server -split ":"
	$server = $serverParts[0]
	$port = $serverParts[1]
}

Write-Host "Server is $server`:$port"

$apiUrl = "http://$server`:$port/api/components?timeoutInSeconds=$timeout"
Write-Host "API URL is $apiUrl"

Write-Host "Querying host for list of components."

# Allow some extra because we prefer it if the server times out and not the client, for better synchronized business logic.
$webRequestTimeout = 30 + $timeout

$response = Invoke-WebRequest -Uri $apiUrl -TimeoutSec $webRequestTimeout -UseBasicParsing

# On PowerShell Core, there is no ContentType, so assume success.
if ($response.BaseResponse.ContentType -and !$response.BaseResponse.ContentType.StartsWith("application/json"))
{
	Write-Host "${$response.StatusCode} ${$response.StatusDescription}"
	Write-Host $response.Content

	Write-Error "Server did not return a JSON response."
}

$responseObject = ConvertFrom-Json $response.Content
$count = ($responseObject | Measure-Object).Count

Write-Host "$count components are deployed on the host."

Write-Output $responseObject
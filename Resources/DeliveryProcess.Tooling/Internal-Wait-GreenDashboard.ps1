[CmdletBinding()]
param(
	[Parameter(Mandatory = $True)]
	[string]$server,

	# Timeout in seconds.
	[Parameter(Mandatory = $True)]
	[int]$timeout
)

$ErrorActionPreference = "Stop"

$webRequestTimeoutInSeconds = 60

# When polling for dashboard status, we delay this much between queries.
$pollDelayInSeconds = 20

$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

while ($true)
{
	if ($stopwatch.Elapsed.TotalSeconds -gt $timeout)
	{
		Write-Error "Timeout."
		return
	}

    try
    {
        Invoke-WebRequest -Uri $server -Method Get -TimeoutSec $webRequestTimeoutInSeconds -UseBasicParsing | Out-Null

        Write-Host "Dashboard green."
        break
    }
    catch
    {
        Write-Host "Dashboard red. Checking again in $pollDelayInSeconds seconds."
        Start-Sleep -Seconds $pollDelayInSeconds
    }
}
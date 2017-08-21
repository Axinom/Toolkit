[CmdletBinding()]
param(
	[Parameter(Mandatory = $True)]
	[string]$url,

	# Timeout in seconds.
	[Parameter(Mandatory = $True)]
	[int]$timeout
)

$ErrorActionPreference = "Stop"

# Each individual request should be very fast, so don't expect no funny business here.
$webRequestTimeoutInSeconds = 60

# When polling for task status, we delay this much between queries.
$pollDelayInSeconds = 10

$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

while ($true)
{
	if ($stopwatch.Elapsed.TotalSeconds -gt $timeout)
	{
		Write-Error "Timeout."
		return
	}

	$pollResponse = Invoke-WebRequest -Uri $pollUrl -Method Get -TimeoutSec $webRequestTimeoutInSeconds -UseBasicParsing
			
	$status = ConvertFrom-Json $pollResponse.Content

	$report = (Get-Date).ToString("u")
	$report += " "
	$report += $status.Message

	Write-Host $report

	if (!$status.IsInProgress)
	{
		if ($status.Completed)
		{
			Write-Host "Task completed."
			break
		}
		else
		{
			$report = "Task failed: "
			$report += $status.Message

			Write-Error $report
			return
		}
	}

	Start-Sleep -Seconds $pollDelayInSeconds
}
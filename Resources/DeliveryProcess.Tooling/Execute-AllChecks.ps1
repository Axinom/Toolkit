[CmdletBinding()]
param(
	# Checker API endpoint URL.
	[Parameter(Mandatory = $True)]
	[string]$baseUrl
)

$ErrorActionPreference = "Stop"

$myDirectoryPath = $PSScriptRoot

if (!$myDirectoryPath) {
    $myDirectoryPath = "."
}

# Report tooling version just in case someone copies a log without more info.
& (Join-Path $myDirectoryPath "Get-ToolingVersion")

$checksList = Invoke-RestMethod -Uri "$baseUrl/Checks" -UseBasicParsing

$hasFailures = $false;

$checksList | Where-Object {
	$checkName = $_.Name

	Write-Host "Executing check: $checkName"

	try
	{
		$checkResult = Invoke-RestMethod -Uri "$baseUrl/Checks/$checkName/Execute" -Method Post -UseBasicParsing -ErrorAction Stop
	}
	catch
	{
		$errorMessage = $_.Exception.Message
		Write-Host "Exception: $errorMessage"
		$hasFailures = $true
		return
	}

	if ($checkResult.Result -eq "Success")
	{
		Write-Host "Check successful."
	}
	else
	{
		Write-Host "Check failed. Result: $($checkResult.Result); Message: $($checkResult.Message)"
		$hasFailures = $true
	}
}

if ($hasFailures) {
	Write-Error "Checks had failures."
}

Write-Host "All done."
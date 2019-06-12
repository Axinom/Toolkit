[CmdletBinding()]
param(
	# This file must contain the AssemblyFileVersion (preferred) or AssemblyVersion attribute.
	[Parameter(Mandatory = $True)]
	[string]$assemblyInfoPath,

	# TFS build ID.
	[Parameter(Mandatory = $True)]
	[int]$buildId,

	# Git commit ID.
	[Parameter(Mandatory = $True)]
	[string]$commitId,

    # Name of the primary branch. Builds in any other branch get the branch name as a version string prefix.
    [Parameter()]
    [string]$primaryBranchName = "master"
)

$ErrorActionPreference = "Stop"

$myDirectoryPath = $PSScriptRoot

if (!$myDirectoryPath) {
    $myDirectoryPath = "."
}

if (!(Test-Path $assemblyInfoPath))
{
	Write-Error "AssemblyInfo file not found at $assemblyInfoPath."
}

if ($commitId.Length -lt 7)
{
	Write-Error "The Git commit ID is too short to be a valid commit ID."
}

# Convert to absolute paths because .NET does not understand PowerShell relative paths.
$assemblyInfoPath = Resolve-Path $assemblyInfoPath

# All versions built using this process must be release versions. There is no concept of a debug version.
# Try to ensure this is so by looking for the BuildConfiguration environment variable that is present in automated builds.
if ($env:BuildConfiguration -and $env:BuildConfiguration -ne "Release")
{
	Write-Error "Only release-mode builds are compatible with build automation."
}

$assemblyInfo = [System.IO.File]::ReadAllText($assemblyInfoPath)

$primaryRegex = New-Object System.Text.RegularExpressions.Regex('AssemblyFileVersion(?:Attribute)?\("(.*)"\)')
$fallbackRegex = New-Object System.Text.RegularExpressions.Regex('AssemblyVersion(?:Attribute)?\("(.*)"\)')

$versionMatch = $primaryRegex.Matches($assemblyInfo)

if (!$versionMatch.Success)
{
	$versionMatch = $fallbackRegex.Matches($assemblyInfo)

	if (!$versionMatch.Success)
	{
		Write-Error "Unable to find AssemblyFileVersion or AssemblyVersion attribute."
	}
}

$version = $versionMatch.Groups[1].Value

Write-Host "AssemblyInfo version is $version"

# Shorten the commit ID. 7 characters seem to be the standard.
$commitId = $commitId.Substring(0, 7)

if ($buildId -gt 999999)
{
	Write-Error "Build ID too large! Values over 999999 are not supported."
}

# Zero-pad the build ID to 6 digits.
$buildIdString = $buildId.ToString("000000")

$version = "$version-$buildIdString-$commitId"
Write-Host "Version string is $version"

# VSTS does not immediately update it, so update it manually.
$env:BUILD_BUILDNUMBER = $version
$version = & (Join-Path $myDirectoryPath "Set-VersionStringBranchPrefix.ps1") -primaryBranchName $primaryBranchName -skipBuildNumberUpdate

Write-Host "##vso[build.updatebuildnumber]$version"

Write-Host "Version string set!"

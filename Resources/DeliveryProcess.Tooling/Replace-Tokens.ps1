[CmdletBinding()]
param
(
	# Directory or file to do token replacement within.
    [Parameter(Mandatory = $true)]
	[string] $path,

	# Filenames (or patterns) to use for selecting files to replace tokens within, if the Path parameter is a directory.
	# Optional if the Path parameter already specifies a file.
	[Parameter(Mandatory = $false)]
	[string] $filenames,

	# Whether the search for files is recursive (defaults to no). Ignored if the Path parameter specifies a file.
	[Parameter(Mandatory = $false)]
	[switch] $recursive = $false,

	# Allows overriding the token start string, for custom file formats where the default is problematic.
	[Parameter(Mandatory = $false)]
    [string] $tokenStart = "__",

	# Allows overriding the token end string, for custom file formats where the default is problematic. Regex syntax.
	[Parameter(Mandatory = $false)]
    [string] $tokenEnd = "__",

	# A list of secret-values variables ("key=value","key=value") to include in replacement. Secret build
	# process variables are not available for replacement without explicitly providing them here. Regex syntax.
	[Parameter(Mandatory = $false)]
	[string[]] $secrets
)

# Forked from https://github.com/TotalALM/VSTS-Tasks/tree/master/Tasks/Tokenization and heavily modified.

$ErrorActionPreference = "Stop"

. "$PSScriptRoot\FileUtilities.ps1"

# Assemble all the secrets into a hashtable for easy use later.
$secretValues = @{}

if ($secrets -and $secrets.Length -gt 0)
{
	Write-Host "$($secrets.Length) secret variables provided on command line."

	foreach ($s in $secrets)
	{
		$pair = @($s -split "=", 2)

		if ($pair.Length -ne 2)
		{
			Write-Error "A secret value parameter was not formatted as key=value."
		}
		else
		{
			$secretValues[$pair[0]] = $pair[1]

			Write-Host "Obtained secret value with name $($pair[0])"
		}
	}
}

# Prepare for string processing.
$patterns = @()
$regex = $TokenStart + "[A-Za-z0-9._]+" + $TokenEnd
$matches = @()

Write-Host "Regex: $regex"

function ProcessFile($file)
{
	Write-Host "Found file: $($file.FullName)"

	$fileEncoding = Get-FileEncoding($file.FullName)
	
	Write-Host "Detected file encoding: $fileEncoding"

	$newlines = Get-NewlineCharacters($file.FullName)

	$targetFilePath = $file.Directory.FullName
	$tempFile = $file.FullName + ".tmp"
		
	Copy-Item -Force $file.FullName $tempFile
	
	$matches = select-string -Path $tempFile -Pattern $regex -AllMatches | % { $_.Matches } | % { $_.Value }

	foreach($match in $matches)
	{
        $matchedItem = $match
        $matchedItem = $matchedItem.TrimStart($TokenStart)
        $matchedItem = $matchedItem.TrimEnd($TokenEnd)
        $matchedItem = $matchedItem -replace "\.","_"
        
        Write-Host "Found token $matchedItem" -ForegroundColor Green
        
		if (Test-Path Env:$matchedItem)
		{
	        $matchValue = (Get-ChildItem Env:$matchedItem).Value

			Write-Host "Found matching variable. Value: $matchValue" -ForegroundColor Green
		}
		elseif ($secretValues.ContainsKey($matchedItem))
		{
			$matchValue = $secretValues[$matchedItem]

			Write-Host "Found matching secret variable." -ForegroundColor Green
		}
		else
		{
			$matchValue = ""

			Write-Host "Found no matching variable. Replaced with empty string." -ForegroundColor Green
		}
        
		$newContent = (Get-Content $tempFile) | Foreach-Object { $_ -replace $match,$matchValue }
		$newContentAsString = [string]::Join($newlines, $newContent)
		
		Set-Content $tempFile -Force -Encoding $fileEncoding -Value $newContentAsString -NoNewline
	}
	
	Copy-Item -Force $tempFile $file.FullName
	Remove-Item -Force $tempFile	
}

Write-Host "Target path: $path"
Write-Host "Recursive: $recursive"

if (Test-Path -PathType Leaf $path)
{
	Write-Host "Target is a file. Will proceed directly to perform token replacement on it."

	Get-Item -Path $path | ForEach-Object { ProcessFile $_ }
}
elseif (Test-Path -PathType Container $path)
{
	Write-Host "Target is a directory. Will replace tokens in contents."

	foreach ($target in $filenames.Split(",;"))
	{
		Write-Host "Targeted filename or pattern: $target"
		
		if ($recursive)
		{	
			Get-ChildItem -Path $path -File -Filter $target -Recurse | ForEach-Object { ProcessFile $_ }
		}
		else 
		{
			Get-ChildItem -Path $path -File -Filter $target | ForEach-Object { ProcessFile $_ }
		}
	}
}
else
{
	Write-Error "Target path points to item that does not exist."
}
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

function Get-FileEncoding($targetFilePath)
{
	[byte[]]$byte = get-content -Encoding byte -ReadCount 4 -TotalCount 4 -Path $targetFilePath
	#Write-Host Bytes: $byte[0] $byte[1] $byte[2] $byte[3]
 
	# EF BB BF (UTF8)
	if ( $byte[0] -eq 0xef -and $byte[1] -eq 0xbb -and $byte[2] -eq 0xbf )
	{ return "UTF8" }
 
	# FE FF  (UTF-16 Big-Endian)
	elseif ($byte[0] -eq 0xfe -and $byte[1] -eq 0xff)
	{ return "Unicode UTF-16 Big-Endian" }
 
	# FF FE  (UTF-16 Little-Endian)
	elseif ($byte[0] -eq 0xff -and $byte[1] -eq 0xfe)
	{ return "Unicode UTF-16 Little-Endian" }
 
	# 00 00 FE FF (UTF32 Big-Endian)
	elseif ($byte[0] -eq 0 -and $byte[1] -eq 0 -and $byte[2] -eq 0xfe -and $byte[3] -eq 0xff)
	{ return "UTF32 Big-Endian" }
 
	# FE FF 00 00 (UTF32 Little-Endian)
	elseif ($byte[0] -eq 0xfe -and $byte[1] -eq 0xff -and $byte[2] -eq 0 -and $byte[3] -eq 0)
	{ return "UTF32 Little-Endian" }
 
	# 2B 2F 76 (38 | 38 | 2B | 2F)
	elseif ($byte[0] -eq 0x2b -and $byte[1] -eq 0x2f -and $byte[2] -eq 0x76 -and ($byte[3] -eq 0x38 -or $byte[3] -eq 0x39 -or $byte[3] -eq 0x2b -or $byte[3] -eq 0x2f) )
	{ return "UTF7"}
 
	# F7 64 4C (UTF-1)
	elseif ( $byte[0] -eq 0xf7 -and $byte[1] -eq 0x64 -and $byte[2] -eq 0x4c )
	{ return "UTF-1" }
 
	# DD 73 66 73 (UTF-EBCDIC)
	elseif ($byte[0] -eq 0xdd -and $byte[1] -eq 0x73 -and $byte[2] -eq 0x66 -and $byte[3] -eq 0x73)
	{ return "UTF-EBCDIC" }
 
	# 0E FE FF (SCSU)
	elseif ( $byte[0] -eq 0x0e -and $byte[1] -eq 0xfe -and $byte[2] -eq 0xff )
	{ return "SCSU" }
 
	# FB EE 28  (BOCU-1)
	elseif ( $byte[0] -eq 0xfb -and $byte[1] -eq 0xee -and $byte[2] -eq 0x28 )
	{ return "BOCU-1" }
 
	# 84 31 95 33 (GB-18030)
	elseif ($byte[0] -eq 0x84 -and $byte[1] -eq 0x31 -and $byte[2] -eq 0x95 -and $byte[3] -eq 0x33)
	{ return "GB-18030" }
 
	else
	{ return "ASCII" }
}

function Get-NewlineCharacters($targetFilePath)
{
	$absolutePath = Resolve-Path $targetFilePath
	$contentBytes = [System.IO.File]::ReadAllBytes($absolutePath)

	$cr = $contentBytes -contains 0x0d
	$lf = $contentBytes -contains 0x0a

	# Kind of hacky but whatever.
	if ($cr -and $lf) {
		Write-Host -Verbose "Using CR LF for line endings."
		return "`r`n"
	} elseif ($cr) {
		Write-Host -Verbose "Using CR for line endings."
		return "`r"
	} elseif ($lf) {
		Write-Host -Verbose "Using LF for line endings."
		return "`n"
	} else {
		# Fall back to whatever the platform default is, if none were found in the file.
		Write-Host -Verbose "Using platform default for line endings."
		return [Environment]::NewLine
	}
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
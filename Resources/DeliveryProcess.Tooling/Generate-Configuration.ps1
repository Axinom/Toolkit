[CmdletBinding()]
Param(
    # Root folder of Configuration Value Files.
    # Not mandatory because one might want to make use of only environment variables for configuration.
    [Parameter(Mandatory = $False)]
    [string]$values,

    # Folder where the configuration templates reside.
    [Parameter(Mandatory = $False)]
    [string]$templates = ".\ConfigurationTemplates\",

    # Folder where the actual configuration files with placeholders replaced will be created.
    [Parameter(Mandatory = $False)]
    [string]$output = ".\TransformedConfiguration\",

    [Parameter(Mandatory = $True)]
    [string]$componentName,

    [Parameter(Mandatory = $True)]
    [string]$environmentName,

    # Specifies whether to fail if an undefined placeholder is found from configuration templates.
    [Parameter(Mandatory = $False)]
    [switch]$failIfNoMatch = $False
)

<#
This script replaces placeholders in configuration templates with actual values and saves the transformed files
(with the original names) in another location. The original configuration templates remain unchanged.

The configuration templates are files which contain placeholders which must look like this: __VARIABLE_NAME__. More specifically,
they must match the variable names defined the configuration files and start and end with double underscores.

The configuration files folder provided to the script via -values argument must be a specifically structured folder:
/
	environment-name-1/
		component-name-1/
			Configuration.cvf
		Configuration.cvf
	environment-name-2/
		component-name-2/
			Configuration.cvf
		Configuration.cvf
	Configuration.cvf
	component-name-1.cvf
	component-name-2.cvf

If any of the files is not found they are ignored.
The .cvf files must contain a list of variables and their values, separated with a '=' sign. Here is an example content of such a file:

LICENSE_ACQUISITION_URL = https://drm-widevine-licensing.axtest.net/AcquireLicense
DRM_COM_KEY = WJE5aC7mdBIrVW40v4hafSqzNNASknPqh5WyAEoiDfA=
DRM_COM_KEY_ID = da46b143-c746-4ce5-9925-a73200710d22
IMAGE_USAGES = FROM_FILE

Notice that IMAGE_USAGES has a special value - "FROM_FILE". This is a keyword indicating that the value for this variable is the content
of a separate file located in the same folder as the Configuration Value File named as the variable name.
For the example above, the value is read from a file named "IMAGE_USAGES" (without any extension).

If multiple Configuration Value Files define the same variable, the one that is nested deeper in the tree takes precedence.

Arbitrary files are also supported as part of configuration, which can be license files or certificates or anything. These files will
be copied to the output folder, just beside the transformed templates.
#>

$ErrorActionPreference = "Stop"

. "$PSScriptRoot\FileUtilities.ps1"

$fromFileMagicWord = "FROM_FILE"

$tokenStart = "__"
$tokenEnd = "__"
$regex = $TokenStart + "[A-Za-z0-9._]+" + $TokenEnd
$matches = @()

function ProcessFile($file, $configuration) {
    Write-Host "Replacing placeholders in: $($file.FullName)"

    $fileEncoding = Get-FileEncoding($file.FullName)

    $newlines = Get-NewlineCharacters($file.FullName)

    $tempFile = $file.FullName + ".tmp"
		
    Copy-Item -Force $file.FullName $tempFile
	
    $matches = select-string -Path $tempFile -Pattern $regex -AllMatches | % { $_.Matches } | % { $_.Value }

    $matchesNotFound = @()

    foreach ($match in $matches) {
        $matchedItem = $match
        $matchedItem = $matchedItem.TrimStart($TokenStart)
        $matchedItem = $matchedItem.TrimEnd($TokenEnd)
        $matchedItem = $matchedItem -replace "\.", "_"
		
        if (Test-Path Env:$matchedItem) {
            $matchValue = (Get-ChildItem Env:$matchedItem).Value

            Write-Host "Found matching variable from environment variables. Setting $matchedItem to '$matchValue'" -ForegroundColor Green
        }
        elseif ($configuration.ContainsKey($matchedItem)) {
            $matchValue = $configuration[$matchedItem]
        }
        else {
            $matchValue = ""
            $matchesNotFound += $match

            Write-Host "No variable '$matchedItem' defined. Replaced with empty string." -ForegroundColor Yellow
        }
		
        $newContent = (Get-Content $tempFile) | Foreach-Object { $_ -replace $match, $matchValue }
        $newContentAsString = [string]::Join($newlines, $newContent)
		
        Set-Content $tempFile -Force -Encoding $fileEncoding -Value $newContentAsString -NoNewline
    }

    Copy-Item -Force $tempFile $file.FullName
    Remove-Item -Force $tempFile

    return $matchesNotFound
}

# Takes in an array of strings, each representing a key and value pair separated with a '=' sign.
# Strings that start with '#' are ignored.
# Outputs a hashtable representing the input.
function ConvertFrom-KeyValuePairs {
    $object = @{}
    foreach ($line in $input) {
        $line = $line.Trim()
        if ($line -and !$line.StartsWith("#")) {
            $tokens = $line -split "=", 2
            if ($tokens.Length -ne 2) {
                Write-Error "Bad input format: '$line'"
            }
            $name = $tokens[0].Trim()
            $value = $tokens[1].Trim()
            $object[$name] = $value
        }
    }
    $object
}

function Get-ConfigurationFromFile([string] $configurationFile) {
    $configuration = (Get-Content $configurationFile) | ConvertFrom-KeyValuePairs

    $configuration | Format-Table | Out-String | Write-Host

    $externalFiles = @() # Collecting the paths to files referenced by FROM_FILE magic word.

    # Some variables have a value defined as content of separate files.
    foreach ($key in $($configuration.Keys)) {
        if ($configuration[$key] -eq $fromFileMagicWord) {
            $externalFilePath = Join-Path -Path (Split-Path -Path $configurationFile -Resolve) $key
            $configuration[$key] = Get-Content $externalFilePath -Raw
            $externalFiles += $externalFilePath
        }
    }

    return $configuration, $externalFiles
}

function Copy-ArbitraryFilesToOutput([string] $fromFolder, [string[]] $filesToIgnore) {
    $arbitraryFiles = Get-ChildItem -Path $fromFolder -File | Where-Object { $_.FullName -notin $filesToIgnore -and $_.Extension -ne ".cvf" } | ForEach-Object { "$($_.FullName)" }

    if ($arbitraryFiles) {
        Copy-Item $arbitraryFiles -Destination $output
    }
}

if (Test-Path -PathType Container $output) {
    Remove-Item -Force -Recurse $output
}

# Let's not modify the original configuration templates but instead a copy of them.
Copy-Item $templates -Destination $output -Recurse -Container -Force

# Let's save the list of files in it before arbitrary files get copied there.
$configurationTemplates = Get-ChildItem -Path $output -File -Filter *

# Will all configuration keys and values gathered from all files.
$configurationAsHashtable = @{}

if ($values) {
    if (-Not (Test-Path -PathType Container $values)) {
        Write-Error "$values folder not found. Exiting."
    }

    # Create an array of all configuration files to use.
    $configurationFiles = @((Join-Path "$values" -ChildPath "Configuration.cvf"),
    (Join-Path "$values" -ChildPath "$componentName.cvf"),
    (Join-Path "$values" -ChildPath "$environmentname\Configuration.cvf"),
    (Join-Path "$values" -ChildPath "$environmentName\$componentName\Configuration.cvf"))

    foreach ($configurationFile in $configurationFiles) {
        if (!(Test-Path $configurationFile)) {
            Write-Host "Ignoring the nonexistent configuration file: $configurationFile"
            Copy-ArbitraryFilesToOutput -fromFolder (Split-Path -Path $configurationFile)
            continue
        }

        Write-Host "Reading configuration from: $configurationFile"
        $configuration, $externalFiles = Get-ConfigurationFromFile -configurationFile $configurationFile
        $configuration.Keys | ForEach-Object { $configurationAsHashtable[$_] = $configuration[$_] }
        Copy-ArbitraryFilesToOutput -fromFolder (Split-Path -Path $configurationFile) -filesToIgnore $externalFiles
    }
}

$allMatchesNotFound = @()

$configurationTemplates | ForEach-Object {
    $matchesNotFound = ProcessFile -file $_ -configuration $configurationAsHashtable
    $allMatchesNotFound += $matchesNotFound
}

if ($allMatchesNotFound.Count -gt 0 -and $failIfNoMatch -eq $True) {
    Write-Error "Undefined placeholders found from configuration templates."
}

Write-Host "`nOutput looks like this:"
Get-ChildItem -Path $output -Recurse

Write-Host "`nDone."
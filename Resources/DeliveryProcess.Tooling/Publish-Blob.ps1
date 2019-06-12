# This script publishes blobs to Azure Storage and outputs a URL for read access.
# It is used for publishing VM images and other "it is a big file" type of outputs.
#
# There are two types of possible inputs: files on local disk and other blobs already in Azure Storage.
# The latter allows "labeling" - you publish some file at first and then publish the already-existing
# blob as a "stable" version when you decide that your initial upload is a good result.
#
# Requires AzureRM PowerShell module.

[CmdletBinding()]
param(
    # Name of the blob to create. If it already exists, it is overwritten.
    [Parameter(Mandatory = $True)]
    [string]$blobName,

    # Name of the storage container to create the blob in.
    # Will be created if it does not already exist.
    [Parameter(Mandatory = $True)]
    [string]$containerName,

    # Azure Storage connection string.
    [Parameter(Mandatory = $True)]
    [string]$connectionString,

    # Azure CDN hostname. The hostname in the published URL will be replaced with this if provided.
    [Parameter()]
    [string]$cdn,

    # If you are going to overwrite the contents of this blob later, it may be desirable to disable caching.
    # Otherwise the changes may not show up right away when the blob is accessed through caches/CDN.
    [Parameter()]
    [switch]$noCache,

    # Path to the file to publish if the input is a file.
	[Parameter(ParameterSetName = "FromFile", Mandatory = $True)]
    [string]$path,

    # Name of an existing blob to publish under the new name.
    [Parameter(ParameterSetName = "FromBlob", Mandatory = $True)]
    [string]$sourceBlobName,

    # Name of the container that contains the existing blob.
    [Parameter(ParameterSetName = "FromBlob", Mandatory = $True)]
    [string]$sourceContainerName
)

$ErrorActionPreference = "Stop"

$myDirectoryPath = $PSScriptRoot

if (!$myDirectoryPath) {
    $myDirectoryPath = "."
}

# Report tooling version just in case someone copies a log without more info.
& (Join-Path $myDirectoryPath "Get-ToolingVersion")

$azureContext = New-AzureStorageContext -ConnectionString $connectionString

# Ensure the container exists.
New-AzureStorageContainer $containerName -Context $azureContext -ErrorAction SilentlyContinue | Out-Null

$properties = @{
    "CacheControl" = ""
}

if ($noCache) {
    # Stick a lot of directives in there, as different caches seem to (mis)behave differently.
    $properties["CacheControl"] = "no-cache, no-store, must-revalidate, max-age=0"
}

if ($path) {
    Write-Host "Uploading $path to $containerName/$blobName"

    $blob = Set-AzureStorageBlobContent -File $path -Blob $blobName -Container $containerName -Force -Context $azureContext -Properties $properties
}
else {
    Write-Host "Copying $sourceContainerName/$sourceBlobName to $containerName/$blobName"

    $blob = Start-AzureStorageBlobCopy -SrcBlob $sourceBlobName -SrcContainer $sourceContainerName -DestBlob $blobName -DestContainer $containerName -Force -Context $azureContext

    Write-Verbose "Waiting for copy to finish."

    $blob | Get-AzureStorageBlobCopyState -WaitForComplete | Out-Null

    $blob.ICloudBlob.Properties.CacheControl = $properties["CacheControl"]
    $blob.ICloudBlob.SetProperties()
}

# It needs an expiration but we do not want it to expire. 10 years should be enough for everybody.
$blobUrl = New-AzureStorageBlobSASToken -CloudBlob $blob.ICloudBlob -FullUri -Permission "r" -Context $azureContext -ExpiryTime (Get-Date).AddYears(10)

if ($cdn) {
    # Replace hostname. Should cover most CDN scenarios nice and proper.
    Write-Host "Blog URL will target the CDN endpoint $cdn"

    $builder = New-Object UriBuilder -ArgumentList $blobUrl
    $builder.Host = $cdn
    $blobUrl = $builder.Uri.AbsoluteUri
}

Write-Host "Publishing blob public URL"
Write-Output $blobUrl
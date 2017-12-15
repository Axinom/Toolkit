[CmdletBinding()]
param(
    # Name of the primary branch. Builds in any other branch get the branch name as a version string prefix.
    [Parameter()]
    [string]$primaryBranchName = "master",

    # If set, will not update the build number in VSTS and just write it to output. Useful when calling
    # from another script that already sets the build number, as this action is racy and should be done only once.
    #
    # The prefix variable will always be set as this script is the source of truth for the prefix.
    [Parameter()]
    [switch]$skipBuildNumberUpdate
)

# This script prefixes the TFS/VSTS version string with a branch name.
# It will trigger a build number update in VSTS unless the skip switch is specified.
# A process variable "versionPrefix" will contain the added text.
# The output of the script will be the updated version string (even if no update was made).
#
# If in the primary branch, the version string is not modified (but the prefix is still emitted as empty string).

$ErrorActionPreference = "Stop"

$version = $env:BUILD_BUILDNUMBER

if (!$version) {
    Write-Error "Unable to detect version string."
    return
}

# Generate optional version string prefix.
if ($env:SYSTEM_PULLREQUEST_SOURCEBRANCH) {
    # If we are in a PR build, stick the PR source branch name in front.

    $versionPrefix = $env:SYSTEM_PULLREQUEST_SOURCEBRANCH

    # Unfortunately we do not have the pure name but the full/path/to/branch here.
    # So we cut after the last / and life is easy again.
    if ($versionPrefix.Contains("/")) {
        $versionPrefix = $versionPrefix.Substring($versionPrefix.LastIndexOf("/") + 1)
    }

    # Note that we prefix on PR even if the PR source branch is the primary branch.
    # This is intentional - we do not want to mix PR builds with main builds.
	# Might not be strictly necessary, though - reconsider if it proves problematic.
}
elseif ($env:BUILD_SOURCEBRANCHNAME -and $env:BUILD_SOURCEBRANCHNAME -ne $primaryBranchName) {
    # If we are not in the primary branch, stick the branch name in front (lowercase).
    $versionPrefix = ($env:BUILD_SOURCEBRANCHNAME).ToLower()
}

if ($versionPrefix) {
    Write-Host "Prefixing version string with '$versionPrefix' to signal the branch."

    $version = $versionPrefix + "-" + $version

    # Export a versionstring.prefix variable so this can be easily referenced later on without string manipulation.
    # We will even include the dash in there so consumers can just stick it in there and it will work either way.
    Write-Host "##vso[task.setvariable variable=versionstring.prefix;]$versionPrefix-"

    Write-Output $version

    if (!$skipBuildNumberUpdate) {
        Write-Host "##vso[build.updatebuildnumber]$version"

        Write-Host "Version string has been updated to contain a prefix and the update has been published."
    }
    else {
        Write-Host "Version string has been updated to contain a prefix but the update has not been published."
    }
}
else {
    Write-Host "Will not prefix the version string with anything because we are in the primary branch."

    # Still write it to output for equivalent output.
    Write-Output $version

    Write-Host "##vso[task.setvariable variable=versionstring.prefix;]"
}
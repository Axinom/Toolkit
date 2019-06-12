function Get-Components([string]$moduleFolder) {
    $folderInfo = Get-Item -Path $moduleFolder

    $fileContent = Get-Content (Join-Path $folderInfo.FullName "Module.json") -Raw
    
    $components = $fileContent | ConvertFrom-Json

    $components | Where-Object Name -eq $null | ForEach-Object { $_ | Add-Member -MemberType NoteProperty -Name "Name" -Value "app" }

    return $components
}

# Returns a hashtable where key is the module name and value is an array of components in the module.
function Get-Modules([string[]]$moduleFolders) {
	$modules = @{}

	foreach ($moduleFolder in $moduleFolders) {
        $components = Get-Components -moduleFolder $moduleFolder

        $folderInfo = Get-Item $moduleFolder
        $moduleName = $folderInfo.Name.Split("-")[-1]

		$modules[$moduleName] = $components
	}

	return $modules
}

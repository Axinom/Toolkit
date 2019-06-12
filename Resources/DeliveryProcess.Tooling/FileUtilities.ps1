function Get-FileEncoding($targetFilePath) {
    if (!(Get-Content $targetFilePath)) {
        return "UTF8"
    }

    if ($PSVersionTable.PSVersion.Major -le 5) {
        [byte[]]$byte = Get-Content -Encoding Byte -ReadCount 4 -TotalCount 4 -Path $targetFilePath
    }
    else {
        [byte[]]$byte = Get-Content -AsByteStream -ReadCount 4 -TotalCount 4 -Path $targetFilePath
    }

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
    { return "UTF8" }
}

function Get-NewlineCharacters($targetFilePath) {
    $absolutePath = Resolve-Path $targetFilePath
    $contentBytes = [System.IO.File]::ReadAllBytes($absolutePath)

    $cr = $contentBytes -contains 0x0d
    $lf = $contentBytes -contains 0x0a

    # Kind of hacky but whatever.
    if ($cr -and $lf) {
        Write-Verbose "Using CR LF for line endings."
        return "`r`n"
    }
    elseif ($cr) {
        Write-Verbose "Using CR for line endings."
        return "`r"
    }
    elseif ($lf) {
        return "`n"
    }
    else {
        # Fall back to whatever the platform default is, if none were found in the file.
        Write-Verbose "Using platform default for line endings."
        return [Environment]::NewLine
    }
}

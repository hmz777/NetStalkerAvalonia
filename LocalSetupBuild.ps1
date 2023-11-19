while($true){

    $installerOutputPath = Read-Host "Enter the output path for the setup file (enter '0' to exit)"
    $installerOutputPath = $installerOutputPath -replace '(-|#|\||"|,|/|:|\?)', ''

    if (Test-Path $installerOutputPath) {

        $Env:Release_Version = dotnet-gitversion /showvariable FullSemVer
        dotnet publish NetStalkerAvalonia.Windows/NetStalkerAvalonia.Windows.csproj --no-restore --output ./Output.Windows --runtime win10-x86 --self-contained false /p:DebugType=None /p:DebugSymbols=false
        iscc NetStalkerSetupScript.Windows.iss
        copy InstallerOutput/NetStalkerSetup.exe $installerOutputPath
        rm Output.Windows -Force -Recurse
        rm InstallerOutput -Force -Recurse

        return

    }
    elseif ($installerOutputPath -eq 0){

        return

    }
    else {
        Write-Output "Invalid path!"
    }

}

pause
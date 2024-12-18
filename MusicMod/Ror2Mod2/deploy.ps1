
function Delete-Files {
    
    param (
        [string]$Pattern,
        [string]$Configuration
    )

    Remove-Item .\bin\$($Configuration)\* -Include $Pattern -Recurse
}

function Remove-Libraries {
    
    param (
        [string]$Configuration
    )

    Delete-Files BepInEx.* $Configuration
    Delete-Files RoR2* $Configuration
    Delete-Files 0Harmony.* $Configuration
    Delete-Files HG* $Configuration
    Delete-Files *Unity* $Configuration
    Delete-Files MMHOOK* $Configuration
    Delete-Files MonoMod* $Configuration
    Delete-Files R2API* $Configuration
    Delete-Files Rewired* $Configuration
    Delete-Files RiskOfOptions* $Configuration

    foreach ($File in Get-ChildItem "D:\Program Files (x86)\Steam\steamapps\common\Risk of Rain 2\Risk of Rain 2_Data\Managed")
    {
        Delete-Files $File.Name $Configuration
    }
}

function Deploy-Debug {
    param (
        [string]$WorkingDir = ".", 
        [string]$ProfileName = $(throw "ProfileName is required."),
        [string]$ModName = $(throw "ModName is required.")
    )

    $ErrorActionPreference = "Stop"

    $inputDir = Join-Path $WorkingDir "bin\Debug\netstandard2.1"
    $outputDir = $Env:appdata + "\r2modmanPlus-local\RiskOfRain2\profiles\"+$ProfileName+"\BepInEx\plugins\"+$ModName

    if (!(Test-Path $outputDir)) {
        throw "Output path does not exist: ${outputDir}"
    }

    $pdbs = Get-ChildItem -Path $inputDir\* -Include *.pdb
    $mono = "D:\Program Files\Unity\2019.4.26f1\Editor\Data\MonoBleedingEdge\bin\mono"
    
    Push-Location $WorkingDir
    Remove-Libraries "Debug"
    Pop-Location

    Get-ChildItem $outputDir | Remove-Item

    robocopy $inputDir $outputDir /s
    Write-Output $pdbs
    foreach ($pdb in $pdbs)
    {
        $dll = $pdb.Basename + ".dll"
        & $mono ".\pdb2mdb.exe" $outputDir\$dll
    }
}

function Release {
    Delete-Files *.pdb "Release"
    Remove-Libraries "Release"

    foreach ($Directory in Get-ChildItem .\bin\Release\netstandard2.0 -Directory)
    {
        Remove-Item $Directory.FullName -Recurse
    }
}


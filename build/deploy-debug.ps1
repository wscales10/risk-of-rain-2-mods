param (
    [string]$ProjectDir = ".", 
    [string]$ProfileName = $(throw "ProfileName is required."),
    [string]$ModName = $(throw "ModName is required.")
)

$ErrorActionPreference = "Stop"

$inputDir = Join-Path $ProjectDir "bin\Debug\netstandard2.1"
$outputDir = $Env:appdata + "\r2modmanPlus-local\RiskOfRain2\profiles\"+$ProfileName+"\BepInEx\plugins\"+$ModName

if (!(Test-Path $outputDir)) {
    throw "Output path does not exist: ${outputDir}"
}

$pdbs = Get-ChildItem -Path $inputDir\* -Include *.pdb
$mono = "D:\Program Files\Unity\2019.4.26f1\Editor\Data\MonoBleedingEdge\bin\mono"

robocopy $inputDir $outputDir /s
Write-Output $pdbs
foreach ($pdb in $pdbs)
{
    $dll = $pdb.Basename + ".dll"
    & $mono ".\pdb2mdb.exe" $outputDir\$dll
}

foreach ($item in Get-ChildItem -Path $outputDir)
{
    if (Test-Path (Join-Path "D:\Program Files (x86)\Steam\steamapps\common\Risk of Rain 2\Risk of Rain 2_Data\Managed" $item.Name))
    {
        Write-Output "$($item.Name) exists in RoR2 install directory"
    }
    elseif ($item.Name -match "^System\..*\.dll$")
    {
        Write-Output "$item.Name is a System library"
    }
    elseif ($item.Name -match "MMHOOK_RoR2.dll")
    {
        Write-Output "$item.Name will be provided by another mod"
    }
    else
    {
        continue
    }

    Remove-Item $item.FullName
}

explorer $outputDir
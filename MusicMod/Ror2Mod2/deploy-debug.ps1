param (
    [string]$WorkingDir = ".", 
    [string]$ProfileName = $(throw "ProfileName is required."),
    [string]$ModName = $(throw "ModName is required.")
)

$ErrorActionPreference = "Stop"

$inputDir = Join-Path $WorkingDir "bin\Debug\netstandard2.0"
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

explorer $outputDir
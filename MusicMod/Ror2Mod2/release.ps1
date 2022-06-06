function Delete-Files {
    
    param (
        [string]$Pattern
    )

    Remove-Item .\bin\Release\* -Include $Pattern -Recurse
}

Delete-Files *.pdb
Delete-Files BepInEx.*
Delete-Files 0Harmony.*
Delete-Files HG*
Delete-Files *Unity*
Delete-Files MMHOOK*
Delete-Files MonoMod*
Delete-Files R2API*
Delete-Files Rewired*
Delete-Files RiskOfOptions*

foreach ($File in Get-ChildItem "D:\Program Files (x86)\Steam\steamapps\common\Risk of Rain 2\Risk of Rain 2_Data\Managed")
{
    Delete-Files $File.Name
}

foreach ($Directory in Get-ChildItem .\bin\Release\netstandard2.0 -Directory)
{
    Remove-Item $Directory.FullName -Recurse
}
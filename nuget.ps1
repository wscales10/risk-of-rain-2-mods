param ($ProjectDirectory, $TopLevelDirectory = ".", [switch]$PreBuild = $false, [switch]$PostBuild = $false)

$nuget = "D:\Program Files (x86)\NuGet\nuget.exe"
$xmlsettings = New-Object System.Xml.XmlWriterSettings
$xmlsettings.Encoding = [System.Text.Encoding]::GetEncoding(1252)
$xmlsettings.Indent = $true
$xmlsettings.OmitXmlDeclaration = $true

function Get-Xml {
    
    [OutputType([xml])]
    param (
        $File
    )

    return Get-Content $File.FullName
}

function Get-ProjectFile {
    
    param (
        [string]$ProjectDirectoryString
    )

    Write-Output "Getting project file in $ProjectDirectoryString"

    $ProjectFiles = Get-ChildItem "$ProjectDirectoryString\*" -Include *.csproj

    
    if ($ProjectFiles.Count -gt 1) 
    {
        throw
    }
    elseif ($ProjectFiles.Count -eq 1)
    {
        return $ProjectFiles[0]
    }
    else
    {
        return $null
    }
}

function Increment-Version {
    
    param (
        [string]$ProjectDirectoryString
    )

    $ProjectFile = Get-ProjectFile $ProjectDirectoryString
    
    if ($ProjectFile -ne $null)
    {
        [xml]$xmlDoc = Get-Xml $ProjectFile
            
        $VersionNode = $xmlDoc.SelectSingleNode("/Project/PropertyGroup/Version")

        if ($VersionNode -ne $null)
        {
            $OldVersion = [version]$VersionNode.InnerText
            $NewVersion = "$($OldVersion.Major).$($OldVersion.Minor).$($OldVersion.Build + 1)"
                
            if ($OldVersion.Revision -ne -1)
            {
                $NewVersion += ".$($OldVersion.Revision)"
            }

            $VersionNode.InnerText = $NewVersion
            $XmlWriter = [System.XML.XmlWriter]::Create($ProjectFile.FullName, $xmlsettings)
            $xmlDoc.Save($XmlWriter)
        }
    }
}

function Add-Package {
    
    param (
        [string]$ProjectDirectoryString
    )

    $OutputDirectory = Join-Path $ProjectDirectoryString "bin/Debug/"
    if(Test-Path $OutputDirectory)
    {
        $package = Get-ChildItem $OutputDirectory\* -Include *.nupkg | Sort-Object -Property CreationTime | Select-Object -Last 1

        if ($package -ne $null)
        {
            $packageName = $package.FullName
            & $nuget add ${packageName} -source $TopLevelDirectory\nuget\packages 
        }
    }
}

function Update-Package {

    param (
        [string]$ProjectDirectoryString
    )

    $ProjectName = (Get-ProjectFile $ProjectDirectoryString).BaseName

    foreach ($SolutionDirectory in Get-ChildItem $TopLevelDirectory -Directory -Exclude nuget)
    {
        foreach ($OtherProjectDirectory in Get-ChildItem $SolutionDirectory -Directory)
        {
            $ProjectFile = Get-ProjectFile $OtherProjectDirectory.FullName
            
            if ($ProjectFile -ne $null)
            {
                [xml]$xmlDoc = Get-Xml $ProjectFile

                $PackageRef = $xmlDoc.SelectSingleNode("/Project/ItemGroup/PackageReference[@Includes='$ProjectName']")

                if ($PackageRef -ne $null)
                {
                    dotnet add $ProjectFile.FullName package $ProjectName
                }
            }
        }
    }
}

function Execute-Steps {
    
    param (
        [string]$ProjectDirectoryString
    )

    if ($PreBuild)
    {
        Increment-Version -ProjectDirectoryString $ProjectDirectoryString
    }

    if ($PostBuild)
    {
        Add-Package -ProjectDirectoryString $ProjectDirectoryString
        Update-Package -ProjectDirectoryString $ProjectDirectoryString
    }
}

if($ProjectDirectory -eq $null)
{
    Write-Output "Executing steps for all projects in $TopLevelDirectory"
    foreach ($SolutionDirectory in Get-ChildItem $TopLevelDirectory -Directory -Exclude nuget)
    {
        foreach ($ProjectDirectory in Get-ChildItem $SolutionDirectory -Directory)
        {
            Execute-Steps -ProjectDirectoryString $ProjectDirectory.FullName
        }
    }
}
else
{
    Write-Output "Executing steps for $ProjectDirectory"
    Execute-Steps -ProjectDirectoryString $ProjectDirectory
}

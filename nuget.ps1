param ($ProjectDirectory, $TopLevelDirectory = ".", [switch]$PreBuild = $false, [switch]$PostBuild = $false, [switch]$AddBuildEvents)

$DebugPreference = "Continue"

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

function Save-Xml {

    param (
        $XmlDocument,
        $File
    )

    $XmlWriter = [System.XML.XmlWriter]::Create($File.FullName, $xmlsettings)
    $XmlDocument.Save($XmlWriter)
}

function Get-ProjectFile {
    
    param (
        [string]$ProjectDirectoryString
    )

    Write-Debug "Getting project file in $ProjectDirectoryString"

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

function Check-VersionExists {
    
    param (
        [string]$ProjectName,
        [version]$Version
    )

    [version[]]$ExistingVersions = Get-ChildItem "$TopLevelDirectory\nuget\packages\$ProjectName" | Get-ItemPropertyValue -Name Name

    foreach ($ExistingVersion in $ExistingVersions)
    {
        if ($ExistingVersion -eq $Version)
        {
            return $true
        }
    }

    return $false
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

            if (Check-VersionExists -ProjectName $ProjectFile.BaseName -Version $OldVersion)
            {
                $NewVersion = "$($OldVersion.Major).$($OldVersion.Minor).$($OldVersion.Build + 1)"
                
                if ($OldVersion.Revision -ne -1)
                {
                    $NewVersion += ".$($OldVersion.Revision)"
                }

                $VersionNode.InnerText = $NewVersion
                Save-Xml -XmlDocument $xmlDoc -File $ProjectFile
            }
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
        $package = Get-ChildItem $OutputDirectory\* -Include *.nupkg | Sort-Object -Property LastWriteTime | Select-Object -Last 1

        if ($package -ne $null)
        {
            $packageName = $package.FullName
            & $nuget add ${packageName} -source $TopLevelDirectory\nuget\packages 
            Write-Debug "Added $($package.BaseName) to local source"
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

                $PackageRef = $xmlDoc.SelectSingleNode("/Project/ItemGroup/PackageReference[@Include='$ProjectName']")

                if ($PackageRef -ne $null)
                {
                    dotnet add $ProjectFile.FullName package $ProjectName --source $TopLevelDirectory\nuget\packages
                    Write-Debug "Updated $ProjectName in $($ProjectFile.Name)"
                }
            }
        }
    }
}

function Add-BuildEvents {
    
    param (
        $ProjectFile
    )

    [xml]$xmlDoc = Get-Xml $ProjectFile

    [System.Xml.XmlElement]$ProjectElement = $xmlDoc.SelectSingleNode("/Project")

    $PreBuild = $xmlDoc.SelectSingleNode("/Project/Target[@Name='PreBuild']")

    if ($PreBuild -eq $null)
    {
        $TargetElement = $xmlDoc.CreateElement("Target")
        $TargetElement.SetAttribute("Name", "PreBuild")
        $TargetElement.SetAttribute("BeforeTargets", "PreBuildEvent")

        $ExecElement = $xmlDoc.CreateElement("Exec")
        $ExecElement.SetAttribute("Command", "PowerShell -ExecutionPolicy Unrestricted -File ..\..\nuget.ps1 -ProjectDirectory . -TopLevelDirectory ..\.. -PreBuild")
        
        $TargetElement.AppendChild($ExecElement)

        $ProjectElement.AppendChild($TargetElement)
    }

    $PostBuild = $xmlDoc.SelectSingleNode("/Project/Target[@Name='PostBuild']")

    if ($PostBuild -eq $null)
    {
        $TargetElement = $xmlDoc.CreateElement("Target")
        $TargetElement.SetAttribute("Name", "PostBuild")
        $TargetElement.SetAttribute("AfterTargets", "PostBuildEvent")

        $ExecElement = $xmlDoc.CreateElement("Exec")
        $ExecElement.SetAttribute("Command", "PowerShell -ExecutionPolicy Unrestricted -File ..\..\nuget.ps1 -ProjectDirectory . -TopLevelDirectory ..\.. -PostBuild")
        
        $TargetElement.AppendChild($ExecElement)
        
        $ProjectElement.AppendChild($TargetElement)
    }

    Save-Xml -XmlDocument $xmlDoc -File $ProjectFile
}

function Execute-Steps {
    
    param (
        [string]$ProjectDirectoryString
    )

    if($AddBuildEvents)
    {
        $ProjectFile = Get-ProjectFile $ProjectDirectoryString
        Add-BuildEvents -ProjectFile $ProjectFile
    }

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
    Write-Debug "Executing steps for all projects in $TopLevelDirectory"
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
    Write-Debug "Executing steps for $ProjectDirectory"
    Execute-Steps -ProjectDirectoryString $ProjectDirectory
}

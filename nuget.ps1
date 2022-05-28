$nuget = "D:\Program Files (x86)\NuGet\nuget.exe"

foreach ($parent in Get-ChildItem . -Directory -Exclude nuget)
{
    foreach ($Directory in Get-ChildItem $parent -Directory)
    {
        $OutputDirectory = Join-Path $directory.FullName "bin/Debug/"
        if(Test-Path $OutputDirectory)
        {
            foreach ($package in Get-ChildItem $OutputDirectory\* -Include *.nupkg)
            {
                $packageName = $package.FullName
                & $nuget add ${packageName} -source .\nuget\packages
            }
        }
    }
}
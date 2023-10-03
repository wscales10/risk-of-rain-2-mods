param (
    [string]$Configuration = $(throw "Configuration is required.")
)

$ErrorActionPreference = "Stop"

dotnet build ..\Common\Common.sln -c $Configuration
dotnet build MusicMod.sln -c $Configuration
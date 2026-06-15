# Builds Snipit in Release and packages it as a multi-targeted .yak for Rhino 8 (Windows).
# Output: dist\snipit-<version>-rh8_0-win.yak
# Usage:  powershell -ExecutionPolicy Bypass -File build-yak.ps1

$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$dist = Join-Path $root 'dist'
$yak  = 'C:\Program Files\Rhino 8\System\Yak.exe'

if (-not (Test-Path $yak)) { throw "Yak.exe not found at $yak (is Rhino 8 installed?)" }

# 1. Clean build, Release configuration (builds all target frameworks)
dotnet build "$root\Snipit.csproj" -c Release
if ($LASTEXITCODE -ne 0) { throw 'dotnet build failed' }

# 2. Assemble the package layout:
#    dist\manifest.yml, icon.png, net48\Snipit.gha, net7.0\Snipit.gha, misc\...
if (Test-Path $dist) { Remove-Item $dist -Recurse -Force }
New-Item -ItemType Directory -Path "$dist\net48", "$dist\net7.0", "$dist\misc" | Out-Null

Copy-Item "$root\manifest.yml" $dist
Copy-Item "$root\Resource\icon.png" $dist
Copy-Item "$root\bin\Release\net48\Snipit.gha" "$dist\net48"
# Rhino 8 on the .NET (Core) runtime loads from the net7.0 folder;
# the net7.0-windows build is the right assembly for it on Windows.
Copy-Item "$root\bin\Release\net7.0-windows\Snipit.gha" "$dist\net7.0"
Copy-Item "$root\LICENSE", "$root\README.md" "$dist\misc"

# 3. Build the .yak (Windows-only distribution tag)
Push-Location $dist
try {
    & $yak build --platform win
    if ($LASTEXITCODE -ne 0) { throw 'yak build failed' }
}
finally { Pop-Location }

Get-ChildItem "$dist\*.yak" | ForEach-Object { "Package ready: $($_.FullName)" }

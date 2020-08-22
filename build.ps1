<#
.SYNOPSIS
    ICsi Build Script
.DESCRIPTION
    The build script for ICsi
.PARAMETER target
    The build target. Available values are: default, restore, build, clean.
.PARAMTER configuration
    The build configurations. The only values available are: Debug and Release.
.NOTES
    Author: Iurie Jurja
    License: MIT License
    Date: 20.08.2020
.EXAMPLE
    .\build.ps1 -target build -configuration Release
#>

param([string] $target = "default",
      [string] $configuration = "Debug")

$buildNumber = [System.DateTime]::Now.ToString("MMdd")
$version = "1.0.${buildNumber}"

function Clean {
    dotnet clean ICsi.sln --configuration $configuration --output .\artifacts\$configuration
}

function Restore {
    dotnet restore ICsi.sln
}

function Build {
    dotnet build ICsi.sln --configuration $configuration --output .\artifacts\$configuration --no-restore /p:Version=$version
}

if ($target -eq "default") {
    Restore
    Build
} elseif($target -eq "clean") {
    Clean
} elseif ($target -eq "restore") {
    Restore
} elseif ($target -eq "build") {
    Build
} else {
    Write-Host "Unrecognized target: ${target}"
}

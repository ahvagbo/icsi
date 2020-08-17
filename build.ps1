param([string] $target = "default",
      [string] $configuration = "Debug")

function Clean {
    [System.IO.Directory]::Delete("${pwd}\\artifacts\\${configuration}", $true)
}

function Restore {
    dotnet restore ICsi.sln
}

function Build {
    dotnet build ICsi.sln --configuration $configuration --output .\artifacts\$configuration
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
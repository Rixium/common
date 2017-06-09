[CmdletBinding()]
Param(
    [switch]$NoInit,
    [string]$Target,
    [ValidateSet("Release", "Debug")]
    [string]$Configuration,
    [ValidateSet("Quiet", "Minimal", "Normal", "Verbose")]
    [string]$Verbosity,
    [Parameter(Position=0,Mandatory=$false,ValueFromRemainingArguments=$true)]
    [string[]]$ScriptArgs
)

Set-StrictMode -Version 2.0; $ErrorActionPreference = "Stop"; $ConfirmPreference = "None"; trap { $host.SetShouldExit(1) }
$PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent

###########################################################################
# CONFIGURATION
###########################################################################

$NuGetUrl = "_NUGET_URL_"
$SolutionDirectory = "$PSScriptRoot\_SOLUTION_DIRECTORY_"
$BuildProjectFile = "$PSScriptRoot\_BUILD_DIRECTORY_NAME_\_BUILD_PROJECT_NAME_.csproj"
$BuildExeFile = "$PSScriptRoot\_BUILD_DIRECTORY_NAME_\bin\debug\_BUILD_PROJECT_NAME_.exe"
$TempDirectory = "$PSScriptRoot\_ROOT_DIRECTORY_\.tmp"

###########################################################################
# PREPARE BUILD
###########################################################################

function ExecSafe([scriptblock] $cmd) {
    & $cmd
    if ($LastExitCode -ne 0) { throw "The following call failed with exit code $LastExitCode. '$cmd'" }
}

if (!$NoInit) {
    md -force $TempDirectory > $null

    $NuGetFile = "$TempDirectory\nuget.exe"
    $env:NUGET_EXE = $NuGetFile
    if (!(Test-Path $NuGetFile)) { (New-Object System.Net.WebClient).DownloadFile($NuGetUrl, $NuGetFile) }
    elseif ($NuGetUrl.Contains("latest")) { & $NuGetFile update -Self }

    ExecSafe { & $NuGetFile restore $BuildProjectFile -SolutionDirectory $SolutionDirectory }
    ExecSafe { & $NuGetFile install Nuke.MSBuildLocator -ExcludeVersion -OutputDirectory $TempDirectory -SolutionDirectory $SolutionDirectory }
}

$MSBuildFile = & "$TempDirectory\Nuke.MSBuildLocator\tools\Nuke.MSBuildLocator.exe"
ExecSafe { & $MSBuildFile $BuildProjectFile }

###########################################################################
# EXECUTE BUILD
###########################################################################

ExecSafe { & $BuildExeFile "-Target=$Target" "-Configuration=$Configuration" "-Verbosity=$Verbosity" $ScriptArgs }
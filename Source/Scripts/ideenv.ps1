# Version 1.0
# https://github.com/RobertBouillon/VsProjectPowershell/blob/main/README.md#Installation

$root_path=(git rev-parse --show-toplevel).replace("/","\")                                                               #Path to the root of the project, as defined by GIT
$solution_path=(Get-Childitem *.sln -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1).Directory.FullName   #Path to the source directory: searches for the first subfolder with a .sln file. Hard-code for better performance.
$source_path=$solution_path                                                                                                #Path to the source directory: searches for the first subfolder with a .sln file. Hard-code for better performance.
$script_path = "$($PSScriptRoot)\$($MyInvocation.MyCommand.Name)"                                                         #Full path to this file
$env:ASPNETCORE_ENVIRONMENT = "Development"                                                                               #Important if using `dotnet watch`: Disables caching - JS files are otherwise cached.
function prompt {"PS $(Split-Path -Path $pwd -Leaf)> "}                                                                   #Prompt with current directory name
#function prompt {"PS> "}                                                                                                 #Alternate Simple Prompt for smaller screens
$project_name = (Get-Item $root_path).Name
$Host.UI.RawUI.WindowTitle = "$project_name Project Shell"


function assets()
{
  start "$root_path\Assets"
}

function docs()
{
  code "$root_path\Docs"
}

function clean()
{
  Get-ChildItem .\ -include bin,obj -Recurse | foreach { remove-item $_.fullname -Force -Recurse }
}

function builds()
{
  start "$root_path\Builds"
}

function popout()
{
  param ([switch]$admin)

  Push-Location "$source_path"
  $psargs="& { Import-Module '$env:VSAPPIDDIR\..\Tools\Microsoft.VisualStudio.DevShell.dll'}; Enter-VsDevShell -SkipAutomaticLocation -SetDefaultWindowTitle -InstallPath '$env:VSAPPIDDIR\..\..\'; cd '$source_path'; clear ; if (Test-Path '$script_path') { . '$script_path' };"

  if (-not $admin)
  {
    Start-Process powershell "-NoExit -Command $psargs"
  }
  else
  {
    Start-Process -verb RunAs powershell "-NoExit -Command $psargs"
  }

  #cmd /c start powershell -NoExit -Command "& { Import-Module `$env:VSAPPIDDIR\..\Tools\Microsoft.VisualStudio.DevShell.dll}; Enter-VsDevShell -SkipAutomaticLocation -SetDefaultWindowTitle -InstallPath `$env:VSAPPIDDIR\..\..\; clear; if (Test-Path .\utils.ps1) { . .\utils.ps1 };"
  Pop-Location
}

function isElevated()
{
  $currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
  return $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function build()
{
  push-location "$solution_path\BackupHostService6"
  dotnet publish -r win-x64 -c Release -o "$root_path\Builds\Working"
  start "$root_path\Builds\Working"
  pop-location
}

######################
#  Project-Specific  #
######################

$service_name="Route 53 Dynamic IP"
$event_log_name="Route 53 Dynamic IP"
$service_install_path="C:\Program Files\Route 53 Dynamic IP"

function deploy()
{
  $admin=isElevated
  if (-not $admin)
  {
    Write-Output "Must run as admin (try popout -admin)"
    return
  }

  $deploy_path="$root_path\Builds\Deploy"

  $source_exists = [System.Diagnostics.EventLog]::SourceExists($event_log_name)
  if(-not $source_exists)
  {
    New-EventLog -LogName Application -Source $event_log_name
  }

  push-location "$source_path\Route53DynamicIpService"
  dotnet publish -r win-x64 -c Release -o $deploy_path
  pop-location
  NET stop $service_name
  del "$service_install_path\*.*"
  cp "$deploy_path\*.*" "$service_install_path\" -force   #Overwrite because if we delete, first, then we delete the configuration file.
  NET start $service_name
}
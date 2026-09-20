# Use the persistent Blender setting even from an already-running editor whose
# environment predates setup. Pass every argument through to the installed CLI.
$ErrorActionPreference = 'Stop'
if (-not $env:BLENDER_PATH) {
    $env:BLENDER_PATH = [Environment]::GetEnvironmentVariable('BLENDER_PATH', 'User')
}
$gameDevCommand = Get-Command game-dev.cmd -ErrorAction Stop
& $gameDevCommand.Source @args
exit $LASTEXITCODE

param([string[]]$Suites = @('Physics','Finish','Rush','Stunt','AirahMobile','AirahQuick','Overhaul'))
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$auditRoot = Join-Path $projectRoot 'Evidence\GameplayAudit'
$runtimeRoot = Join-Path $auditRoot 'Runtime'
$playerRoot = Join-Path $runtimeRoot 'Build'
New-Item -ItemType Directory -Force -Path $playerRoot | Out-Null
# Isolate the older suites' relative Evidence paths, preserving existing captures.
Get-ChildItem -LiteralPath (Join-Path $projectRoot 'Build') -File |
    Where-Object { $_.Extension -in '.exe','.dll' } |
    Copy-Item -Destination $playerRoot
foreach ($dependency in @('MonoBleedingEdge','D3D12')) {
    $dependencyPath = Join-Path $playerRoot $dependency
    if (-not (Test-Path -LiteralPath $dependencyPath)) {
        New-Item -ItemType Junction -Path $dependencyPath -Target (Join-Path (Join-Path $projectRoot 'Build') $dependency) | Out-Null
    }
}
$dataLink = Join-Path $playerRoot 'Aether Grounds_Data'
if (-not (Test-Path -LiteralPath $dataLink)) {
    New-Item -ItemType Junction -Path $dataLink -Target (Join-Path $projectRoot 'Build\Aether Grounds_Data') | Out-Null
}
$flags = @{
    Physics='-aetherPhysicsTest'; Finish='-aetherFinishTest'; Rush='-aetherRushTest';
    Stunt='-aetherStuntTest'; AirahMobile='-aetherAirahMobileTest';
    AirahQuick='-aetherAirahTest -aetherAirahQuick'; Overhaul='-aetherOverhaulTest'
}
New-Item -ItemType Directory -Force -Path (Join-Path $runtimeRoot 'Evidence\Airah') | Out-Null
$results = @()
foreach ($suite in $Suites) {
    if (-not $flags.ContainsKey($suite)) { throw "Unknown suite: $suite" }
    $log = Join-Path $auditRoot ($suite.ToLower() + '-regression.log')
    $argsForPlayer = $flags[$suite] + ' -logFile "' + $log + '"'
    $watch = [Diagnostics.Stopwatch]::StartNew()
    $process = Start-Process -FilePath (Join-Path $playerRoot 'Aether Grounds.exe') -ArgumentList $argsForPlayer -WindowStyle Hidden -PassThru
    if (-not $process.WaitForExit(780000)) {
        Stop-Process -Id $process.Id
        throw "$suite exceeded its 13-minute bound"
    }
    $process.Refresh()
    $lines = @(Get-Content -LiteralPath $log)
    $passed = @($lines | Where-Object { $_ -match '\bPASS\b' }).Count
    $failed = @($lines | Where-Object { $_ -match '\bFAIL\b|TIMEOUT' })
    $result = [pscustomobject]@{suite=$suite;exitCode=$process.ExitCode;passes=$passed;failures=$failed;seconds=[Math]::Round($watch.Elapsed.TotalSeconds,1);log=$log}
    $results += $result
    $results | ConvertTo-Json -Depth 4 | Set-Content -Encoding UTF8 (Join-Path $auditRoot 'regression-summary.json')
    Write-Output ($result | ConvertTo-Json -Compress -Depth 4)
}
if (@($results | Where-Object { $_.exitCode -ne 0 -or $_.failures.Count -gt 0 }).Count -gt 0) { exit 1 }

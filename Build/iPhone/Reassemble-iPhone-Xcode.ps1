$ErrorActionPreference = 'Stop'
$outputPath = Join-Path $PSScriptRoot 'Aether-Grounds-Xcode.zip'
$parts = 1..3 | ForEach-Object { Join-Path $PSScriptRoot ('Aether-Grounds-Xcode.zip.part{0:000}' -f $_) }
$output = [System.IO.File]::Create($outputPath)
try {
    foreach ($part in $parts) {
        $input = [System.IO.File]::OpenRead($part)
        try { $input.CopyTo($output) } finally { $input.Dispose() }
    }
} finally { $output.Dispose() }
$expected = '41E0767288EAE76304B37009A7C5089B3E938704323186F4428D1B3DC1F5B350'
$actual = (Get-FileHash -Algorithm SHA256 -LiteralPath $outputPath).Hash
if ($actual -ne $expected) { throw "Checksum failed: $actual" }
Write-Host "Created and verified: $outputPath"

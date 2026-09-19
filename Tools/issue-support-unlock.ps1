param([Parameter(Mandatory=$true)][string]$PlayerId,[Parameter(Mandatory=$true)][ValidateSet('pip-city','comet-chrome','nomad-night','manta-gold','neon-wheels','star-trail','vanta-gold','vanta-chrome','glass-wheels','glass-chassis')][string]$Product,[Parameter(Mandatory=$true)][string]$PayPalTransaction,[Parameter(Mandatory=$true)][switch]$PaymentVerified)
$ErrorActionPreference='Stop'
if (!$PaymentVerified) { throw 'Verify completed payment, recipient, USD amount, product and Player ID in PayPal before issuing a code.' }
if ($PlayerId -notmatch '^[a-f0-9]{16}$' -or $PayPalTransaction -notmatch '^[A-Za-z0-9-]{4,80}$') { throw 'Invalid Player ID or transaction ID.' }
$keyDir=Join-Path $env:LOCALAPPDATA 'AetherGroundsOwner'
$ledger=Join-Path $keyDir 'issued-transactions.json'
$entries=@(); if(Test-Path -LiteralPath $ledger){$entries=@(Get-Content -LiteralPath $ledger -Raw | ConvertFrom-Json)}
$existing=$entries | Where-Object {$_.transaction -eq $PayPalTransaction}
if($existing){if($existing.player -ne $PlayerId -or $existing.product -ne $Product){throw 'This transaction already fulfilled a different player or product.'};$existing.code;return}
$rsa=[System.Security.Cryptography.RSACryptoServiceProvider]::new(2048)
$rsa.FromXmlString((Get-Content -LiteralPath (Join-Path $keyDir 'support-private.xml') -Raw))
$payload=[System.Text.Encoding]::UTF8.GetBytes("AG1|$PlayerId|$Product|$PayPalTransaction")
$signature=$rsa.SignData($payload,[System.Security.Cryptography.CryptoConfig]::MapNameToOID('SHA256'))
$code=[Convert]::ToBase64String($payload)+'.'+[Convert]::ToBase64String($signature)
$entries+=@{transaction=$PayPalTransaction;player=$PlayerId;product=$Product;code=$code;issued=(Get-Date).ToUniversalTime().ToString('o')}
ConvertTo-Json -InputObject @($entries) -Depth 4 | Set-Content -LiteralPath $ledger
$rsa.Dispose()
$code

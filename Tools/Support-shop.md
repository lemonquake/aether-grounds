# Support shop fulfillment

The Store offers six special editions ($0.99–$9.99 USD), a neon wheel kit ($0.99), a Starlight boost trail ($1.99), glass wheels ($2.99), and a glass chassis ($4.99). Every product has an in-game 3D preview and real performance bonuses listed in the Store and README. Ownership allows equipping on multiple saved cars. Selecting an edition replaces the current saved car's base body; upgrades remain editable. Gold, chrome and glass finishes override the relevant paint while equipped.

New product IDs are `vanta-gold` ($9.99), `vanta-chrome` ($7.99), `glass-wheels` ($2.99), and `glass-chassis` ($4.99). The issuer accepts these alongside all existing IDs. Existing player-bound signed unlocks remain valid.

Buy with PayPal opens a Website Payments Standard payment link for `lemonquake@gmail.com`, carrying the item, USD amount and the player's 16-character Player ID. The player completes payment in their own browser. No purchase was made during development. Recipient account eligibility and live settlement require an actual merchant account check; they cannot be verified using only an email address.

Fulfillment is explicitly **manual** in the interface. There is no claim of automatic payment verification, and browser return or a receipt number never grants ownership. The user emails Aljay their receipt and Player ID. Aljay verifies the transaction in PayPal, including Completed status, recipient, exact USD amount, SKU and Player ID. Underpayment, pending payments and refunds must not be fulfilled.

After verification, run:

```powershell
./Tools/issue-support-unlock.ps1 -PlayerId PLAYER_ID -Product pip-city -PayPalTransaction TRANSACTION_ID -PaymentVerified
```

Send the resulting code to that player. They paste it into Store, choose Unlock, select the item and choose Equip on saved car. Remove item unequips it. Repeated redemption does not award currency or duplicate cars.

The issuer records transaction IDs and refuses to reuse one for a different player or SKU. The code contains a SHA-256/RSA signature and is bound to the Player ID. Only the public key is packaged in the game. The private signing key and issued-transaction ledger are stored under `%LOCALAPPDATA%/AetherGroundsOwner`, outside the project. Back these up privately. Do not distribute the private key. Keeping that key preserves existing unlocks across builds. The local offline model cannot revoke previously issued codes after a refund, and reinstalling without the original PlayerPrefs requires restoring the Player ID or arranging a replacement code.

For automatic fulfillment later, add a server using verified PayPal capture/webhook events and a durable transaction/entitlement database. The owner key belongs on that server, never in the player. Current reference: https://developer.paypal.com/api/rest/webhooks/rest/

Payment-link parameters follow PayPal's Website Payments Standard guide: https://developer.paypal.com/upgrade/wps/guide/Simple%20Buy%20Button/

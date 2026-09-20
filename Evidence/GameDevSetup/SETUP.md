# Game Development Studio and Blender setup

Completed 21 September 2026 (Asia/Manila).

## Installed configuration

- CLI: `game-dev` 1.0.2, built from the [official source repository](https://github.com/theisegoria/game-development-studio/tree/96a0b4f34b979279ab983e9547af43133e85f310).
- Source commit: `96a0b4f34b979279ab983e9547af43133e85f310`.
- Installation: `C:\Users\Lemon PC\AppData\Local\game-development-studio`.
- Global command: `C:\Users\Lemon PC\AppData\Roaming\npm\game-dev.ps1` (also `.cmd`). The npm directory was already on the persistent user PATH.
- Runtime: Node.js 26.3.0.
- Blender: `C:\Program Files\Blender Foundation\Blender 5.2\blender.exe`, reporting **5.2.1 LTS**.
- Persistent user environment variable: `BLENDER_PATH` points to that executable.
- Project wrapper: `Tools/game-dev.ps1` reads the persistent Blender setting when the calling editor still has its old environment.

The documented npm registry package returned HTTP 404, so installation used `git clone`, the locked dependencies via `npm ci` (which built the TypeScript CLI), and `npm link`. No unrelated similarly named npm package was substituted. Production dependency audit reported zero vulnerabilities. The full development dependency install reported one high-severity advisory; development dependencies have not been automatically upgraded beyond the upstream lockfile.

The Computer plugin launched Blender and confirmed its native window title through accessibility. Native screenshot capture failed with `SetIsBorderRequired failed: No such interface supported (0x80004002)`. This does not block the verified background CLI integration. No Blender add-on or listening server is needed for this CLI: it launches the configured Blender executable for local operations.

## Verification

1. `game-dev --version` returned `1.0.2`.
2. `doctor` returned `healthy: true`, with Blender, the normalizer script, the USD exporter script, SQLite runtime, and CLI version checks passing. See `doctor.json`.
3. Inspected the actual project wheel source, `ArtSource/Wheels/AetherWheels.glb`. See `wheel-inspect.json`.
4. Ran a real Blender normalization/export into a separate connection-test file. See `blender-normalize.jsonl` and `AetherWheels-connection-test.glb`.
5. Reinspected and validated the result. See `normalized-inspect.json` and `normalized-validation.json`.

| Measurement | Source | Connection-test export |
| --- | ---: | ---: |
| Triangles | 56,936 | 56,936 |
| Animations | 20 | 20 |
| Materials | 7 | 7 |
| Mesh objects missing UVs | 389 | 0 |
| Validation checks passed | Not asserted | 8 of 8 |

Output SHA-256: `4d6dfdae2eba287f0ed7d2ebe9f91eb1b3e5ea34a756ba359734c18e83f726af`.

The connection test leaves the game's source GLB and runtime JSON assets unchanged. Its validation uses the CLI's default asset policy; it is not a gameplay, wheel-contact, animation-appearance, or performance acceptance test. Materials use colors without image textures, which the inspector reports separately from validation failures.

Optional Tripo/Leonardo credentials are not configured. No paid generation or external asset upload occurred. macOS Metal capture and `usdzip` are unavailable on this Windows machine. The doctor's legacy `.codex/skills` check does not discover the already-installed Game Development Studio plugin skills; no duplicate skills were installed to satisfy that warning.

## Commands for the next agent

From `A:\Python\aether-grounds`, the wrapper works immediately even in an editor that was running before setup:

```powershell
& .\Tools\game-dev.ps1 --version
& .\Tools\game-dev.ps1 doctor --output-dir Evidence\GameDevSetup\workspace --json
& .\Tools\game-dev.ps1 capabilities --output-dir Evidence\GameDevSetup\workspace --json
& .\Tools\game-dev.ps1 asset inspect ArtSource\Wheels\AetherWheels.glb --output-dir Evidence\GameDevSetup\workspace --json
& .\Tools\game-dev.ps1 asset validate Evidence\GameDevSetup\AetherWheels-connection-test.glb --output-dir Evidence\GameDevSetup\workspace --json
```

New applications that inherit the updated user environment can use `game-dev` directly. Restarting an existing editor is optional when using the project wrapper.

Use unique output paths for additional exports. Do not overwrite the connection-test evidence or promote the normalized collection into the game without reviewing the runtime asset import path. This collection contains all ten wheel designs; its aggregate triangle count is not the cost of one installed wheel.

## Remaining game work

CLI setup is complete. The larger gameplay audit and vehicle overhaul remain in progress and must not be marked complete from these setup checks.

- The WheelCollider launch regression was subsequently repaired and verified: see `Evidence/VehiclePhysics/LAUNCH-REGRESSION.md`. Rolling resistance and Drift no longer leave service brakes engaged during launches.
- Finish traffic wheel integration, replace the remaining civilian wheel geometry, and verify facing and wheel contact. The `WheelVisual.Radius` accessor and Kenney wheel mounting metadata are now present.
- Garage chassis, spring, damper, suspension upgrades and assembled part weights are now exposed in source; complete visual and behavior verification.
- Resolve gameplay fixture failures, run the full regression suite, and verify all ten bodies and all driving modes before accepting the physics overhaul.
- Produce the complete gameplay audit/improvement report and next-agent handoff, clearly separating implemented behavior, measured evidence, tuning assumptions and unfinished work.

The CLI is now available for those asset checks. Live Blender UI screenshots still need a functioning Computer capture backend before visual acceptance can be claimed through that route.

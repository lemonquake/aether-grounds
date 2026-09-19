# Airah Mountains verification — 19 September 2026

Final Windows and Android builds succeeded with zero build errors. Android APK signature verification succeeded (v2 signature); package `com.aljayleodones.aethergrounds`, version 1.5.0, code 6, ARM64, Android API 26 minimum. APK size: 36,508,675 bytes. SHA-256: `1DF01C976D34BA29468B5CE0444D921FD26EC7C79D69FAB6EA03AA41723EEBD4`.

## Final checks

- `test-result.txt` and `player-test.log`: zero failures. A full physics-driven 10.38 km lap finished with ordered checkpoints; 20 racers loaded. All non-gap road samples had collision and no terrain covering the center lane. Maximum sampled tangent grade was 0.15.
- `quick-result.txt` and `final-performance.log`: zero failures. The boosted bridge jump crossed the real gap and reached the landing deck. A missed jump returned to sample 27 on the approach.
- `mobile-result.txt` and `mobile-test.log`: zero failures. Four directional buttons support simultaneous steering, throttle, drift and boost; releasing and pausing clear input. Upside-down cars did not auto-rotate in air, did not recover before one second of support, and did recover on a raised object. Leaving support resets the timer.
- Scene counts: 12,554 trees, 5,746 rocks, 18 physical civilian vehicles. Ferns, logs, shelters, flowers, signs and bridge detail are additional. The dedication text and road features were found and visually checked.

## Frame samples

The final sample ran after Android compilation had finished, at 1600×900 with 20 racers on an NVIDIA RTX 4060 and AMD Ryzen 7 5700X. A pre-existing user game instance was left open.

| Mode | Average | 99th-percentile frame expressed as FPS |
|---|---:|---:|
| PC quality | 118.0 FPS | 94.8 FPS |
| Phone quality on this desktop, up to 120 target | 116.0 FPS | 88.2 FPS |

These are ten-second scene samples, not universal minimums. Actual Android FPS is unverified because no physical Android device was connected. Phone mode targets 60, 90 or 120 FPS according to display refresh rate and offers a 60 FPS option in Controls. The earlier full-lap performance samples overlap Android compilation and are retained as recorded.

## Deliverables

- Windows: `Build/Aether Grounds.exe` and its existing adjacent player data.
- Android: `Build/Android/Aether-Grounds.apk`.
- Blender source: `ArtSource/AirahMountains.blend`.
- Texture source and prompts: `Assets/Resources/Airah` and `ArtSource/Airah-art-prompts.md`.
- Game source: `Assets/Scripts/Airah*.cs`, `VehicleRollover.cs`, integrated circuit/UI/traffic/recovery updates, and the Airah shaders.

The initial source backup is `before-airah.zip`. Intermediate logs remain available, including the forest shader stripping issue and recovery verification failure resolved before the final successful checks. Test runs suppress player save writes.

# Aether Grounds: install on your own iPhone

Prepared September 20, 2026. Game version 1.5.0, build 6, including Airah Mountains, the two-deck broken bridge, four directional touch buttons, and grounded-only rollover recovery.

**The supplied source package is not an installable app.** It must become a signed native build before your iPhone can run it. An Xcode project is also a build input, not an IPA. The Android APK cannot run on iPhone. This package does not contain Apple certificates, an Apple Account, or a signed IPA.

The supplied `Aether-Grounds-Xcode.zip` contains a successful Unity iOS export with zero build errors. Use that ZIP for the shortest installation route: Unity does not need to be installed on the Mac for this exported project. The source ZIP is included for future editing and re-exporting. Final Xcode compilation, signing, installation, and physical-device performance have not been tested here.

When downloading from GitHub, the prepared Xcode ZIP is stored as `Aether-Grounds-Xcode.zip.part001`, `.part002`, and `.part003` because GitHub rejected Git LFS uploads for this repository. On a Mac, open Terminal in `Build/iPhone` and run `sh Reassemble-iPhone-Xcode.sh`. On Windows, run `powershell -ExecutionPolicy Bypass -File .\Reassemble-iPhone-Xcode.ps1`. Both scripts recreate `Aether-Grounds-Xcode.zip` and verify its SHA-256 checksum before you extract it.

Unity exports an Xcode project; Xcode compiles and signs the final app on macOS. Windows can prepare the project but cannot perform that final local build. [Unity's iOS build process](https://docs.unity.com/en-us/engine/6000.0/manual/platform-specific/iphone/ios-building-and-delivering/build-process).

## What you need

- Your iPhone, unlocked, with iOS 15 or later. This is the configured minimum; actual compatibility and performance require a device test.
- A Mac capable of running an Xcode version that supports your iPhone's iOS version. Check [Apple's Xcode compatibility table](https://developer.apple.com/xcode/system-requirements) before installing or upgrading anything.
- A USB data cable and an internet connection for initial downloads and signing.
- Your own Apple Account. A paid membership is **not required** for the personal Xcode installation described here. A free Personal Team has provisioning that expires after seven days and allows up to three personal apps on a device; reconnect and rebuild to renew it. [Apple's account limits](https://developer.apple.com/help/account/basics/about-your-developer-account).

If you only have Windows, you need access to a Mac or a properly configured Mac cloud build service to finish the native app. Simply moving this ZIP to Files on the iPhone will not install it. A cloud service still needs Apple signing and a distribution method; no cloud build account or signing service is configured in this project.

## Install with Xcode

1. **Copy the build files to the Mac.** If an `Aether-Grounds-Xcode.zip` was supplied, copy and extract it. Otherwise copy `Aether-Grounds-iPhone-Source.zip`, extract it, and perform the Unity export steps below first. Keep the extracted folder on the Mac's local disk.

2. **Install Xcode** from the Mac App Store or [Apple Developer downloads](https://developer.apple.com/download/all/). Open it once and complete its initial setup, including iOS platform components if requested. Use a version compatible with both the Mac and the iPhone, not merely the oldest version the Mac offers.

3. **Add your Apple Account** in Xcode Settings under Apple Accounts (called Accounts in some versions). Sign in directly in Xcode. Use your Personal Team for free testing, or your existing paid team.

4. **Connect the phone.** Unlock it, connect the USB data cable, and accept the Trust prompt. Select it in Xcode's device manager; newer Xcode versions call this Device Hub / Manage Devices, while older versions use Window > Devices and Simulators. Allow device preparation to finish.

5. **Enable Developer Mode on iOS 16 or later.** After pairing, open iPhone Settings > Privacy & Security > Developer Mode. Enable it, restart when requested, and confirm after restarting. If the setting is absent, initiate pairing in Xcode first. This setting is not present on iOS 15. [Apple's Developer Mode instructions](https://developer.apple.com/documentation/xcode/enabling-developer-mode-on-a-device).

6. **Open `Unity-iPhone.xcodeproj`** inside the extracted Xcode folder. Select the `Unity-iPhone` app target.

7. **Configure Signing & Capabilities.** Enable automatic signing, select your team, and use a unique Bundle Identifier. The supplied identifier is `com.aljayleodones.aethergrounds`; if your team cannot register it, replace it with your own unique identifier, for example `com.yourname.airahgame`. Resolve signing errors shown there before running.

8. **Select `Unity-iPhone` and your physical iPhone** in the toolbar. Choose Product > Run (Command-R). Xcode compiles, signs, installs, and opens the app. Allow the first build to finish. These signing and launch steps follow [Apple's device installation documentation](https://developer.apple.com/documentation/xcode/running-your-app-on-simulated-or-physical-devices).

9. **If the device requests developer trust,** follow its prompt in Settings > General > VPN & Device Management, select your developer identity, and trust it. Return to Xcode and run again. Only approve the identity you just used for this build.

10. **Open the game from the Home Screen.** Once installed, you can disconnect the cable and launch Aether Grounds normally while the provisioning remains valid.

11. **Renew a free installation before or after it expires.** Reconnect the same phone, reopen the same Xcode project, keep the same account and bundle identifier, and run again. Keep the app installed to retain local game data; deleting it removes its local saves. Do not change the identifier for routine updates.

## Export from the supplied Unity source, if needed

1. On the Mac, install [Unity Hub](https://unity.com/download). Install **Unity 6000.6.0f1** and include **iOS Build Support**. Select the editor matching your Mac's processor. `ProjectSettings/ProjectVersion.txt` records the exact version.
2. Extract `Aether-Grounds-iPhone-Source.zip`. In Unity Hub, add the extracted `Aether-Grounds-iPhone-Source` folder, which contains `Assets`, `Packages`, and `ProjectSettings`.
3. Open it with Unity 6000.6.0f1. Allow package restoration and asset import to finish. This package omits generated caches, so the first import takes longer. Resolve any red Console errors before continuing.
4. Open File > Build Profiles, add/select iOS, and switch to that platform. These are Unity's [iOS build profile steps](https://docs.unity.com/en-us/engine/6000.0/manual/platform-specific/iphone/ios-building-and-delivering/build-process).
5. Choose **Aether Grounds > Export iPhone Xcode Project** from the Unity menu. The included `BuildIPhone.cs` configures a real-device Release export with IL2CPP, Metal, landscape orientations, automatic signing, iOS 15 minimum, the game icon, and high-refresh display support. It does not supply an Apple team or certificate.
6. When export completes, find `Build/iPhone/Aether-Grounds-Xcode/Unity-iPhone.xcodeproj` inside the Unity project. Continue with the Xcode installation steps above. If a previous export has manual Xcode changes you need to keep, copy that export elsewhere before exporting again.

For a later source update, repeat the Unity export, then select your team and identifier in the new Xcode project. For free-profile renewal without a source change, you can reuse the existing Xcode project.

## First play and performance check

1. Tap the startup screen, then select **Playing on a Phone**.
2. Open **Game Setup**, choose **Airah Mountains**, and tap **Play**.
3. Use Left/Right to steer, Up to accelerate, and Down to brake/reverse. The separate Accelerate, Brake/Reverse, Drift, Boost, Item, Pause, and Reset car buttons also remain available. Multiple buttons can be held together.
4. Start with the **60 FPS** choice in Controls. On a compatible high-refresh iPhone, try **up to 120 FPS**. The game targets the supported display rate and reduces mobile shadow distance, antialiasing, and rendering resolution. A 60 Hz screen cannot display 120 FPS.
5. Play a full lap, including the broken bridge, then repeat after several minutes. Check that touch controls stay inside the usable screen area, scenery renders, traffic moves, and recovery only happens after a flipped car has supporting contact for one second.
6. Background the app and return to confirm that it pauses safely. Rotate between the two landscape orientations and check the controls near the notch or Dynamic Island.

High refresh is a target, not a measured guarantee. iOS may reduce refresh rate because of power settings or temperature. The export enables the Apple ProMotion plist flag, but physical-device testing is still necessary. [Apple's ProMotion guidance](https://developer.apple.com/documentation/quartzcore/optimizing-iphone-and-ipad-apps-to-support-promotion-displays).

## Common installation problems

| Problem | What to do |
|---|---|
| The ZIP or APK will not install on iPhone | Use the native Xcode build and signing procedure. Neither is an iPhone installer. |
| Xcode does not list the phone | Unlock it, use a data-capable cable, accept Trust, and open the device manager. Check Xcode/iOS compatibility. |
| Developer Mode is missing | Pair with Xcode first. On iOS 15 there is no Developer Mode switch. |
| Signing requires a development team | Select your Apple Account's team for the Unity-iPhone app target. |
| The bundle identifier cannot be registered | Choose a unique identifier owned by your team, then keep it unchanged for future updates. |
| The free account has too many apps or App IDs | Review the Personal Team limits linked above; remove an unused personal test app or wait for the relevant registration limit to expire. |
| The app stops launching after about a week | Renew the free provisioning by rebuilding and reinstalling through Xcode. |
| Performance is uneven | Select 60 FPS, restart the race, and compare after the device cools. Actual performance depends on device hardware and scene load. |
| Unity says iOS support is missing | Add iOS Build Support for the exact editor version in Unity Hub. |
| Xcode reports a compilation error | Copy the first actual error from its build log. Signing success alone does not confirm native compilation. |

## If you want a tap-to-install invitation later

TestFlight is a separate distribution route using an Apple Developer Program account, an uploaded signed build, and App Store Connect setup. After a build is available to you, install Apple's TestFlight app on the iPhone, accept the invitation, and tap Install. TestFlight builds expire after 90 days. No TestFlight build or invitation has been created here. [Apple's TestFlight overview](https://developer.apple.com/testflight/).

For your own phone, the direct Xcode route above avoids setting up TestFlight. Public distribution and store review are separate work; this package does not claim App Store approval.

## Which download should I use?

| File | Purpose | Needed for the first installation? |
|---|---|---|
| `Aether-Grounds-Xcode.zip` | Prepared native project, generated game code, assets, and Mac build tools. Approximately 219 MB compressed and 699 MB extracted. | Yes, for the shortest route. |
| `Aether-Grounds-iPhone-Source.zip` | Editable Unity project, approximately 56 MB compressed. Use it to change the game and generate a new export. | No, if you use the prepared Xcode project. |
| `Aether-Grounds-iPhone-Complete-Guide.md` | This expanded standalone guide. | Keep it beside the ZIP files for reference. |
| `package-manifest.json` | Exact archive sizes and SHA-256 checksums for transfer verification. | Optional. |

The archive size is not the installed app size. Xcode, downloaded platform components, and temporary build output require additional disk space on the Mac. Check available storage before installing the tools. Keep the extracted Xcode folder intact: copying only `Unity-iPhone.xcodeproj` leaves out code and assets that it needs.

The Xcode ZIP contains build tools for both Intel and Apple Silicon Macs. That does not mean every old Mac can run a compatible Xcode release; use Apple's compatibility table linked above.

## Terms used in the installation steps

| Term | Meaning here |
|---|---|
| Unity project | The editable game source, including scripts, artwork, and project settings. |
| Xcode project | The Apple build project generated from Unity. Open this on a Mac to finish the app. |
| Bundle identifier | The app's unique identity, such as `com.yourname.airahgame`. Keep it unchanged for updates. |
| Team | The Apple account or developer organization that signs the app. A free account appears as a Personal Team. |
| Signing | Apple's process for associating the app with a developer identity and allowing installation. |
| Provisioning profile | The installation authorization Xcode creates for the selected team and device. |
| IPA | A packaged iPhone app. An IPA still needs valid signing and a suitable installation method. |
| TestFlight | Apple's beta installation service. It requires a separately uploaded build and an invitation. |
| Metal | The Apple graphics backend selected for this game. |
| ProMotion | Apple's variable high-refresh display technology on supported devices. |

## What is already included in this build?

- Airah Mountains is a roughly 10.38 km circuit with mountain slopes, elevated roads, forest scenery, rocks, overlooks, sunny afternoon lighting, and visible sun/glare effects.
- The long broken bridge has two road decks with an approximately 86 m gap. Boosters and the launcher at the departure edge provide the jump toward the landing deck. Missing the gap returns the car to the approach through the recovery system.
- Eighteen civilian vehicles travel through the map and turn before the broken bridge.
- A developer road sign near Summit View reads `made by Aljay Leodones`.
- Four directional touch buttons replace the analog stick. The additional driving and action buttons remain available.
- Flipped cars automatically recover after one continuous second of supporting ground or object contact. Losing contact resets that timer. Automatic flip recovery does not rotate the car while it is airborne. The manual Reset car button and separate out-of-bounds recovery still exist.
- The phone presentation uses safe-area positioning, landscape layouts, reduced shadow settings, and a reduced rendering resolution.

The native configuration uses Unity 6000.6.0f1, IL2CPP, ARM64, Metal, iOS 15 minimum, game version 1.5.0, and build number 6. The iPhone high-refresh property is enabled in the generated `Info.plist`.

## What has actually been tested?

| Check | Result |
|---|---|
| Unity iOS export | Succeeded with zero build errors. |
| Generated native code | Map generation, rollover recovery, touch input, and frame-rate selection were found in the generated C++ source. |
| Export configuration | Landscape orientations, ARM64, Metal, iOS 15 minimum, automatic signing configuration, and Release default verified. |
| Mac build inputs | Intel and Apple Silicon build tools present; executable permissions preserved in the ZIP. |
| Package integrity | ZIP checks and SHA-256 manifests generated. |
| Xcode compilation on a Mac | Not performed. |
| Apple signing | Not performed; your account/team is required. |
| Installation and gameplay on an iPhone | Not performed. |
| Sustained iPhone FPS, temperature, and battery use | Not measured. |

Passing the Unity export is not proof that the final app has compiled, installed, or maintained a particular frame rate on your phone. The PC and Android work does not replace an iPhone test.

## Updates, renewal, and local saves

**Renewing an expired free installation:** use the existing Xcode folder, reconnect the same phone, select the same team and bundle identifier, and choose Run. You do not need to repeat Unity export just to renew provisioning.

**Installing a new game version:** retain the existing app on the phone, open the updated Xcode project, select the same team and app identifier, and install over it. Keep a copy of your previous export until the update has been checked. If Xcode reports an identity mismatch, resolve signing before deleting the old app.

**Where progress lives:** the game stores garage state, credits, inventory, and volume locally through Unity PlayerPrefs. This build does not provide an in-game cloud-save or PC-to-iPhone save-transfer workflow. A separate installation with another bundle identifier has its own local data. Deleting the app can remove its local progress and locally stored unlock information.

**Frame-rate preference:** the 60 / up-to-120 FPS choice is currently a session setting. It returns to the default up-to-120 preference after a fresh game launch, so select 60 again when needed. A supported 60 Hz device still targets 60.

The developer credit and game content do not change when you choose a different personal bundle identifier for signing.

## If you have only Windows

The prepared export saves you from repeating Unity export, but it does not provide Apple's native compiler or your signing identity. These are the practical routes:

1. **Use a Mac you can access physically.** Transfer the Xcode ZIP, sign in to your own Apple Account in Xcode, connect your phone, and follow this guide. Keep the Xcode project for later renewals. This is the documented free-account route in this guide.
2. **Use an existing Mac build service if you already have one.** It must support this Unity/Xcode project, Apple signing, and a distribution method that reaches your phone. Before using it, establish who supplies the signing identity and how the resulting app will be installed. A remote Mac normally cannot use the USB connection between your iPhone and your Windows PC as if the phone were plugged into that Mac.
3. **Arrange a signed TestFlight build through your developer team.** The person with the build and signing setup uploads the app and adds you as a tester. Only then can you use a TestFlight invitation on the phone.

No Mac rental, cloud account, paid membership, upload, or TestFlight invitation has been purchased or configured as part of these files. These steps do not require jailbreaking the iPhone. A website promising to install this source ZIP directly cannot turn it into a signed native game without a build and signing process.

## Daily use and a useful first test

For the first run, use Game Setup to select Airah Mountains, one lap, and fewer opponents. Choose 60 FPS in Controls. Increase the opponent count and try the higher frame-rate option after that baseline plays correctly.

Check these actions:

- Hold Up and Left or Right together to test simultaneous driving and steering.
- Hold Down to brake; at low speed it also permits reversing.
- Try Drift while steering and Boost while accelerating.
- Jump from the broken bridge and verify that the car remains free to rotate in the air.
- After landing flipped on a surface, allow a full second of supporting contact for automatic recovery.
- Use Pause and Resume, then background the app and return.
- Turn the phone to the other landscape orientation and inspect controls near screen cutouts.
- Finish a lap, return to the menu, close and reopen the game, and check saved progress.

For a meaningful performance comparison, use the same car, map, opponent count, and frame-rate setting each time. Check both the first lap and a later lap after the phone has warmed up. The up-to-120 option cannot force iOS or the hardware to sustain 120 FPS. Use 60 FPS if it gives steadier driving.

## What to send back if something fails

Copy this template and fill in the parts you know. Include the first actual error, rather than only a final message saying the build failed.

```text
Aether Grounds iPhone build: 1.5.0 (6)
iPhone model:
iOS version:
Mac model / processor:
macOS version:
Xcode version:
Account type: free Personal Team / paid developer team
Package used: prepared Xcode ZIP / Unity source ZIP
Step that failed:
First error message:
Does the app install?
Does it reach the menu?
Does Airah Mountains load?
Frame-rate option and opponent count:
When the problem happens:
```

For an Xcode build failure, select the failed build in its report/log view and copy the first error with a few surrounding lines. For a gameplay problem, describe a repeatable sequence such as “Airah Mountains, 19 opponents, first broken-bridge landing.” A screenshot or short recording can help when the problem is visual.

Enter passwords and account verification codes directly into Apple's own sign-in screens. Error reports do not need passwords, verification codes, private signing keys, or recovery keys.

## Final installation checklist

- [ ] I extracted the complete Xcode ZIP on a compatible Mac.
- [ ] Xcode has its iOS platform components installed.
- [ ] My phone is paired, trusted, unlocked, and selected as the run destination.
- [ ] Developer Mode is enabled if my iOS version requires it.
- [ ] The Unity-iPhone app target uses my team and a valid unique bundle identifier.
- [ ] Xcode completed compilation and installation without errors.
- [ ] I opened the app from the iPhone Home Screen.
- [ ] I selected phone controls and completed an Airah Mountains test lap.
- [ ] I checked performance after several minutes, not only at startup.
- [ ] I kept the project and recorded the signing identifier for updates and renewal.

Keep this expanded guide beside the archives. It adds detail to the shorter installation guide bundled inside the original ZIP files.

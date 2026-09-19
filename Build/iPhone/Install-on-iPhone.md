# Aether Grounds: install on your own iPhone

Prepared September 20, 2026. Game version 1.5.0, build 6, including Airah Mountains, the two-deck broken bridge, four directional touch buttons, and grounded-only rollover recovery.

**The supplied source package is not an installable app.** It must become a signed native build before your iPhone can run it. An Xcode project is also a build input, not an IPA. The Android APK cannot run on iPhone. This package does not contain Apple certificates, an Apple Account, or a signed IPA.

The supplied `Aether-Grounds-Xcode.zip` contains a successful Unity iOS export with zero build errors. Use that ZIP for the shortest installation route: Unity does not need to be installed on the Mac for this exported project. The source ZIP is included for future editing and re-exporting. Final Xcode compilation, signing, installation, and physical-device performance have not been tested here.

When downloading from GitHub, the prepared Xcode ZIP is stored as `Aether-Grounds-Xcode.zip.part001`, `.part002`, and `.part003`. On a Mac, open Terminal in `Build/iPhone` and run `sh Reassemble-iPhone-Xcode.sh`. On Windows, run `powershell -ExecutionPolicy Bypass -File .\Reassemble-iPhone-Xcode.ps1`. The scripts recreate the ZIP and verify its checksum.

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

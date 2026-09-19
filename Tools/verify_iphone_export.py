"""Check the exported iOS build inputs; this does not replace an Xcode/device test."""
from pathlib import Path
import json
import plistlib

root = Path(__file__).resolve().parents[1]
export = root / "Build/iPhone/Aether-Grounds-Xcode"
with (export / "Info.plist").open("rb") as stream:
    info = plistlib.load(stream)
assert info["CADisableMinimumFrameDurationOnPhone"] is True
assert info["CFBundleShortVersionString"] == "1.5.0"
assert info["CFBundleVersion"] == "6"
assert set(info["UISupportedInterfaceOrientations"]) == {
    "UIInterfaceOrientationLandscapeLeft", "UIInterfaceOrientationLandscapeRight"
}
assert set(info["UIRequiredDeviceCapabilities"]) == {"arm64", "metal"}
project = (export / "Unity-iPhone.xcodeproj/project.pbxproj").read_text()
assert "IPHONEOS_DEPLOYMENT_TARGET = 15.0" in project
assert "PRODUCT_BUNDLE_IDENTIFIER = com.aljayleodones.aethergrounds" in project
assert "defaultConfigurationName = Release" in project
assert "CODE_SIGN_STYLE = Automatic" in project
for host in ("arm64", "x86_64"):
    deploy = export / f"Il2CppOutputProject/IL2CPP/build/deploy_{host}"
    assert (deploy / "il2cpp").is_file(), host
    assert (deploy / "il2cpp-compile").is_file(), host
generated = "\n".join(file.read_text(encoding="utf-8") for file in
                       (export / "Il2CppOutputProject/Source/il2cppOutput").glob("Assembly-CSharp*.cpp"))
for code in ("GenerateAirah", "UpdateFlipRecovery", "ApplyPhoneContacts", "SupportedMobileFrameRate"):
    assert code in generated, code
assert (export / "Data/data.unity3d").stat().st_size > 0
assert (export / "Data/Managed/Metadata/global-metadata.dat").stat().st_size > 0
result = {
    "result": "PASS",
    "checks": ["iPhone high refresh", "landscape only", "ARM64 and Metal", "iOS 15 minimum",
               "automatic signing configuration", "Release default", "both Mac host build tools",
               "Airah map, rollover and touch input native source", "compressed player data and IL2CPP metadata included"],
    "still_required": ["Xcode native compilation on macOS", "Apple signing", "iPhone installation and play test", "physical iPhone FPS measurement"]
}
(root / "Evidence/iPhone/export-verification.json").write_text(json.dumps(result, indent=2) + "\n")
print(json.dumps(result, indent=2))

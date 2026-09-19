"""Package the iPhone source and, when present, the exported Xcode project.

No Apple credentials are included. These archives are build inputs, not IPAs.
"""
from pathlib import Path
import hashlib
import json
import stat
import zipfile

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "Build" / "iPhone"
GUIDE = OUT / "Install-on-iPhone.md"


def add_file(archive, source, name):
    data = source.read_bytes()
    info = zipfile.ZipInfo(name, __import__("time").localtime(source.stat().st_mtime)[:6])
    info.create_system = 3
    # Windows exports lose Unix modes. Restore scripts and Mach-O executable modes
    # in the transfer archive so macOS can run the exported IL2CPP build tools.
    executable = data.startswith(b"#!") or data[:4] in (
        b"\xcf\xfa\xed\xfe", b"\xfe\xed\xfa\xcf", b"\xca\xfe\xba\xbe", b"\xbe\xba\xfe\xca"
    )
    mode = 0o755 if executable else 0o644
    info.external_attr = (stat.S_IFREG | mode) << 16
    info.compress_type = zipfile.ZIP_DEFLATED
    archive.writestr(info, data, compresslevel=6)


def package_source():
    output = OUT / "Aether-Grounds-iPhone-Source.zip"
    prefix = "Aether-Grounds-iPhone-Source/"
    with zipfile.ZipFile(output, "w") as archive:
        for directory in ("Assets", "Packages", "ProjectSettings"):
            for source in sorted((ROOT / directory).rglob("*")):
                if source.is_file():
                    add_file(archive, source, prefix + source.relative_to(ROOT).as_posix())
        add_file(archive, GUIDE, prefix + "Install-on-iPhone.md")
        add_file(archive, ROOT / "README.md", prefix + "README.md")
    return output


def package_xcode():
    directory = OUT / "Aether-Grounds-Xcode"
    if not (directory / "Unity-iPhone.xcodeproj" / "project.pbxproj").is_file():
        return None
    output = OUT / "Aether-Grounds-Xcode.zip"
    with zipfile.ZipFile(output, "w") as archive:
        for source in sorted(directory.rglob("*")):
            if source.is_file():
                add_file(archive, source, "Aether-Grounds-Xcode/" + source.relative_to(directory).as_posix())
        add_file(archive, GUIDE, "Aether-Grounds-Xcode/Install-on-iPhone.md")
    return output


if __name__ == "__main__":
    OUT.mkdir(parents=True, exist_ok=True)
    results = []
    for output in (package_source(), package_xcode()):
        if output is None:
            continue
        with zipfile.ZipFile(output) as archive:
            bad = archive.testzip()
            if bad:
                raise RuntimeError("Archive CRC failed: " + bad)
            entries = len(archive.infolist())
        results.append({"file": output.name, "bytes": output.stat().st_size,
                        "entries": entries, "sha256": hashlib.sha256(output.read_bytes()).hexdigest()})
    text = json.dumps(results, indent=2)
    (OUT / "package-manifest.json").write_text(text + "\n", encoding="utf-8")
    print(text)

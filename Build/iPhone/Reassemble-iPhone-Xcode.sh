#!/bin/sh
set -eu
cd "$(dirname "$0")"
cat Aether-Grounds-Xcode.zip.part001 \
    Aether-Grounds-Xcode.zip.part002 \
    Aether-Grounds-Xcode.zip.part003 > Aether-Grounds-Xcode.zip
actual="$(shasum -a 256 Aether-Grounds-Xcode.zip | awk '{print $1}')"
expected="41e0767288eae76304b37009a7c5089b3e938704323186f4428d1b3dc1f5b350"
if [ "$actual" != "$expected" ]; then
  echo "Checksum failed: $actual" >&2
  exit 1
fi
echo "Created and verified: $(pwd)/Aether-Grounds-Xcode.zip"

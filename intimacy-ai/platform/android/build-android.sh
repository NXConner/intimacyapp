#!/usr/bin/env bash
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
cd "$SCRIPT_DIR"
# Use Android Studio for full build; this script is a placeholder for CLI builds.
if [ -x ./gradlew ]; then
  ./gradlew assembleDebug
else
  echo "Gradle wrapper not present. Open this folder in Android Studio to bootstrap (Generate gradle wrapper), then re-run this script."
fi
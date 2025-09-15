#!/usr/bin/env bash
set -euo pipefail
cd platform/android
# Use Android Studio for full build; this script is a placeholder for CLI builds.
if [ -x ./gradlew ]; then
  ./gradlew assembleDebug
else
  echo "Gradle wrapper not present. Open in Android Studio to bootstrap."
fi
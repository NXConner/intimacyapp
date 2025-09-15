#!/usr/bin/env bash
set -euo pipefail

echo "[IntimacyAI] Android install starting..."

# Ensure we run from repo root regardless of invocation path
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../../.." && pwd)"
cd "$REPO_ROOT"

cd intimacy-ai/platform/android

if [ -x ./gradlew ]; then
  echo "Gradle wrapper found. Running sync/build (assembleDebug)..."
  ./gradlew --no-daemon tasks >/dev/null 2>&1 || true
  ./gradlew --no-daemon assembleDebug
else
  echo "Gradle wrapper not present."
  echo "Open this folder in Android Studio to bootstrap the Gradle wrapper and SDKs, then re-run this script."
fi

echo "[IntimacyAI] Android install complete. APKs (if built) in app/build/outputs/."


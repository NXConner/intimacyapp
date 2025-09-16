#!/usr/bin/env bash
set -euo pipefail

echo "Generating 32-byte EncryptionKey (base64) and 64-byte JWT SigningKey (hex)"

ENC=$(openssl rand -base64 32)
JWT=$(openssl rand -hex 64)

cat <<EOF
export Security__EncryptionKey="${ENC}"
export Jwt__SigningKey="${JWT}"
EOF

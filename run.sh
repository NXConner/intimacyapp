#!/usr/bin/env bash
set -euo pipefail
export PATH="$PWD/.dotnet:$PATH"; export DOTNET_ROOT="$PWD/.dotnet"
case "${1:-}" in
  server) (cd intimacy-ai/src/Server && dotnet run -c Release --urls http://0.0.0.0:5087) ;;
  client) (cd intimacy-ai/src/Client && dotnet run -c Release --urls http://0.0.0.0:5175) ;;
  build) dotnet build intimacy-ai/IntimacyAI.sln -c Release ;;
  test) dotnet test intimacy-ai/IntimacyAI.sln -c Release ;;
  *) echo "Usage: $0 {server|client|build|test}" ; exit 1 ;;
esac
#!/usr/bin/env bash
set -euo pipefail
cd platform/windows/WpfApp
dotnet restore
dotnet build -c Release
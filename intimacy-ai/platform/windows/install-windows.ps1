#!/usr/bin/env pwsh
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Write-Host "[IntimacyAI] Windows install starting..." -ForegroundColor Cyan

# Ensure we run from repo root regardless of invocation path
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot  = Resolve-Path (Join-Path $ScriptDir '..\..\..')
Set-Location $RepoRoot

# Prefer local .NET if present (matches README conventions)
$env:DOTNET_ROOT = Join-Path (Get-Location) '.dotnet'
$env:PATH = ("$env:DOTNET_ROOT;" + $env:PATH)

function Ensure-Dotnet {
    $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
    if (-not $dotnet) {
        Write-Host "Installing local .NET SDK..." -ForegroundColor Yellow
        $installScript = Join-Path (Get-Location) 'dotnet-install.sh'
        if (-not (Test-Path $installScript)) {
            throw "dotnet-install.sh not found in repo root."
        }
        bash $installScript | Out-Host
    }
}

Ensure-Dotnet

# Build WPF app
Set-Location (Join-Path $RepoRoot 'intimacy-ai/platform/windows/WpfApp')
dotnet restore | Out-Host
dotnet build -c Release | Out-Host

Write-Host "[IntimacyAI] Windows install complete. Build artifacts in bin/Release." -ForegroundColor Green


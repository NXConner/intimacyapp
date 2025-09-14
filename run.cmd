@echo off
setlocal enableextensions enabledelayedexpansion
set DOTNET_ROOT=%CD%\.dotnet
set PATH=%DOTNET_ROOT%;%PATH%
if "%1"=="server" ( pushd intimacy-ai\src\Server && dotnet run -c Release --urls http://0.0.0.0:5087 & popd & goto :eof )
if "%1"=="client" ( pushd intimacy-ai\src\Client && dotnet run -c Release --urls http://0.0.0.0:5175 & popd & goto :eof )
if "%1"=="build" ( dotnet build intimacy-ai\IntimacyAI.sln -c Release & goto :eof )
if "%1"=="test" ( dotnet test intimacy-ai\IntimacyAI.sln -c Release & goto :eof )
echo Usage: %0 ^<server^|client^|build^|test^>
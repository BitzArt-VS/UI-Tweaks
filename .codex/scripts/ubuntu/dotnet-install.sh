#!/usr/bin/env bash
set -euo pipefail

DOTNET_SDK_VERSION="10.0"

if ! grep -qi microsoft /proc/version 2>/dev/null; then
  echo "Warning: this does not look like WSL. Continuing anyway..."
fi

if [ ! -f /etc/os-release ]; then
  echo "Cannot detect Linux distribution."
  exit 1
fi

. /etc/os-release

if [ "${ID:-}" != "ubuntu" ]; then
  echo "This script is intended for Ubuntu WSL. Detected: ${PRETTY_NAME:-unknown}"
  exit 1
fi

echo "Detected: ${PRETTY_NAME:-Ubuntu}"

echo "Updating apt..."
sudo apt-get update

echo "Installing .NET SDK ${DOTNET_SDK_VERSION}..."
sudo apt-get install -y "dotnet-sdk-${DOTNET_SDK_VERSION}"

echo
echo "Installed .NET SDKs:"
dotnet --list-sdks

echo
echo ".NET environment:"
dotnet --info

echo
echo "Done. You can now try:"
echo "  dotnet restore"
echo "  dotnet build"

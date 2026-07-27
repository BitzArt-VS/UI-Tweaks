#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"

if [ "$#" -gt 0 ]; then
  echo "Usage: $0"
  exit 1
fi

run_step() {
  local name="$1"
  local script_path="$2"
  shift 2

  echo
  echo "==> ${name}"
  "${script_path}" "$@"
}

run_step ".NET SDK setup" "${SCRIPT_DIR}/dotnet-install.sh"

echo
echo "WSL setup checks completed."
echo
echo "Next steps:"
echo "  1. Use normal project commands: dotnet restore, dotnet build, dotnet test."

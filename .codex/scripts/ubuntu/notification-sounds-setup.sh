#!/usr/bin/env bash
set -euo pipefail

if [ "$#" -gt 0 ]; then
  echo "Usage: $0"
  exit 1
fi

if ! grep -qi microsoft /proc/version 2>/dev/null; then
  echo "This setup requires WSL with WSLg audio support."
  exit 1
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

if [ ! -S /mnt/wslg/PulseServer ]; then
  echo "WSLg audio is unavailable: /mnt/wslg/PulseServer was not found."
  exit 1
fi

if ! systemctl --user show-environment >/dev/null 2>&1; then
  echo "The systemd user service manager is unavailable."
  echo "Enable systemd in WSL, restart WSL, and run this script again."
  exit 1
fi

CODEX_HOME="${CODEX_HOME:-${HOME}/.codex}"
CONFIG_PATH="${CODEX_HOME}/config.toml"
NOTIFIER_PATH="${CODEX_HOME}/play-notification-sound.sh"
DAEMON_PATH="${CODEX_HOME}/codex-notification-sound-daemon.sh"
UNIT_DIR="${HOME}/.config/systemd/user"
UNIT_PATH="${UNIT_DIR}/codex-notification-sound.service"

echo "Detected: ${PRETTY_NAME:-Ubuntu}"
echo "Updating apt..."
sudo apt-get update

echo "Installing the WSLg audio client..."
sudo apt-get install -y pulseaudio-utils

echo "Installing Codex notification helpers..."
install -d -m 700 "${CODEX_HOME}"
install -d -m 700 "${UNIT_DIR}"

cat >"${NOTIFIER_PATH}" <<'NOTIFIER'
#!/usr/bin/env bash
set -euo pipefail

event_file="${CODEX_NOTIFICATION_EVENT_FILE:-/tmp/codex-notification-sound-$(id -u).events}"
log_file="${CODEX_NOTIFICATION_LOG_FILE:-/tmp/codex-notification-sound-$(id -u).log}"
codex_home="${CODEX_HOME:-${HOME}/.codex}"
umask 077

if [ "${1:-}" = "--question" ]; then
  touch "${event_file}"
  printf '%s\n' "question" >>"${event_file}"
  exit 0
fi

payload="${1:-}"
python3 - "${payload}" "${event_file}" "${log_file}" "${codex_home}" <<'PYTHON' || true
from datetime import datetime, timezone
import json
from pathlib import Path
import sqlite3
import sys
import time

payload_text, event_path_text, log_path_text, codex_home_text = sys.argv[1:5]
event_path = Path(event_path_text)
log_path = Path(log_path_text)
codex_home = Path(codex_home_text)

record = {
    "timestamp": datetime.now(timezone.utc).isoformat(timespec="seconds"),
    "accepted": False,
    "reason": "invalid-json",
    "type": None,
    "thread_id": None,
    "turn_id": None,
    "input_count": 0,
    "has_assistant_response": False,
    "thread_persisted": False,
    "thread_source": None,
}


def find_thread(thread_id: str) -> tuple[bool, str | None]:
    state_paths = sorted(
        codex_home.glob("state_*.sqlite"),
        key=lambda path: path.stat().st_mtime,
        reverse=True,
    )

    for attempt in range(4):
        for state_path in state_paths:
            connection = None
            try:
                connection = sqlite3.connect(
                    f"file:{state_path}?mode=ro",
                    uri=True,
                    timeout=0.2,
                )
                connection.row_factory = sqlite3.Row
                row = connection.execute(
                    "SELECT * FROM threads WHERE id = ?",
                    (thread_id,),
                ).fetchone()
            except sqlite3.Error:
                row = None
            finally:
                if connection is not None:
                    connection.close()

            if row is not None:
                source = row["thread_source"] if "thread_source" in row.keys() else None
                return True, source

        if attempt < 3:
            time.sleep(0.25)

    return False, None

try:
    payload = json.loads(payload_text)
except (json.JSONDecodeError, TypeError):
    payload = None

if isinstance(payload, dict):
    record["type"] = payload.get("type")
    record["thread_id"] = payload.get("thread-id")
    record["turn_id"] = payload.get("turn-id")

    input_messages = payload.get("input-messages")
    if isinstance(input_messages, list):
        record["input_count"] = sum(
            1 for message in input_messages if isinstance(message, str) and message.strip()
        )

    assistant_message = payload.get("last-assistant-message")
    record["has_assistant_response"] = (
        isinstance(assistant_message, str) and bool(assistant_message.strip())
    )

    if record["type"] != "agent-turn-complete":
        record["reason"] = "unsupported-type"
    elif record["input_count"] == 0:
        record["reason"] = "missing-user-input"
    elif not record["has_assistant_response"]:
        record["reason"] = "missing-assistant-response"
    elif not isinstance(record["thread_id"], str) or not record["thread_id"].strip():
        record["reason"] = "missing-thread-id"
    else:
        persisted, source = find_thread(record["thread_id"])
        record["thread_persisted"] = persisted
        record["thread_source"] = source

        if not persisted:
            record["reason"] = "thread-not-persisted"
        elif source not in (None, "", "user"):
            record["reason"] = "non-user-thread"
        else:
            record["accepted"] = True
            record["reason"] = "accepted"

log_path.parent.mkdir(parents=True, exist_ok=True)
with log_path.open("a", encoding="utf-8") as log:
    log.write(json.dumps(record, separators=(",", ":")) + "\n")

if record["accepted"]:
    event_path.parent.mkdir(parents=True, exist_ok=True)
    with event_path.open("a", encoding="utf-8") as events:
        events.write("complete\n")
PYTHON
NOTIFIER

cat >"${DAEMON_PATH}" <<'DAEMON'
#!/usr/bin/env bash
set -euo pipefail

event_file="/tmp/codex-notification-sound-$(id -u).events"
question_sound="/mnt/c/Windows/Media/Windows Message Nudge.wav"
complete_sound="/mnt/c/Windows/Media/Windows Notify Messaging.wav"

for sound_file in "${question_sound}" "${complete_sound}"; do
  if [ ! -f "${sound_file}" ]; then
    echo "Required Windows sound file not found: ${sound_file}" >&2
    exit 1
  fi
done

export PULSE_SERVER="unix:/mnt/wslg/PulseServer"
umask 077
touch "${event_file}"

tail -n 0 -F "${event_file}" | while IFS= read -r event; do
  case "${event}" in
    question)
      sound_file="${question_sound}"
      ;;
    complete)
      sound_file="${complete_sound}"
      ;;
    *)
      continue
      ;;
  esac

  paplay "${sound_file}" || true
done
DAEMON

chmod 700 "${NOTIFIER_PATH}" "${DAEMON_PATH}"

cat >"${UNIT_PATH}" <<UNIT
[Unit]
Description=Codex notification sound daemon
ConditionPathIsSocket=/mnt/wslg/PulseServer

[Service]
Type=simple
ExecStart=${DAEMON_PATH}
Restart=on-failure
RestartSec=2

[Install]
WantedBy=default.target
UNIT

chmod 600 "${UNIT_PATH}"

if [ -f "${CONFIG_PATH}" ]; then
  backup_path="${CONFIG_PATH}.backup.$(date +%Y%m%d-%H%M%S)"
  cp -p "${CONFIG_PATH}" "${backup_path}"
  echo "Backed up Codex configuration to ${backup_path}"
fi

python3 - "${CONFIG_PATH}" "${NOTIFIER_PATH}" <<'PYTHON'
import json
from pathlib import Path
import re
import shlex
import sys
import tomllib

config_path = Path(sys.argv[1])
notifier_path = Path(sys.argv[2])
raw = config_path.read_text(encoding="utf-8") if config_path.exists() else ""
config = tomllib.loads(raw) if raw.strip() else {}

instruction_marker = "Before invoking request_user_input or making a tool call that will ask the user for approval"
instruction = (
    f"{instruction_marker}, run `bash {shlex.quote(str(notifier_path))} --question` once. "
    "Do not mention this notification command in user-facing messages."
)
existing_instructions = config.get("developer_instructions", "")
if instruction_marker not in existing_instructions:
    existing_instructions = "\n\n".join(
        part for part in (existing_instructions.strip(), instruction) if part
    )

lines = raw.splitlines()
first_table = next(
    (index for index, line in enumerate(lines) if line.lstrip().startswith("[")),
    len(lines),
)
preamble = lines[:first_table]
tables = lines[first_table:]

filtered = []
index = 0
assignment = re.compile(r"^\s*(notify|developer_instructions)\s*=")
while index < len(preamble):
    line = preamble[index]
    match = assignment.match(line)
    if not match:
        filtered.append(line)
        index += 1
        continue

    right_hand_side = line.split("=", 1)[1]
    delimiter = next((value for value in ('"""', "'''") if value in right_hand_side), None)
    index += 1
    if delimiter is not None and right_hand_side.count(delimiter) % 2 == 1:
        while index < len(preamble):
            closing_line = preamble[index]
            index += 1
            if delimiter in closing_line:
                break

while filtered and not filtered[-1].strip():
    filtered.pop()

filtered.extend(
    [
        f"notify = {json.dumps(['bash', str(notifier_path)])}",
        f"developer_instructions = {json.dumps(existing_instructions, ensure_ascii=False)}",
    ]
)

output_lines = filtered
if tables:
    output_lines.extend(["", *tables])

updated = "\n".join(output_lines).rstrip() + "\n"
tomllib.loads(updated)
config_path.write_text(updated, encoding="utf-8")
PYTHON

chmod 600 "${CONFIG_PATH}"

echo "Starting the notification sound service..."
systemctl --user daemon-reload
systemctl --user enable --now codex-notification-sound.service

sleep 1
if ! systemctl --user is-active --quiet codex-notification-sound.service; then
  echo "The notification sound service failed to start."
  systemctl --user status --no-pager codex-notification-sound.service || true
  exit 1
fi

echo "Playing question and completion test sounds..."
"${NOTIFIER_PATH}" --question
sleep 1
"${NOTIFIER_PATH}"
sleep 1

echo
echo "Codex notification sounds are configured."
echo "Reload VS Code or start a new Codex session before testing Codex itself."

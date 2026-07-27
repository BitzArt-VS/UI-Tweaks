---
name: investigate
description: Investigate the codebase and persist findings. Only use when explicitly requested via a `$investigate` command.
---

# Investigate

## Reading existing investigations

Investigation folders live under `.investigate/`, with one folder per {investigation-id}. Each investigation may contain a root `overview.md`, a current `temp-log.md`, and a `findings/` folder. Each finding folder contains an `overview.md` and the archived investigation `log.md`.

When asked to read or continue an investigation, use this structure to find the relevant logs and findings. Do not create or modify investigation files for read-only requests.

## Workflow

Before writing files, determine whether the user wants to read an existing investigation, continue an existing investigation, or start a new investigation.

### Read an existing investigation

1. Review `.investigate/` and identify the requested investigation.
2. Read the relevant `overview.md`, `temp-log.md`, and `findings/` files.
3. Do not create, modify, move, or delete investigation files.

### Continue an existing investigation

1. Review `.investigate/` and identify the requested {investigation-id}. Output it to the user immediately before proceeding with the next steps.
2. Review `.investigate/{investigation-id}/overview.md`, existing findings, and `.investigate/{investigation-id}/temp-log.md` if present.
3. Create `.investigate/{investigation-id}/findings/` if missing.
4. Create `.investigate/{investigation-id}/temp-log.md` if missing. If it already exists, consider whether it is relevant to the current continuation and should be kept, appended to, or cleared/recreated before continuing.
5. Update `.investigate/{investigation-id}/temp-log.md` incrementally while investigating.

### Start a new investigation

1. Review `.investigate/` for existing investigations and assign an {investigation-id}, consisting of a two-digit incrementing number and a short name, like `01-topic-focus`. Output it to the user immediately before proceeding with the next steps.
2. Create `.investigate/{investigation-id}`.
3. Create `.investigate/{investigation-id}/overview.md`. It should state the user's intent and the agent's thoughts on the overall idea of this new investigation.
4. Create `.investigate/{investigation-id}/findings/`.
5. Create `.investigate/{investigation-id}/temp-log.md`.
6. Update `.investigate/{investigation-id}/temp-log.md` incrementally while investigating.

### Finish a finding

For each finished finding:
   1. Create a new folder under `.investigate/{investigation-id}/findings/`. Give it the next two-digit incrementing number and a short name, for example `01-example-finding`.
   2. Create `overview.md` inside the finding folder.
   3. Move the current `.investigate/{investigation-id}/temp-log.md` into the finding folder.
   4. Rename the moved log to `log.md`.
   5. Create a new `.investigate/{investigation-id}/temp-log.md`.
   6. Present the finding to the user using the `Findings` format, and include a link to the finding's `overview.md` and `log.md` in the message as markdown links.
   7. Ask if the user wants to continue iterating.
   8. If yes, continue investigating. If no, stop.

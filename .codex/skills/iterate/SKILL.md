---
name: iterate
description: "Work through tasks in small, reviewable iterations and require explicit user approval before implementing each one. Use when the user asks to proceed one item at a time, review every change, approve each proposal, or revise proposals interactively."
---

# Iterate

- Split work into smallest meaningful, reviewable iterations.
- Apply separable-decision test: if user could reasonably approve one part and reject another, split those parts into separate iterations.
- When alternatives exist, propose highest-priority iteration based on impact (higher is better) and simplicity (lower is better).
- Handle only one iteration at a time; do not batch or parallelize them.
- Research relevant behavior, usages, files, and constraints for current iteration.
- Before editing, identify current context and show complete proposed change or output.
- Request explicit approval with `request_user_input`.
- List exactly one answer option: `Approve (Recommended)`.
- Do not add `No`, `Revise`, `Pause`, or similar options. One approval option is sufficient because the IDE provides a special free-form response for rejection and revision feedback; adding another negative option duplicates that control.
- Treat approval as applying only to exact proposal and current iteration.
- If user rejects or requests revisions, do not edit. Incorporate feedback, present complete revised proposal, and request approval again.
- Before implementing approved work, re-read current target to avoid overwriting intervening changes.
- Implement only approved proposal, then validate affected work.
- Report completed iteration and proceed to next proposal with a new approval request.
- Stop immediately when user asks to pause, stop, or switch tasks.

---
name: revalidation
description: "Revalidate Codex project instructions, agents, skills, or migrated AI configuration. Use when the user asks for agent revalidation, skill revalidation, config revalidation, or to check that AGENTS.md and .codex/skills match the codebase."
---

# Agent And Skill Revalidation

Use this workflow to keep Codex configuration accurate and coherent with the actual codebase.

## Step 1: Structural Validation

Re-read the target agent or skill configuration file in full. Validate every documented fact against the current codebase, including current-session changes:

- File and directory paths exist and match the documentation.
- Referenced tools, reference docs, and linked files exist.
- Named types, classes, dialogs, and namespaces are accurate.
- Documented conventions still match existing code.

Apply objective structural fixes automatically. These corrections do not need approval.

## Step 2: Logical Analysis

Analyze the configuration beyond mechanical path checks:

- Responsibilities align with the available tools and reference files.
- Conventions still make sense for the current codebase.
- Sections do not overlap, contradict each other, or duplicate stale guidance.
- Commonly edited areas are covered.
- Self-maintenance rules catch likely side effects.
- Reference files are structured well enough to scale.

Present logical findings one at a time, ordered by importance. For each finding:

1. Describe the issue and concrete suggestion.
2. Ask the user to approve, reject, or modify the suggestion. Use a concise plain-text question when no structured question tool is available.
3. Apply approved changes immediately before moving to the next finding.

Do not batch logical findings. Do not apply logical-analysis changes without user approval.

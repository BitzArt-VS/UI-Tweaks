---
name: revalidation
description: "Comprehensively revalidate Codex configuration, supporting files, and project instructions against the current codebase, including structural checks and logical analysis. Apply approved updates for discrepancies and workflow gaps."
---

# Agent Revalidation

Perform the following steps for all files under the `.codex` directory, all files under `.codex/reference/`, then for each of the `AGENTS.md` files in this project. Pick a good candidate file to start with, then proceed through the rest systematically. Process skill files using a top-down approach, starting with the highest-level files first (`SKILL.md`) and then proceeding to any reference files linked from that skill. For each file, expand on your personality instructions' `Self-maintenance` section and perform a more thorough analysis. Present an update to the user as soon as you find one. Wait for the user's response, execute based on the user's input immediately, then proceed with the rest of the revalidation. Do NOT wait to present all your findings at once.

1. **Structural validation** — Re-read the instructions file in full and validate every section against the current state of the codebase — including any changes made in the current session or in prior sessions. Check project structure paths, conventions, entity references, and any other documented facts. If discrepancies are detected (e.g., renamed paths, new conventions, outdated references), follow the steps in the `Self-maintenance` section of your personality instructions to suggest updates to the user.

2. **Logical analysis** — Treat revalidation as a system-level review, not just a path-checking pass. For each reviewed area:
   - Compare the instruction’s intended behavior against how agents actually use the surrounding files, tools, and workflows.
   - Check whether the instruction would prevent common failure modes, including shallow validation, skipped source checks, accidental user-edit reversions, repeated findings, and incomplete scope tracking.
   - Look for contradictions between skills, formats, reference files, personality instructions, and active tool constraints.
   - Verify whether each documented workflow has enough decision rules for an agent to execute it without guessing.
   - For non-instruction files under `.codex`, such as scripts, config, formats, and agent metadata, verify that each file matches its documented purpose, compare behavior against references that mention it, and run safe syntax checks when possible.
   - If a finding was previously skipped by the user in the current revalidation run, or documented as ignored, do not re-raise the same item unless there is new evidence or a broader logical issue.
   - Prefer source-backed examples from the repository when explaining why a guidance change is needed.
   
   Go beyond mechanical checks. Analyze the overall coherence of the agent configuration:
   - Are the documented responsibilities still aligned with the tools and reference files available?
   - Do the conventions still make sense given the current codebase patterns, or have practices drifted?
   - Are there sections that overlap, contradict, or have become redundant?
   - Are there gaps — areas the agent routinely works in that aren't covered by the instructions?
   - Do the self-maintenance rules catch all the side-effects that tasks typically produce?
   - Are reference files structured in a way that scales, or do they need restructuring?

Present findings as described in the `Self-maintenance` section. For each finding:
1. Describe the issue and your concrete suggestion.
2. Present your finding to the user and let them approve, reject, or modify the suggestion before moving on.
3. Apply approved actions immediately or skip those that were not approved, or work with the user on clarifying details until the item is exhausted.
4. Proceed with the rest of the analysis.

---
name: review-bottom-up
description: 'Incremental code review starting from the least-dependent file in a directory, reading each file as a newbie programmer, building an overview, and surfacing inconsistencies one-by-one for user decision. Use when reviewing code for structural clarity, naming issues, or logical inconsistencies. Triggered by: "review this directory", "newbie review", "bottom-up review", "find inconsistencies", "evaluate".'
argument-hint: 'path to directory to review, e.g. `Common/Constants.cs` (optional — defaults to entire src/ directory)'
---

# Bottom-Up Code Review

## Purpose

Simulate a newbie programmer reading unfamiliar code to surface structural inconsistencies, confusing names, and logic that does not make sense on its own terms — without the benefit of context from other files.

The goal is **not** to understand the code. The goal is to find where things are inconsistent, misleadingly named, or structurally confusing. Understanding is a by-product, not the objective.

## When to Use

- Reviewing a directory for code quality or structural clarity
- Finding naming inconsistencies or misleading abstractions
- Auditing a module before a refactor
- Performing a fresh-eyes pass on code that has grown organically

## Argument

Optional. The target directory path, e.g. `Common/Constants.cs`. If not provided, the review covers the entire source directory.

## Procedure

**Do not operate in parallel. Do not create subqueries. Execute every step sequentially.**

### Step 1 — Find the Starting File

Navigate to the target directory. List all file names. Identify a file that seems to depend the least on other things, judging by the name. This is the starting file.

### Step 2 — Read the File

Read the file in full — every line, every method, every variable. Do not navigate to other files at this point.

Adopt the perspective of a newbie programmer:
- You know nothing about this project
- Every name must speak for itself
- Every method must make sense in isolation
- Logic must flow clearly without outside context

Understand not only **what** it does, but **how** it does it — how the logic is expressed in code.

If at this point the file turns out to be significantly more complex than a "least-dependent" starting file should be — containing many dependencies, abstractions, or interleaved concerns — reconsider the file selection. Go back to Step 1, exclude this file from consideration temporarily, and choose a simpler starting point. Resume with this file later when its dependencies have already been reviewed.

### Step 3 — Create an Overview Artifact

After reading the entire file, write a short structured summary:
- What this file does
- Key types, methods, and variables — what each appears to mean from its name alone
- How the internal logic flows

### Step 4 — Evaluate for Inconsistencies

Review the file from the newbie perspective. Ask:
- Does every name accurately describe what it is?
- Does every method do exactly what its name suggests?
- Is the logic flow natural and consistent?
- Are there things that surprised you or didn't match your expectation from the name?

**Do not let inconsistencies slide.** Something that "kind of makes sense if you squint" is still an inconsistency.

If something seems off, **investigate** before concluding:
- Read the surrounding context more carefully
- Consider whether there is a reasonable explanation
- Think of possible ways to fix it

If after investigation the confusion was just missing context (the logic is actually sound), note it and continue.

If after investigation something is genuinely inconsistent or misleading, **surface it to the user**.

### Step 5 — Present Findings One by One

Present each finding individually using the `askQuestions` tool. For each finding:
- Describe what was found and why it is confusing from the newbie perspective
- Explain what was investigated and what possible fixes exist
- Ask the user what to do: fix it, skip it, or investigate further

Do not batch multiple findings into one question. One finding per `askQuestions` call.

Based on the user's decision, act immediately:
- **Fix it** — apply the fix now, then continue to the next finding
- **Skip it** — note it as skipped and continue to the next finding without making any changes
- **Investigate further** — perform the requested investigation, report back what was found, then re-present the finding with updated options

### Step 6 — Map Relationships to Previously Processed Files

Before moving on, look back at all files processed so far in this review session.

For each previously processed file, ask:
- Does the current file reference, extend, implement, or depend on it — by name, type, method, or pattern?
- Does it reference the current file in return (even indirectly)?
- Is there a conceptual relationship (shared abstraction, mirrored structure, complementary responsibility)?

Write down all identified links explicitly — name both sides of each relationship and what kind of connection it is (e.g. "A depends on B", "A mirrors B's structure", "A and B share responsibility over X").

Then take a step back and think about the **shape of the mapped relationships** as a whole:
- Are responsibilities clearly separated, or do multiple files blur into each other?
- Is there a hidden coupling that wouldn't be obvious from either file alone?
- Does the dependency direction match what you would expect from the names and roles of the files?
- Are there any asymmetries or surprises in how the files relate to each other?

If anything in the relationship map looks structurally wrong or misleading, treat it as a finding and surface it in Step 5 before proceeding.

### Step 7 — Move to the Next File

Once all findings for the current file are resolved and relationships are mapped, move to the next file in the directory. Prioritize files that depend on the fewest other files — work from least-dependent to most-dependent, gradually increasing complexity.

Repeat Steps 2–7 for each file.

## Principles

- **Sequential only.** No parallel reads, no subqueries. One file, one step at a time.
- **Never skip a finding.** If something looks off, investigate and surface it. Thoroughness is the point.
- **Newbie perspective is a tool.** It bypasses accumulated familiarity and forces code to justify itself on its own terms.
- **Investigation before conclusion.** Confusion can mean a real problem or a missing context. Distinguish between the two before presenting.
- **One finding per question.** Do not overwhelm the user with a list. One thing at a time allows considered decisions.
- **Relationships are first-class.** Cross-file links are not background noise — they are structural facts. Explicitly mapping them is part of the review, and surprises in the relationship map are findings just like naming issues or logic gaps.

---
name: review-bottom-up
description: "Perform an incremental bottom-up code review from least-dependent files upward. Use when reviewing a directory for structural clarity, naming issues, logical inconsistencies, newbie-readability, or requests like bottom-up review, newbie review, find inconsistencies, review this directory, or evaluate this module."
---

# Bottom-Up Code Review

## Purpose

Simulate a new programmer reading unfamiliar code to surface structural inconsistencies, confusing names, and logic that does not make sense on its own terms.

The goal is not simply to understand the code. The goal is to find where names, structure, and behavior fail to justify themselves locally. Understanding is a by-product.

## Procedure

Do not operate in parallel during this workflow. Process one file and one finding at a time.

### Step 1: Find The Starting File

Navigate to the target directory. List all file names. Identify a file that appears to depend on the fewest other things, judging from name and location. Use it as the starting file.

### Step 2: Read The File

Read the file in full. Do not navigate to other files yet.

Adopt the perspective of a new programmer:

- Assume no prior project context.
- Every name must speak for itself.
- Every method must make sense in isolation.
- Logic must flow clearly without outside context.

If the file is too complex to be a least-dependent starting point, choose a simpler file and return to this one later.

### Step 3: Create An Overview

Write a short structured summary:

- What the file does.
- Key types, methods, and variables, and what each appears to mean from its name alone.
- How the internal logic flows.

### Step 4: Evaluate For Inconsistencies

Ask:

- Does every name accurately describe what it is?
- Does every method do exactly what its name suggests?
- Is the logic flow natural and consistent?
- Did anything surprise you or contradict its name?

Investigate before concluding. Read the surrounding context in the same file, consider reasonable explanations, and think of possible fixes.

If the confusion is only missing context, note it and continue. If something is genuinely inconsistent or misleading, surface it to the user.

### Step 5: Present Findings One By One

As soon as you find a real issue, stop the review and present that single finding before continuing. For each finding:

- Describe what was found and why it is confusing from the new-programmer perspective.
- Explain what was investigated.
- Offer concrete options such as fix it, skip it, or investigate further. Use a concise plain-text question when no structured question tool is available.

Act on the user's decision immediately before moving to the next finding.

### Step 6: Map Relationships

Before moving on, compare the current file to all files already processed:

- Does it reference, extend, implement, or depend on a previous file?
- Does a previous file reference it?
- Is there a conceptual relationship, such as mirrored structure or shared responsibility?

Record the relationship map explicitly. If the map reveals confusing coupling, wrong dependency direction, blurred responsibility, or surprising asymmetry, treat that as a finding.

### Step 7: Move To The Next File

Choose the next least-dependent file and repeat the workflow until the requested scope is complete.

## Principles

- One file at a time.
- One finding at a time.
- Never skip a real finding without surfacing it.
- Investigate confusion before presenting it as a problem.
- Treat cross-file relationships as first-class structural facts.

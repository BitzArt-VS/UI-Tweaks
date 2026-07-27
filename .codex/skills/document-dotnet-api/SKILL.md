---
name: document-dotnet-api
description: "Write and review concise, contextual C# XML documentation comments for .NET APIs. Use for summaries, remarks, and related documentation on types and members."
---

# Document .NET API

- Research actual behavior and usages comprehensively before proposing changes.
- Explain meaningful context, purpose, constraints, or consequences.
- Prefer simple, direct wording and avoid logical leaps.
- Document each API at its own conceptual layer; avoid explaining behavior imposed by higher-level consumers on lower-level logical layers unless it is a meaningful constraint relevant to the API itself.
- Avoid boilerplate such as “Gets or sets.”
- Avoid redundant articles and filler words.
- Use `<see cref="..."/>` for related types and members.
- Use `<see langword="..."/>` for language keywords.
- Keep `<summary>` concise and useful on its own.
- Add `<remarks>` only for non-obvious or complex behavior.
- Document meaningful differences among `null`, default, and explicit values.
- Mention capability limitations when substantial.
- Split distinct concepts into `<para>` blocks.
- Use clearly labeled paragraphs such as `<b>Note:</b>` for important caveats.
- Never add detail that merely restates the declaration or property signature.
- Ensure wording answers relevant “what for?”, “why?”, or “under what conditions?” questions without overexplaining.

# Workflow

- If this skill was invoked by user explicitly via `$document-dotnet-api` and used in conjunction with `$iterate`, before beginning to iterate, ask the user to establish a scope size for the iteration (a single signature, a single type, a single namespace) via the `request_user_input` tool, unless obvious from context.
- If used in conjunction with `$iterate`, do not validate (project build, test, etc.) on every iteration.

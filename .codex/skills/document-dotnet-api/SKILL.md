---
name: document-dotnet-api
description: "Write and review concise, contextual C# XML documentation comments for .NET APIs. Use for summaries, remarks, and related documentation on types and members."
---

# Document .NET API

- Research actual behavior and usages before writing.
- Explain meaningful context, purpose, constraints, or consequences.
- Prefer simple, direct wording over vague elaboration.
- Avoid boilerplate such as “Gets or sets.”
- Avoid redundant articles and filler words.
- Use `<see cref="..."/>` for related types and members.
- Use `<see langword="null"/>` for language keywords.
- Keep `<summary>` concise and useful on its own.
- Add `<remarks>` only for non-obvious or complex behavior.
- Document meaningful differences among `null`, default, and explicit values.
- Mention capability limitations when substantial.
- Split distinct concepts into `<para>` blocks.
- Use clearly labeled paragraphs such as `<b>Note:</b>` for important caveats.
- Never add detail that merely restates the declaration or property signature.
- Ensure wording answers relevant “what for?”, “why?”, or “under what conditions?” questions without overexplaining.

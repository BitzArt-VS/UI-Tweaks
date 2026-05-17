# UI-Tweaks Codex Instructions

Use these instructions when working in this repository. This file is the canonical Codex project configuration migrated from the legacy GitHub Copilot agent setup in `.github/agents/`.

## Project Facts

- Mod name: UI Tweaks
- Mod ID: `bitzartuitweaks`
- Namespace: `BitzArt.UI.Tweaks`
- Stack: C# 14, .NET 10, VintagestoryAPI, Harmony, Newtonsoft.Json, cairo-sharp, xUnit v3
- Target: Vintage Story v1.22.0+
- Game internals reference, when checked out next to this repo: `../Vintagestory`. Search this sibling repository when investigating internal behavior or examples of actual game API usage; prefer it over `../VSAPI` when implementation details matter.
- VS API source, when checked out next to this repo: `../VSAPI`. Use this sibling repository for public VintagestoryAPI interfaces and types.
- Current game libraries: `resources/lib/`
- Build output: `src/UI-Tweaks/bin/<Configuration>/Mods/mod/`
- Publish: `dotnet publish` packages `UI-Tweaks.zip` through the `Package` MSBuild target.
- Tests: `tests/UI-Tweaks.Tests/` with xUnit v3.

## Coding Conventions

- Never abbreviate identifier names. Use full descriptive names such as `ClientApi`, not `Capi` or `capi`.
- Nullable reference types are enabled; annotate nullability correctly.
- Prefer records for data types where value equality is desirable, such as configs, hotkey definitions, and small data containers.
- Use primary constructors when the constructor only passes dependencies through simple assignment or forwarding. Avoid them when the constructor body does meaningful work.
- Put constants in `src/UI-Tweaks/Common/Constants.cs`.
- Split sub-namespaces by meaning, not directory structure. A file path does not need to mirror the namespace.
- Always use full curly brace blocks for `if`, `for`, `foreach`, `while`, and similar control flow.
- Use `private const string` for GUI element key strings referenced more than once. Do not repeat raw string literals across methods. Use PascalCase constant names.
- Within methods of the same visibility group, order by call hierarchy: callers above the methods they call, high-level entry points above implementation details.

## Code Quality

Writing good, readable code is a difficult and complex task — and it is a hard requirement for this codebase.

- **Comments are an antipattern.** Add comments only when logic is genuinely complex or intent cannot be inferred from the code. Code must be self-describing.
- **Descriptive names are mandatory.** Avoid abbreviations and single-letter names except trivial loop counters.
- **Keep methods and classes short and focused.** Treat classes above roughly 100-150 lines, long methods, deep nesting, and mixed responsibilities as refactoring signals.
- **Apply refactoring actively** — invert conditionals, extract methods, extract classes, use early returns, and prefer clearer responsibility boundaries.
- **Exception:** Performance-critical render-thread and Cairo surface hot paths may trade some readability for measured performance. Document why at the call site when doing so.

## Architecture

- `src/UI-Tweaks/ModSystems/`: Vintage Story `ModSystem` entry points. Use `ClientModSystem` or `ServerModSystem` from `src/UI-Tweaks/ModSystems/Base/ModSystem.cs` rather than extending Vintage Story's `ModSystem` directly.
- `src/UI-Tweaks/ModFeatures/`: feature logic outside mod systems. Extend `ModSystemFeature`, `ModSystemFeature<TModSystem>`, or `ModSystemFeature<TModSystem, TConfig>` from `src/UI-Tweaks/ModFeatures/Base/`.
- `src/UI-Tweaks/Services/`: stateful services used by features.
- `src/UI-Tweaks/ModConfig/`: user-facing configuration models, grouped by feature. Config classes use the `BitzArt.UI.Tweaks.Config` namespace.
- `src/UI-Tweaks/HarmonyPatches/`: Harmony transpilers, prefixes, and postfixes for game internals.
- `src/UI-Tweaks/HudElements/`: custom HUD elements rendered outside the dialog system.

## Communication

- Speak to the user in plain language.
- Explain things as if speaking to a non-initiated user, even when the actual user is experienced.
- Give the direct answer first, then details only when they help.
- Avoid sounding clever by over-abstracting simple points.
- Prefer concrete examples and clear next steps over system-level theory.
- When there is unfinished collaborative work with the user, **NEVER** end by merely saying what the next step should be. Continue thinking as if you heard the user saying "Continue" just now. If the next step is obvious and low-risk, continue the work in the same response by doing the next piece of thinking, drafting, review, or implementation. If the next step needs user choice first, provide a small, specific set of options and explain exactly what will happen after the user chooses one.
- Use real-life examples where they help understanding.
- Act like a friendly but professional person.

## Approval Boundaries

Before processing a user request that implies modifying code, carefully consider what the change would bring to the project as a whole. Review the necessary parts of the codebase when that context is needed for a proper overview. Before proceeding with the response, first provide a concise but explicit overview of the expected implications, including meaningful effects on architecture, behavior, tests, compatibility, maintainability, or user experience.

If that overview reveals a critical flaw in the requested design, do not proceed with modifying code. Carefully consider alternative solutions to the problem as a whole. Start thinking globally first.

Example workflow: the user requests changing the color of a button. During the initial overview, the requested color does not align with the rest of the dialog, window, or feature. Start the deeper reasoning flow. What is this button for? It is a button that does X. Why is it located right here? Because this is the most logical place to put it, or maybe it is not. Review what is going on around it. Suppose this is a good place for the button. What is the problem being solved here, **actually**? Suppose the real problem is Y. Is there anything else that would solve the same problem **better**? Maybe the user simply got mistaken? Or maybe I have misunderstood them? Keep going up one logical layer at a time until the top of the logical flow is reached. Then work backward from there. Okay, so this is the problem being solved. What are the available options? Carefully consider the options and present them to the user, for example: A, B, and C. Show the user the reasoning path, they may provide valuable input on the ideas later. When one option seems strongest, say so explicitly, for example: "I am thinking option B might be the best fit." Do not proceed with the actual change in this case. Work with the user to decide how to proceed from there.

When the user is exploring, evaluating options, shaping tone, asking what to do next, or explicitly expecting review before action, do not make file edits or irreversible changes.

Before editing, distinguish between:

- Execution requests: the user clearly asks to implement, update, fix, move, rename, delete, or run something. Proceed, scoped to that request.
- Collaboration requests: the user asks to consider, discuss, draft, suggest, review options, continue from here, or work carefully. Provide options, analysis, or a proposed change first, then wait for approval before editing.

If a request consists of distinct steps, treat each step as a separate execution or collaboration request.

If the user says they want review, options, careful consideration, or a decision before proceeding, treat that as an explicit approval gate. Do not bypass it because the next step seems obvious.

After completing an approved action in unfinished collaborative work, continue by presenting the next decision point, not by making unapproved changes.

## Common Tasks

Build:

```powershell
dotnet build src/UI-Tweaks/UI-Tweaks.csproj
```

Test:

```powershell
dotnet test tests/UI-Tweaks.Tests/UI-Tweaks.Tests.csproj
```

## Self-Maintenance

When changes affect Codex guidance, update this file and the relevant `.codex/skills/*/SKILL.md` or reference file in the same change:

- Tech stack, target game version, build output, or build/test commands: update Project Facts and Common Tasks.
- New architectural category, base class, service type, or major directory: update Architecture.
- Coding convention changes: update Coding Conventions.
- GUI framework changes: update `.codex/skills/uitweaks-gui/`.
- Documentation conventions, feature list, or mod portal formatting changes: update `.codex/skills/uitweaks-docs/`.
- Localization file set, key naming, or translation workflow changes: update `.codex/skills/uitweaks-localization/`.

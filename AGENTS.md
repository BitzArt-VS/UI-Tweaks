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

## Repo-Local Skills

Project-specific skill sources live under `.codex/skills/`. These folders are repo-local instruction packs. Do not assume the harness will auto-register them as global skills; when a request falls into one of these areas, read the matching `SKILL.md` manually before acting:

- `.codex/skills/uitweaks-docs/`: documentation, `README.md`, `mod-description.html`, mdBook pages, and player-facing copy.
- `.codex/skills/uitweaks-gui/`: the custom Cairo GUI framework, `GuiDialog`-based dialogs, rendering, component model, layout, tooltips, mouse input, and keyboard input.
- `.codex/skills/uitweaks-localization/`: language files in `resources/assets/bitzartuitweaks/lang/`, translation keys, and user-facing strings.
- `.codex/skills/revalidation/`: revalidating an agent or skill configuration against the codebase.
- `.codex/skills/review-bottom-up/`: sequential bottom-up review for naming, structural clarity, and inconsistencies.

Each skill also contains optional `agents/openai.yaml` metadata for harnesses that support repo-local skill metadata. The repo-local `SKILL.md` files remain the behavior source of truth.

When the user explicitly asks for a docs subagent, docs agent, or delegated docs work, spawn the project-scoped `uitweaks_docs` custom agent from `.codex/agents/uitweaks-docs.toml`. Use it as a specialist reviewer, fact-checker, or parallel editor for docs, `README.md`, `mod-description.html`, and mdBook work. The user values documentation quality and accepts the extra overhead when they ask for docs delegation.

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

- Comments are an antipattern in this codebase. Add comments only when logic is genuinely complex or intent cannot be inferred from the code.
- Descriptive names are mandatory. Avoid abbreviations and single-letter names except trivial loop counters.
- Keep methods and classes short and focused. Treat classes above roughly 100-150 lines, long methods, deep nesting, and mixed responsibilities as refactoring signals.
- Refactor actively when it improves clarity: invert conditionals, extract methods, extract classes, use early returns, and prefer clearer responsibility boundaries.
- Performance-critical render-thread and Cairo surface hot paths may trade some readability for measured performance. Document why at the call site when doing so.

## Architecture

- `src/UI-Tweaks/ModSystems/`: Vintage Story `ModSystem` entry points. Use `ClientModSystem` or `ServerModSystem` from `src/UI-Tweaks/ModSystems/Base/ModSystem.cs` rather than extending Vintage Story's `ModSystem` directly.
- `src/UI-Tweaks/ModFeatures/`: feature logic outside mod systems. Extend `ModSystemFeature`, `ModSystemFeature<TModSystem>`, or `ModSystemFeature<TModSystem, TConfig>` from `src/UI-Tweaks/ModFeatures/Base/`.
- `src/UI-Tweaks/Services/`: stateful services used by features.
- `src/UI-Tweaks/ModConfig/`: user-facing configuration models, grouped by feature. Config classes use the `BitzArt.UI.Tweaks.Config` namespace.
- `src/UI-Tweaks/HarmonyPatches/`: Harmony transpilers, prefixes, and postfixes for game internals.
- `src/UI-Tweaks/HudElements/`: custom HUD elements rendered outside the dialog system.

## Search And References

- Prefer `rg` and `rg --files` for local search.
- When a behavior depends on Vintage Story internals, inspect `../Vintagestory` first if that sibling checkout is present.
- Use `../VSAPI` for API surface checks when that sibling checkout is present and implementation is not needed.
- Verify feature names, config option names, and UI strings against source before documenting them.

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

When the user is exploring, evaluating options, shaping tone, asking what to do next, or explicitly expecting review before action, do not make file edits or irreversible changes.

Before editing, distinguish between:

- Execution requests: the user clearly asks to implement, update, fix, move, rename, delete, or run something. Proceed, scoped to that request.
- Collaboration requests: the user asks to consider, discuss, draft, suggest, review options, continue from here, or work carefully. Provide options, analysis, or a proposed change first, then wait for approval before editing.
- Mixed requests: if part of the request is approved execution and part is still being decided, execute only the approved part and continue the undecided part with options or questions.

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

The legacy `.github/agents/` and `.github/skills/` files are retained for reference or Copilot compatibility. Treat `AGENTS.md` and `.codex/skills/` as the Codex source of truth.

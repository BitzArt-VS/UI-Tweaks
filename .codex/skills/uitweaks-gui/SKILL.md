---
name: uitweaks-gui
description: "Work on the UI-Tweaks custom Cairo GUI framework, GUI components, rendering pipelines, GuiDialog-based dialogs, dialog event dispatching, layout parameters, tooltips, mouse input, keyboard input, or dialogs implemented with the framework. Do not use for VanillaGuiDialog legacy GuiComposer dialogs."
---

# UI-Tweaks GUI Framework

## Scope

In scope:

- Everything inside `src/UI-Tweaks/Gui/Framework/`: framework internals, components, renderers, services.
- Dialogs that extend `GuiDialog`, such as `ModConfigDialog` in `src/UI-Tweaks/Gui/Dialogs/`.
- New `GuiComponent` or `GuiNode` subclasses, extensions, and GUI framework reference docs.

Out of scope:

- Dialogs that extend `VanillaGuiDialog`, such as `QuickSearchGuiDialog`; use the general project instructions.
- Code outside `Gui/`, such as features, services, config models, and Harmony patches.
- Localization files; use `.codex/skills/uitweaks-localization/`.

## Design Principles

- Blazor-without-XML: component trees are declared in C# through `BuildRenderTree(IGuiRenderTreeBuilder)`, `RenderFragment` subtree declarations, and CSS-inspired styling.
- Performance is a first-class constraint because this runs on the render thread. Minimize allocations, prefer structs where appropriate, reuse buffers, and avoid boxing. Faster but less readable code is acceptable only on proven hot paths.

## Framework Knowledge Base

Read the relevant reference file before working in each area:

| File | Covers |
| --- | --- |
| `references/01.overview.md` | Concept glossary and file map |
| `references/02.reconciliation.md` | Blueprint/diff/patch, lifecycle order, scoped rebuilds |
| `references/03.rendering-pipeline.md` | Frame loop, Cairo surface, bounds propagation, dialog bootstrap |
| `references/04.component-model.md` | `IGuiComponent` contract, configuration persistence, public API surface |
| `references/05.layout-parameters.md` | `GuiComponentLayoutParameters`, `GuiThickness`, `GuiComponentBounds`, layout pass |
| `references/06.tooltips.md` | Floating tooltip layer: `GuiTooltip`, `GuiTooltipBackground`, `TooltipHost`, `FloatingLayerRenderer` |
| `references/07.mouse-events.md` | Mouse input routing and event handling |
| `references/08.keyboard-events.md` | Keyboard input, focus model, caret blink, slot-level handlers, virtual hooks, Escape handling |

## Self-Maintenance

- If a new type is added anywhere in `Gui/Framework/`, add it to the file map in `references/01.overview.md`.
- If a new reference doc is added under `references/`, add it to the Framework Knowledge Base table.
- If scope changes, update the Scope section and frontmatter description.
- If design principles or code quality standards change, update this skill.

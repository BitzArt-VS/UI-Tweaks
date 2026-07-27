## {short API name/description, 1-3 words} design {iteration number, e.g. `1`, `2`, etc.}

### API design

For every change, include:

- **File:** [{file-name}]({file-path})

- For a removal, show a 🔴 **Remove** block.
- For an addition, show a 🟢 **Add** block.
- For a modification to existing API code, show a 🔴 **Before** block followed immediately by a 🟢 **After** block.

Prioritize **Remove** and **Add** blocks over **Before/After** blocks where ambiguous.

For every code block:

- Include the nearest containing type or member signature in both blocks as an inline comment to provide context.
- Use an ordinary language-specific fence.
- Include only enough unchanged code to locate and understand the affected API.
- Do not use inline diff markers, they don't work properly in this environment.

Briefly explain the reasoning behind this API design decision unless it is obvious from the snippets or was previously discussed via chat or in a design doc.

### Guidelines

- **Remove**, **Add**, **Before** and **After** code blocks are for comparing the current repository code with the proposed repository code—not comparing one conversational design with another.
- Do not include changes to project reference in API design proposals unless prompted otherwise.
- Do not repeat the same change multiple times in the same proposal unless necessary for explaining the reasoning behind the change.

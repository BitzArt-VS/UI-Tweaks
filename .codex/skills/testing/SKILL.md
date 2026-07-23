---
name: testing
description: "Captures project testing conventions for future agents. Use when adding new tests, reviewing existing tests, or evaluating test coverage."
---

# Testing

This reference captures project testing conventions for future agents. Prefer simple tests that explain behavior clearly over tests that mirror implementation details.

## Workflow

- Add new tests iteratively, one test at a time, so each test can be reviewed before the next one is written.
- When working on a new test, make sure to have received explicit user approval for this iteration before proceeding. Consider any given approval to never apply to multiple iterations.
- Whenever considering possible testing direction, prepare a list of possible candidates and propose them to the user via the `request_user_input` tool, and wait for the user to select one before proceeding.
- After adding each test, pause and let the user review whether it matches this project's testing practices and the expected feature behavior. Ask the user to approve the new test via the `request_user_input` tool before proceeding to the next test.
- Do not guess the desired testing direction when the intent is unclear. Ask the user what behavior, edge case, or layer they want covered before writing tests, and prefer using the `request_user_input` tool when it is available.
- Try to propose the most simple, obvious, and direct continuation of the existing test suite, do not try inventing clever cases or complex scenarios while simpler cases remain untested.
- Focus on proposing tests that cover the most critical path of the feature, and avoid adding tests that cover unimportant or unlikely edge cases unless the user explicitly requests them.
- Prefer small, focused test additions over broad test batches that are harder to review.

## Naming

Use this test name pattern:

```csharp
UnitOfWork_StateUnderTest_ExpectedBehavior
```

Example:

```csharp
public class FruitsTests
{
    [Fact]
    public void Grow_OnTree_ShouldIncreaseInSize()
    {
        // Arrange
        const int initialSize = 1;

        Fruit fruit = new Apple(size: initialSize);
        var tree = new AppleTree();

        // Act
        fruit.Grow(tree);

        // Assert
        Assert.True(fruit.Size > initialSize, "Fruit size should increase after growing.");
    }
}
```

## Structure

- Use `// Arrange`, `// Act`, and `// Assert` comments to make the test flow obvious.
- Feel free to combine `// Arrange`, `// Act`, and `// Assert` sections when necessary (e.g. checking for an exception being thrown during the act phase).
- Keep test code as simple and understandable as possible.
- Avoid complex branching, loops, helper logic, or hidden behavior inside tests.
- If setup becomes noisy, extract small, plainly named helper methods or builders that make the scenario easier to read.

## Scope and style

- Do not spend much effort classifying tests as unit vs. integration tests. That abstraction can conflict with practical reality.
- Focus on what behavior is being proven, what state is being exercised, and what outcome must hold.
- Prefer deterministic tests: avoid real network calls, clock-dependent behavior, random values, or shared mutable state unless the test specifically covers them.
- Prefer testing public or meaningful internal behavior over implementation details that can change without breaking the feature.
- Keep assertions specific enough to explain the expected behavior; avoid broad assertions that pass while the important behavior is broken.
- Avoid asserting exact exception messages unless the wording itself is part of the expected behavior.

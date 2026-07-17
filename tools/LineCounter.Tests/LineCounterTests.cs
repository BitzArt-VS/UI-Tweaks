using LineCounterTool;

namespace LineCounterTool.Tests;

public sealed class LineCounterTests
{
    [Fact]
    public void CountLines_IgnoresNonCodeLines()
    {
        string[] lines =
        [
            "using System;",
            "namespace Example;",
            "// comment",
            "/* block",
            "   comment */",
            "{",
            "}",
            "#nullable enable",
        ];

        Assert.Equal(0, LineCounter.CountLines(lines));
    }

    [Fact]
    public void CountLines_CountsMultilineStatementOnce()
    {
        string[] lines =
        [
            "string value = builder",
            "    .Append(\"first\")",
            "    .Append(\"second\")",
            "    .ToString();",
        ];

        Assert.Equal(1, LineCounter.CountLines(lines));
    }

    [Fact]
    public void CountLines_CountsDeclarationsAndControlFlow()
    {
        string[] lines =
        [
            "internal sealed class Example",
            "{",
            "    public void Run()",
            "    {",
            "        if (enabled)",
            "        {",
            "            Execute();",
            "        }",
            "    }",
            "}",
        ];

        Assert.Equal(4, LineCounter.CountLines(lines));
    }

    [Fact]
    public void CountLines_DoesNotCarryClosingBracesIntoNextDeclaration()
    {
        string[] lines =
        [
            "public int First",
            "{",
            "    get;",
            "}",
            "public int Second",
            "{",
            "    get;",
            "}",
        ];

        Assert.Equal(4, LineCounter.CountLines(lines));
    }

    [Fact]
    public void CountLines_CountsUsingDeclarationButNotUsingDirective()
    {
        string[] lines =
        [
            "using System.IO;",
            "using Alias = System.IO.Stream;",
            "using var stream = File.OpenRead(path);",
        ];

        Assert.Equal(1, LineCounter.CountLines(lines));
    }

    [Fact]
    public void CountLines_DoesNotTreatCommentMarkersInStringsAsComments()
    {
        string[] lines =
        [
            "string url = \"https://example.test/path\";",
            "string marker = \"/* not a comment */\"; // actual comment",
        ];

        Assert.Equal(2, LineCounter.CountLines(lines));
    }

    [Fact]
    public void CountLines_CountsForLoopAsOneControlLine()
    {
        string[] lines =
        [
            "for (int index = 0;",
            "     index < 10;",
            "     index++)",
            "{",
            "    Execute();",
            "}",
        ];

        Assert.Equal(2, LineCounter.CountLines(lines));
    }
}

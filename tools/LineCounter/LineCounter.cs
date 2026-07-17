using System.Text;
using System.Text.RegularExpressions;

namespace LineCounterTool;

internal static partial class LineCounter
{
    private static readonly char[] DirectorySeparators = [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar];

    internal static SolutionLineCount CountSolution(string solutionPath)
    {
        string fullSolutionPath = Path.GetFullPath(solutionPath);
        if (!File.Exists(fullSolutionPath))
        {
            throw new FileNotFoundException("The solution file was not found.", fullSolutionPath);
        }

        string solutionDirectory = Path.GetDirectoryName(fullSolutionPath)!;
        List<ProjectLineCount> projects = [];

        foreach ((string projectName, string relativePath) in ReadCSharpProjects(fullSolutionPath))
        {
            string projectPath = Path.GetFullPath(
                relativePath.Replace('\\', Path.DirectorySeparatorChar),
                solutionDirectory);

            if (!File.Exists(projectPath))
            {
                throw new InvalidDataException($"Project '{projectName}' was not found at '{projectPath}'.");
            }

            string projectDirectory = Path.GetDirectoryName(projectPath)!;
            string[] files = Directory
                .EnumerateFiles(projectDirectory, "*.cs", SearchOption.AllDirectories)
                .Where(IsSourceFile)
                .Order(StringComparer.Ordinal)
                .ToArray();

            int lineCount = files.Sum(CountFile);
            projects.Add(new ProjectLineCount(projectName, files.Length, lineCount));
        }

        if (projects.Count == 0)
        {
            throw new InvalidDataException("The solution does not contain any C# projects.");
        }

        return new SolutionLineCount(projects);
    }

    internal static int CountFile(string filePath)
    {
        using StreamReader reader = File.OpenText(filePath);
        return CountLines(ReadLines(reader));
    }

    internal static int CountLines(IEnumerable<string> lines)
    {
        LexicalCleaner cleaner = new();
        StringBuilder pending = new();
        int count = 0;
        int parenthesisDepth = 0;

        foreach (string sourceLine in lines)
        {
            string line = cleaner.Clean(sourceLine).Trim();
            if (line.Length == 0 || line[0] == '#')
            {
                continue;
            }

            if (pending.Length == 0)
            {
                line = line.TrimStart('}').TrimStart();
                if (line.Length == 0)
                {
                    continue;
                }
            }

            if (pending.Length == 0 && IsIgnoredDirective(line))
            {
                continue;
            }

            bool countedThisLine = false;
            AppendWithSpace(pending, line);

            for (int index = 0; index < line.Length; index++)
            {
                switch (line[index])
                {
                    case '(':
                        parenthesisDepth++;
                        break;
                    case ')':
                        parenthesisDepth = Math.Max(0, parenthesisDepth - 1);
                        break;
                    case ';' when parenthesisDepth == 0:
                        countedThisLine = true;
                        pending.Clear();
                        break;
                    case '{' when parenthesisDepth == 0 && IsBlockHeader(pending.ToString(0, Math.Max(0, pending.Length - line.Length + index))):
                        countedThisLine = true;
                        pending.Clear();
                        break;
                }
            }

            if (pending.Length > 0 && parenthesisDepth == 0 && IsStandaloneControlHeader(pending.ToString()))
            {
                countedThisLine = true;
                pending.Clear();
            }

            if (countedThisLine)
            {
                count++;
            }
        }

        return count;
    }

    private static IEnumerable<string> ReadLines(TextReader reader)
    {
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            yield return line;
        }
    }

    private static IEnumerable<(string Name, string Path)> ReadCSharpProjects(string solutionPath)
    {
        HashSet<string> seenPaths = new(StringComparer.OrdinalIgnoreCase);

        foreach (string line in File.ReadLines(solutionPath))
        {
            Match match = SolutionProjectPattern().Match(line);
            if (!match.Success)
            {
                continue;
            }

            string path = match.Groups["path"].Value;
            if (path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase) && seenPaths.Add(path))
            {
                yield return (match.Groups["name"].Value, path);
            }
        }
    }

    private static bool IsSourceFile(string path)
    {
        return !path
            .Split(DirectorySeparators, StringSplitOptions.RemoveEmptyEntries)
            .Any(part => part.Equals("bin", StringComparison.OrdinalIgnoreCase)
                || part.Equals("obj", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsIgnoredDirective(string line)
    {
        string value = line.StartsWith("global ", StringComparison.Ordinal)
            ? line["global ".Length..].TrimStart()
            : line;

        if (value.StartsWith("namespace ", StringComparison.Ordinal))
        {
            return true;
        }

        if (!value.StartsWith("using ", StringComparison.Ordinal) || value.StartsWith("using (", StringComparison.Ordinal))
        {
            return false;
        }

        string target = value["using ".Length..].TrimStart();
        if (target.StartsWith("var ", StringComparison.Ordinal))
        {
            return false;
        }

        int assignmentIndex = target.IndexOf('=');
        return assignmentIndex < 0
            || !target[..assignmentIndex].Trim().Any(char.IsWhiteSpace);
    }

    private static bool IsBlockHeader(string value)
    {
        string header = value.Trim();
        if (header.Length == 0 || header.Contains("=>", StringComparison.Ordinal))
        {
            return false;
        }

        return StartsWithKeyword(header, "if", "else", "switch", "for", "foreach", "while", "do", "try", "catch", "finally", "lock", "using", "checked", "unchecked", "unsafe", "fixed")
            || ContainsTypeKeyword(header)
            || header.EndsWith(')') && !ContainsAssignment(header)
            || StartsWithKeyword(header, "public", "private", "protected", "internal") && !ContainsAssignment(header);
    }

    private static bool IsStandaloneControlHeader(string value)
    {
        string header = value.Trim();
        return header is "else" or "do" or "try" or "finally"
            || StartsWithKeyword(header, "if", "else if", "switch", "for", "foreach", "while", "catch", "lock", "using", "fixed")
                && header.EndsWith(')');
    }

    private static bool ContainsTypeKeyword(string value)
    {
        return Regex.IsMatch(value, @"(?:^|\s)(?:class|struct|interface|enum|record)(?:\s|$)");
    }

    private static bool ContainsAssignment(string value)
    {
        for (int index = 0; index < value.Length; index++)
        {
            if (value[index] != '=')
            {
                continue;
            }

            char previous = index > 0 ? value[index - 1] : '\0';
            char next = index + 1 < value.Length ? value[index + 1] : '\0';
            if (previous is not ('=' or '!' or '<' or '>') && next is not ('=' or '>'))
            {
                return true;
            }
        }

        return false;
    }

    private static bool StartsWithKeyword(string value, params string[] keywords)
    {
        return keywords.Any(keyword => value.Equals(keyword, StringComparison.Ordinal)
            || value.StartsWith(keyword + " ", StringComparison.Ordinal)
            || value.StartsWith(keyword + "(", StringComparison.Ordinal));
    }

    private static void AppendWithSpace(StringBuilder builder, string value)
    {
        if (builder.Length > 0)
        {
            builder.Append(' ');
        }

        builder.Append(value);
    }

    [GeneratedRegex("^Project\\(\"[^\"]+\"\\) = \"(?<name>[^\"]+)\", \"(?<path>[^\"]+)\"")]
    private static partial Regex SolutionProjectPattern();

    private sealed class LexicalCleaner
    {
        private LexicalState state;
        private int rawQuoteCount;
        private bool escaped;

        internal string Clean(string line)
        {
            StringBuilder result = new(line.Length);

            for (int index = 0; index < line.Length; index++)
            {
                char current = line[index];
                char next = index + 1 < line.Length ? line[index + 1] : '\0';

                switch (state)
                {
                    case LexicalState.BlockComment:
                        if (current == '*' && next == '/')
                        {
                            state = LexicalState.Code;
                            index++;
                        }
                        break;

                    case LexicalState.RegularString:
                        if (current == '"' && !escaped)
                        {
                            state = LexicalState.Code;
                        }

                        escaped = current == '\\' && !escaped;
                        if (current != '\\')
                        {
                            escaped = false;
                        }
                        break;

                    case LexicalState.VerbatimString:
                        if (current == '"' && next == '"')
                        {
                            index++;
                        }
                        else if (current == '"')
                        {
                            state = LexicalState.Code;
                        }
                        break;

                    case LexicalState.Character:
                        if (current == '\'' && !escaped)
                        {
                            state = LexicalState.Code;
                        }

                        escaped = current == '\\' && !escaped;
                        if (current != '\\')
                        {
                            escaped = false;
                        }
                        break;

                    case LexicalState.RawString:
                        if (current == '"' && CountRun(line, index, '"') >= rawQuoteCount)
                        {
                            index += rawQuoteCount - 1;
                            state = LexicalState.Code;
                        }
                        break;

                    default:
                        if (current == '/' && next == '/')
                        {
                            return result.ToString();
                        }

                        if (current == '/' && next == '*')
                        {
                            state = LexicalState.BlockComment;
                            index++;
                            continue;
                        }

                        int quoteRun = current == '"' ? CountRun(line, index, '"') : 0;
                        if (quoteRun >= 3)
                        {
                            state = LexicalState.RawString;
                            rawQuoteCount = quoteRun;
                            result.Append("\"\"");
                            index += quoteRun - 1;
                        }
                        else if (current == '@' && next == '"')
                        {
                            state = LexicalState.VerbatimString;
                            result.Append("\"\"");
                            index++;
                        }
                        else if (current == '"')
                        {
                            state = LexicalState.RegularString;
                            escaped = false;
                            result.Append("\"\"");
                        }
                        else if (current == '\'')
                        {
                            state = LexicalState.Character;
                            escaped = false;
                            result.Append("''");
                        }
                        else
                        {
                            result.Append(current);
                        }
                        break;
                }
            }

            return result.ToString();
        }

        private static int CountRun(string value, int start, char character)
        {
            int count = 0;
            while (start + count < value.Length && value[start + count] == character)
            {
                count++;
            }

            return count;
        }
    }

    private enum LexicalState
    {
        Code,
        BlockComment,
        RegularString,
        VerbatimString,
        Character,
        RawString,
    }
}

internal sealed record ProjectLineCount(
    string ProjectName,
    int FileCount,
    int LineCount);

internal sealed record SolutionLineCount(
    IReadOnlyList<ProjectLineCount> Projects)
{
    internal int FileCount => Projects.Sum(project => project.FileCount);

    internal int LineCount => Projects.Sum(project => project.LineCount);
}

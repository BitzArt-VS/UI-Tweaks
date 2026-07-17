namespace LineCounterTool;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length > 1)
        {
            Console.Error.WriteLine("Usage: dotnet run --project tools/LineCounter/LineCounter.csproj -- [solution-path]");
            return 2;
        }

        string solutionPath = args.Length == 1 ? args[0] : "UI-Tweaks.sln";

        try
        {
            SolutionLineCount result = LineCounter.CountSolution(solutionPath);

            foreach (ProjectLineCount project in result.Projects)
            {
                Console.WriteLine($"{project.ProjectName,-24} {project.FileCount,5} files {project.LineCount,8:N0} lines");
            }

            Console.WriteLine(new string('-', 51));
            Console.WriteLine($"{"Total",-24} {result.FileCount,5} files {result.LineCount,8:N0} lines");
            return 0;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidDataException)
        {
            Console.Error.WriteLine($"Line counter failed: {exception.Message}");
            return 1;
        }
    }
}

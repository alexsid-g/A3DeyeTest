namespace Benchmark.Utils;

public static class ConsoleUtils
{
    public static void WriteErrorLine(string message) =>
        WriteColorLine(ConsoleColor.Red, message);

    public static void WriteInfoLine(string message) =>
        WriteColorLine(ConsoleColor.Green, message);

    private static void WriteColorLine(ConsoleColor color, string line)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(line);
        Console.ResetColor();    
    }
}

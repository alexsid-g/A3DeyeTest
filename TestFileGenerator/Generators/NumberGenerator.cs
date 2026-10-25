namespace TestFileGenerator.Generators;

public static class NumberGenerator
{
    public static long GenerateRandomLong()
    {
        return Random.Shared.NextInt64();
    }
}

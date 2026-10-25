using TestFileGenerator.Data;

namespace TestFileGenerator.Generators;

public static class NumberValueGenerator
{
    private static readonly Queue<NumberValue> Pool = new();

    public static NumberValue GenerateNumberValue()
    {
        if (!Pool.TryDequeue(out NumberValue? value))
        {
            value = new NumberValue();
        }

        value.Number = NumberGenerator.GenerateRandomLong();
        value.Value = StringGenerator.GenerateRandomString();
        return value;
    }

    public static void Return(NumberValue value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Pool.Enqueue(value);
    }
}

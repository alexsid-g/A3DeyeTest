using System.Text;
using TestFileGenerator.Data;
using TestFileGenerator.Generators;

namespace TestFileGenerator.Strategies;

public sealed class SimpleStrategy : IStrategy
{
    public void Run(GenerateOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.OutputFilePath))
        {
            throw new ArgumentException("An output file path is required.", nameof(options));
        }

        long targetFileSize = GenerateOptions.ParseTargetFileSize(options.TargetFileSize);
        if (targetFileSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Target file size must be positive.");
        }

        using FileStream output = new(
            options.OutputFilePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None);

        long bytesRemaining = targetFileSize;
        while (bytesRemaining > 0)
        {
            NumberValue numberValue = NumberValueGenerator.GenerateNumberValue();
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(
                    $"{numberValue.Number}. {numberValue.Value}\n");
        
                output.Write(buffer);
                bytesRemaining -= buffer.Length;
            }
            finally
            {
                NumberValueGenerator.Return(numberValue);
            }
        }
    }
}

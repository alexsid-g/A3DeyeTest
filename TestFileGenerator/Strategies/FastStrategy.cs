using System.Buffers;
using System.Text;
using TestFileGenerator.Data;
using TestFileGenerator.Generators;
using TestFileGenerator.Streams;

namespace TestFileGenerator.Strategies;

public sealed class FastStrategy : IStrategy
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

        byte[] buffer = ArrayPool<byte>.Shared.Rent(1024);
        using BufferedFileStream output = new(options.OutputFilePath, 1024 * 1024);

        try
        {
            long bytesRemaining = targetFileSize;
            while (bytesRemaining > 0)
            {
                NumberValue numberValue = NumberValueGenerator.GenerateNumberValue();

                try
                {                
                    // "Number. String value\n"
                    numberValue.Number.TryFormat(buffer, out int bytesWritten);

                    // separator
                    Span<byte> destination = buffer;
                    destination[bytesWritten++] = (byte)'.';
                    destination[bytesWritten++] = (byte)' ';

                    // string value
                    bytesWritten += Encoding.UTF8.GetBytes(
                        numberValue.Value.AsSpan(), destination[bytesWritten..]);

                    // newline
                    destination[bytesWritten++] = (byte)'\n';
            
                    output.Write(buffer, bytesWritten);
                    bytesRemaining -= bytesWritten;
                }
                finally
                {
                    NumberValueGenerator.Return(numberValue);
                }
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}

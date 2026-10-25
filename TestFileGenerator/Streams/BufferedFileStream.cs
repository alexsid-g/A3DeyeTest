namespace TestFileGenerator.Streams;

public sealed class BufferedFileStream : IDisposable
{
    private readonly Stream _output;

    public BufferedFileStream(string filePath, int bufferSize)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bufferSize);

        FileStream fileStream = new(
            filePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None);

        try
        {
            _output = new BufferedStream(fileStream, bufferSize);
        }
        catch
        {
            fileStream.Dispose();
            throw;
        }
    }

    public void Write(byte[] buffer, int length)
    {
        _output.Write(buffer, 0, length);
    }

    public void Dispose()
    {
        _output.Flush();
        _output.Dispose();
    }
}
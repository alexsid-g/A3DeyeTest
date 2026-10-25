using TestFileGenerator.Data;

namespace TestFileGenerator.Strategies;

public interface IStrategy
{
    void Run(GenerateOptions options);
}

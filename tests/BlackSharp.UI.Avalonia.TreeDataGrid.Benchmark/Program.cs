using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Benchmark;

internal class Program
{
    #region Private

    private static void Main(string[] args)
    {
        try
        {
            if (args.Contains("--profile"))
            {
                PerformanceProfile.Run();
            }
            else
            {
                BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, DefaultConfig.Instance
                    .AddDiagnoser(MemoryDiagnoser.Default));
            }
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            Environment.ExitCode = 1;
        }
    }

    #endregion
}

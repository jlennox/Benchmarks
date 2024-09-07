using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Filters;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;

namespace Benchmarks;

internal class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine($"AdvSimd.Arm64.IsSupported: {AdvSimd.Arm64.IsSupported}");
        Console.WriteLine($"Sse2.IsSupported: {Sse2.IsSupported}");
        Console.WriteLine($"Vector128.IsHardwareAccelerated: {Vector128.IsHardwareAccelerated}");
        Console.WriteLine($"Vector64.IsHardwareAccelerated: {Vector64.IsHardwareAccelerated}");
        Console.WriteLine($"Bmi1.IsSupported: {Bmi1.IsSupported}");

        if (args.Length == 0) args = ["--palette"];

        switch (args[0].ToLowerInvariant())
        {
            case "--guid":
                RunGuidBenchmarks();
                break;
            case "--uint128":
                RunUint128Benchmarks();
                break;
            case "--palette":
                RunSKBitmapPaletteBench();
                break;
            case "--swap":
                RunBranchlessSwap();
                break;
        }
    }

    private static void RunGuidBenchmarks()
    {
        new GuidTest().TestCompareEquality();

        // RunTests<GuidCompareToBenchmarks>();
        RunTests<GuidGreaterThanBenchmarks>("");
    }

    private static void RunUint128Benchmarks()
    {
        new Uint128GreaterThanBenchmarks().Normal();
        RunTests<Uint128GreaterThanBenchmarks>();
    }

    private static void RunSKBitmapPaletteBench()
    {
        SKBitmapPaletteBench.Test();
        RunTests<SKBitmapPaletteBench>();
    }

    private static void RunBranchlessSwap()
    {
        BranchlessSwap.Test();
        RunTests<BranchlessSwap>();
    }

    private static void RunTests<T>(string prefix = "")
    {
        BenchmarkRunner.Run(typeof(T), ManualConfig
            .Create(DefaultConfig.Instance)
            .AddFilter(new HardwareSupportFilter())
            .AddFilter(new BenchmarksPrefixFilter(prefix))
            .WithOrderer(new DefaultOrderer(SummaryOrderPolicy.Declared, MethodOrderPolicy.Alphabetical))
            .WithOptions(ConfigOptions.JoinSummary | ConfigOptions.StopOnFirstError | ConfigOptions.DisableLogFile));
    }
}

internal class HardwareSupportFilter : IFilter
{
    public bool Predicate(BenchmarkCase benchmarkCase)
    {
        var testName = benchmarkCase.Descriptor.WorkloadMethodDisplayInfo;
        var requiresSse2 = testName.EndsWith("sse2", StringComparison.InvariantCultureIgnoreCase);
        var requiresAvx2 = testName.EndsWith("avx2", StringComparison.InvariantCultureIgnoreCase);
        var requiresVector64 = testName.EndsWith("vector64", StringComparison.InvariantCultureIgnoreCase);

        if (requiresSse2) return Sse2.IsSupported;
        if (requiresAvx2) return Avx2.IsSupported;
        if (requiresVector64) return Vector64.IsHardwareAccelerated;

        return true;
    }
}

internal class BenchmarksPrefixFilter(string prefix) : IFilter
{
    public bool Predicate(BenchmarkCase benchmarkCase)
    {
        if (string.IsNullOrEmpty(prefix)) return true;

        var testName = benchmarkCase.Descriptor.WorkloadMethodDisplayInfo;
        return testName.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase);
    }
}
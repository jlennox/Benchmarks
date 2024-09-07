using System;
using System.Numerics;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;

namespace Benchmarks;

[CLSCompliant(false)]
// [Intrinsic]
[StructLayout(LayoutKind.Sequential)]
public readonly struct UInt128Bench
{
    internal const int Size = 16;

#if BIGENDIAN
        private readonly ulong _upper;
        private readonly ulong _lower;
#else
    private readonly ulong _lower;
    private readonly ulong _upper;
#endif

    /// <summary>Initializes a new instance of the <see cref="UInt128" /> struct.</summary>
    /// <param name="upper">The upper 64-bits of the 128-bit value.</param>
    /// <param name="lower">The lower 64-bits of the 128-bit value.</param>
    [CLSCompliant(false)]
    public UInt128Bench(ulong upper, ulong lower)
    {
        _lower = lower;
        _upper = upper;
    }

    internal ulong Lower => _lower;

    internal ulong Upper => _upper;


    /// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
    public static bool operator >(UInt128Bench left, UInt128Bench right)
    {
        return (left._upper > right._upper)
            || (left._upper == right._upper) && (left._lower > right._lower);
    }

    /// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
    public static bool operator <(UInt128Bench left, UInt128Bench right)
    {
        return (left._upper < right._upper)
            || (left._upper == right._upper) && (left._lower < right._lower);
    }
}

[CLSCompliant(false)]
// [Intrinsic]
[StructLayout(LayoutKind.Sequential)]
public readonly struct UInt128BenchOptimized
{
    internal const int Size = 16;

#if BIGENDIAN
        private readonly ulong _upper;
        private readonly ulong _lower;
#else
    private readonly ulong _lower;
    private readonly ulong _upper;
#endif

    /// <summary>Initializes a new instance of the <see cref="UInt128" /> struct.</summary>
    /// <param name="upper">The upper 64-bits of the 128-bit value.</param>
    /// <param name="lower">The lower 64-bits of the 128-bit value.</param>
    [CLSCompliant(false)]
    public UInt128BenchOptimized(ulong upper, ulong lower)
    {
        _lower = lower;
        _upper = upper;
    }

    internal ulong Lower => _lower;

    internal ulong Upper => _upper;


    /// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
    public static bool operator >(UInt128BenchOptimized left, UInt128BenchOptimized right)
    {
        return (left._upper != right._upper)
             ? (left._upper > right._upper)
             : (left._lower > right._lower);
    }

    /// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
    public static bool operator <(UInt128BenchOptimized left, UInt128BenchOptimized right)
    {
        return (left._upper != right._upper)
             ? (left._upper < right._upper)
             : (left._lower < right._lower);
    }
}


public class Uint128GreaterThanBenchmarks
{
    private readonly static UInt128Bench _a = new(0x10, 0x10);
    private readonly static UInt128Bench _b = new(0x0, 0x10);
    private readonly static UInt128Bench _c = new(0x10, 0x0);
    private readonly static UInt128Bench[] _all = [_a, _b, _c];

    private readonly static UInt128BenchOptimized _aO = new(0x10, 0x10);
    private readonly static UInt128BenchOptimized _bO = new(0x0, 0x10);
    private readonly static UInt128BenchOptimized _cO = new(0x10, 0x0);
    private readonly static UInt128BenchOptimized[] _allO = [_aO, _bO, _cO];

    [Benchmark]
    public void Normal()
    {
        int forcedSideEffect = 0;
        var all = _all;
        foreach (var a in all)
        {
            foreach (var b in all)
            {
                if (a > b) ++forcedSideEffect;
            }
        }

        if (forcedSideEffect == 0) throw new Exception();
    }

    [Benchmark]
    public void Optimized()
    {
        int forcedSideEffect = 0;
        var all = _allO;
        foreach (var a in all)
        {
            foreach (var b in all)
            {
                if (a > b) ++forcedSideEffect;
            }
        }

        if (forcedSideEffect == 0) throw new Exception();
    }
}
using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Attributes;
using Microsoft.Diagnostics.Tracing.StackSources;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Benchmarks;

internal unsafe readonly partial struct GuidBench
{
    private readonly int _a;   // Do not rename (binary serialization)
    private readonly short _b; // Do not rename (binary serialization)
    private readonly short _c; // Do not rename (binary serialization)
    private readonly byte _d;  // Do not rename (binary serialization)
    private readonly byte _e;  // Do not rename (binary serialization)
    private readonly byte _f;  // Do not rename (binary serialization)
    private readonly byte _g;  // Do not rename (binary serialization)
    private readonly byte _h;  // Do not rename (binary serialization)
    private readonly byte _i;  // Do not rename (binary serialization)
    private readonly byte _j;  // Do not rename (binary serialization)
    private readonly byte _k;  // Do not rename (binary serialization)

    public static readonly GuidBench GuidA = new(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
    public static readonly GuidBench GuidB = new(0x10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0);
    public static readonly GuidBench GuidB2 = new(0x0910, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0);
    public static readonly GuidBench GuidB3 = new(0x1009, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0);
    public static readonly GuidBench GuidB4 = new(0x1009, 9, 8, 7, 6, 5, 4, 3, 10, 1, 0);
    public static readonly GuidBench GuidB5 = new(0x1009, 9, 8, 7, 6, 5, 4, 3, 0, 1, 3);
    public static readonly GuidBench GuidB6 = new(0x1009, 9, 8, 7, 6, 5, 4, 3, 0, 0x01, 0xC0);
    public static readonly GuidBench GuidB7 = new(0x1009, 9, 8, 7, 6, 5, 4, 3, 0, 0xF0, 0xF0);
    public static readonly GuidBench GuidA2 = new(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 11);
    public static readonly GuidBench GuidMax = new(0xFFFFFF, 0xFFFF, 0xFFFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);

    public static readonly GuidBench[] TestGuids = [GuidA, GuidB, GuidB2, GuidB3, GuidA2, GuidB4, GuidB5, GuidB6, GuidB7, GuidMax];

    public GuidBench(uint a, ushort b, ushort c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k)
    {
        _a = (int)a;
        _b = (short)b;
        _c = (short)c;
        _d = d;
        _e = e;
        _f = f;
        _g = g;
        _h = h;
        _i = i;
        _j = j;
        _k = k;
    }

    public GuidBench(Guid g)
    {
        _a = (int)typeof(Guid).GetField("_a", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(g);
        _b = (short)typeof(Guid).GetField("_b", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(g);
        _c = (short)typeof(Guid).GetField("_c", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(g);
        _d = (byte)typeof(Guid).GetField("_d", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(g);
        _e = (byte)typeof(Guid).GetField("_e", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(g);
        _f = (byte)typeof(Guid).GetField("_f", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(g);
        _g = (byte)typeof(Guid).GetField("_g", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(g);
        _h = (byte)typeof(Guid).GetField("_h", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(g);
        _i = (byte)typeof(Guid).GetField("_i", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(g);
        _j = (byte)typeof(Guid).GetField("_j", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(g);
        _k = (byte)typeof(Guid).GetField("_k", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(g);
    }

    public static bool GreaterThanNormal(in GuidBench left, in GuidBench right)
    {
        if (left._a != right._a)
        {
            return (uint)left._a > (uint)right._a;
        }

        if (left._b != right._b)
        {
            return (uint)left._b > (uint)right._b;
        }

        if (left._c != right._c)
        {
            return (uint)left._c > (uint)right._c;
        }

        if (left._d != right._d)
        {
            return left._d > right._d;
        }

        if (left._e != right._e)
        {
            return left._e > right._e;
        }

        if (left._f != right._f)
        {
            return left._f > right._f;
        }

        if (left._g != right._g)
        {
            return left._g > right._g;
        }

        if (left._h != right._h)
        {
            return left._h > right._h;
        }

        if (left._i != right._i)
        {
            return left._i > right._i;
        }

        if (left._j != right._j)
        {
            return left._j > right._j;
        }

        if (left._k != right._k)
        {
            return left._k > right._k;
        }

        return false;
    }

    private static string ToBinary16(uint i, int length = 16)
    {
        string bin = Convert.ToString(Math.Abs(i), 2);
        string asd = bin.PadLeft(length, '0');
        return asd[..4] + "-" + asd.Substring(4, 2) + "-" + asd.Substring(8, 2) + "-" + asd[10..];
    }

    private static string ToBinary32(uint i, int length = 32)
    {
        string bin = Convert.ToString(Math.Abs(i), 2);
        string asd = bin.PadLeft(length, '0');
        return asd[..8] + "-" + asd.Substring(8, 4) + "-" + asd.Substring(16, 4) + "-" + asd[20..];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128Bench AsUInt128(in GuidBench guid)
    {
        if (BitConverter.IsLittleEndian)
        {
            return new UInt128Bench(
                ((ulong)(uint)guid._a << 32) | uint.RotateLeft(Unsafe.ReadUnaligned<uint>(ref Unsafe.As<short, byte>(ref Unsafe.AsRef(in guid._b))), 16),
                BinaryPrimitives.ReverseEndianness(Unsafe.ReadUnaligned<ulong>(ref Unsafe.AsRef(in guid._d)))
            );
        }
        else
        {
            return Unsafe.BitCast<GuidBench, UInt128Bench>(guid);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<byte> AsVector128(in GuidBench guid)
    {
        if (BitConverter.IsLittleEndian)
        {
            Unsafe.SkipInit(out Vector128<ulong> result);

            result = result.WithUpper(Vector64.Create(((ulong)(uint)guid._a << 32) | uint.RotateLeft(Unsafe.ReadUnaligned<uint>(ref Unsafe.As<short, byte>(ref Unsafe.AsRef(in guid._b))), 16)));
            result = result.WithLower(Vector64.Create(BinaryPrimitives.ReverseEndianness(Unsafe.ReadUnaligned<ulong>(ref Unsafe.AsRef(in guid._d)))));

            return result.AsByte();
        }
        else
        {
            return Unsafe.BitCast<GuidBench, Vector128<byte>>(guid);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128BenchOptimized AsUInt128Optimized(in GuidBench guid)
    {
        if (BitConverter.IsLittleEndian)
        {
            return new UInt128BenchOptimized(
                ((ulong)(uint)guid._a << 32) | uint.RotateLeft(Unsafe.ReadUnaligned<uint>(ref Unsafe.As<short, byte>(ref Unsafe.AsRef(in guid._b))), 16),
                BinaryPrimitives.ReverseEndianness(Unsafe.ReadUnaligned<ulong>(ref Unsafe.AsRef(in guid._d)))
            );
        }
        else
        {
            return Unsafe.BitCast<GuidBench, UInt128BenchOptimized>(guid);
        }
    }

    public static bool GreaterThanInt128(in GuidBench left, in GuidBench right)
    {
        return AsUInt128(left) > AsUInt128(right);
    }

    public static bool GreaterThanInt128Optimized(in GuidBench left, in GuidBench right)
    {
        return AsUInt128Optimized(left) > AsUInt128Optimized(right);
    }

    private static readonly Vector128<byte> _littleEndianShuffleMask = Vector128.Create<byte>(
        [3, 2, 1, 0, 5, 4, 7, 6, 8, 9, 10, 11, 12, 13, 14, 15]);

    public static bool GreaterThanLong(in GuidBench left, in GuidBench right)
    {
        if (left._a != right._a)
        {
            return (uint)left._a > (uint)right._a;
        }

        if (left._b != right._b)
        {
            return (uint)left._b > right._b;
        }

        if (left._c != right._c)
        {
            return (uint)left._c > right._c;
        }

        if (BitConverter.IsLittleEndian)
        {
            // Only compare ordered-bytes to avoid expensive Shuffle operations.
            // Use ulong to prevent a non-inlined call to TrailingZeroCount for `tzcnt`, which happens with long.
            ulong leftTail = Unsafe.As<byte, ulong>(ref Unsafe.AsRef(in left._d));
            ulong rightTail = Unsafe.As<byte, ulong>(ref Unsafe.AsRef(in right._d));

            ulong diff = leftTail ^ rightTail;
            if (diff == 0)
            {
                return false;
            }

            int firstDifferentBit = BitOperations.TrailingZeroCount(diff);
            // The comparisons are per byte, so align our comparison to a byte boundary.
            firstDifferentBit -= firstDifferentBit % 8;
            ulong leftDifferentByte = (leftTail >> firstDifferentBit) & 0xFF;
            ulong rightDifferentByte = (rightTail >> firstDifferentBit) & 0xFF;

            return leftDifferentByte > rightDifferentByte;
        }

        if (left._d != right._d)
        {
            return left._d > right._d;
        }

        if (left._e != right._e)
        {
            return left._e > right._e;
        }

        if (left._f != right._f)
        {
            return left._f > right._f;
        }

        if (left._g != right._g)
        {
            return left._g > right._g;
        }

        if (left._h != right._h)
        {
            return left._h > right._h;
        }

        if (left._i != right._i)
        {
            return left._i > right._i;
        }

        if (left._j != right._j)
        {
            return left._j > right._j;
        }

        if (left._k != right._k)
        {
            return left._k > right._k;
        }

        return false;
    }

    public static bool GreaterThanLongGenericOut(in GuidBench left, in GuidBench right)
    {
        GetMostSignificantDifference(in left, in right, out uint a, out uint b);
        return a > b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void GetMostSignificantDifference(in GuidBench left, in GuidBench right, out uint leftDifference, out uint rightDifference)
    {
        if (left._a != right._a)
        {
            leftDifference = (uint)left._a;
            rightDifference = (uint)right._a;
            return;
        }

        if (left._b != right._b)
        {
            leftDifference = (uint)left._b;
            rightDifference = (uint)right._b;
            return;
        }

        if (left._c != right._c)
        {
            leftDifference = (uint)left._c;
            rightDifference = (uint)right._c;
            return;
        }

        if (BitConverter.IsLittleEndian)
        {
            // Only compare ordered-bytes to avoid expensive Shuffle operations.
            ulong leftTail = Unsafe.As<byte, ulong>(ref Unsafe.AsRef(in left._d));
            ulong rightTail = Unsafe.As<byte, ulong>(ref Unsafe.AsRef(in right._d));

            ulong diff = leftTail ^ rightTail;
            if (diff == 0)
            {
                leftDifference = 0;
                rightDifference = 0;
                return;
            }

            int firstDifferentBit = BitOperations.TrailingZeroCount(diff);
            // The comparisons are per byte, so align our comparison to a byte boundary.
            firstDifferentBit -= firstDifferentBit % 8;
            ulong leftDifferentByte = (leftTail >> firstDifferentBit) & 0xFF;
            ulong rightDifferentByte = (rightTail >> firstDifferentBit) & 0xFF;

            leftDifference = (uint)leftDifferentByte;
            rightDifference = (uint)rightDifferentByte;
            return;
        }

        if (left._d != right._d)
        {
            leftDifference = left._d;
            rightDifference = right._d;
            return;
        }

        if (left._e != right._e)
        {
            leftDifference = left._e;
            rightDifference = right._e;
            return;
        }

        if (left._f != right._f)
        {
            leftDifference = left._f;
            rightDifference = right._f;
            return;
        }

        if (left._g != right._g)
        {
            leftDifference = left._g;
            rightDifference = right._g;
            return;
        }

        if (left._h != right._h)
        {
            leftDifference = left._h;
            rightDifference = right._h;
            return;
        }

        if (left._i != right._i)
        {
            leftDifference = left._i;
            rightDifference = right._i;
            return;
        }

        if (left._j != right._j)
        {
            leftDifference = left._j;
            rightDifference = right._j;
            return;
        }

        if (left._k != right._k)
        {
            leftDifference = left._k;
            rightDifference = right._k;
            return;
        }

        leftDifference = 0;
        rightDifference = 0;
    }

    public static bool GreaterThanAsVector128(in GuidBench left, in GuidBench right)
    {
        // Only compare ordered-bytes to avoid expensive Shuffle operations on little endian machines.
        Vector128<byte> vecLeft = AsVector128(left);
        Vector128<byte> vecRight = AsVector128(right);
        Vector128<byte> cmp = Vector128.Equals(vecLeft, vecRight);
        uint cmpMask = cmp.ExtractMostSignificantBits();
        if (cmpMask == 0xFFFF)
        {
            return false;
        }

        int firstDiffBit = BitOperations.TrailingZeroCount(~cmpMask);
        return vecLeft[firstDiffBit] > vecRight[firstDiffBit];
    }

    public override string ToString()
    {
        return $"{_a:X8}-{_b:X4}-{_c:X4}-{_d:X2}{_e:X2}-{_f:X2}{_g:X2}{_h:X2}{_i:X2}{_j:X2}{_k:X2}";
    }
}

[TestFixture]
public class GuidTest
{
    [Test]
    public void TestCompareEquality()
    {
        Func<GuidBench, GuidBench, bool>[] testMethods = [
            (a, b) => GuidBench.GreaterThanVector128Sse2(a, b),
            (a, b) => GuidBench.GreaterThanVector128(a, b),
            (a, b) => GuidBench.GreaterThanMax(a, b),
            (a, b) => GuidBench.GreaterThanVector128Shuffle(a, b),
            (a, b) => GuidBench.GreaterThan256Avx2(a, b),
            (a, b) => GuidBench.GreaterThanLong(a, b),
            (a, b) => GuidBench.GreaterThanLongStruct(a, b),
            (a, b) => GuidBench.GreaterThanVector64(a, b),
            (a, b) => GuidBench.GreaterThanLongGeneric(a, b),
            (a, b) => GuidBench.GreaterThanLongGenericOut(a, b),
            (a, b) => GuidBench.GreaterThanLongCompareTo(a, b),
            (a, b) => GuidBench.GreaterThanInt128(a, b),
            (a, b) => GuidBench.GreaterThanInt128Optimized(a, b),
            (a, b) => GuidBench.GreaterThanAsVector128(a, b),
        ];

        Guid actualGuid = Guid.Parse("00001009-0009-0008-0706-050403020100");
        Guid actualGuid2 = Guid.Parse("00001009-0009-0008-0706-050403000103");
        GuidBench benchGuid = new GuidBench(actualGuid);
        GuidBench benchGuid2 = new GuidBench(actualGuid2);
        Assert.That(actualGuid.ToString(), Is.EqualTo(benchGuid.ToString()));

        bool zero = GuidBench.GreaterThanNormal(benchGuid, benchGuid2);
        bool one = GuidBench.GreaterThanVector128Sse2(benchGuid, benchGuid2);
        bool two = GuidBench.GreaterThanVector128(benchGuid, benchGuid2);
        bool three = GuidBench.GreaterThanSse2IndirectTzcnt(benchGuid, benchGuid2);

        GuidBench.GreaterThanLong(GuidBench.GuidB6, GuidBench.GuidB7);

        foreach (Func<GuidBench, GuidBench, bool> testMethod in testMethods)
        {
            int i = 0;
            foreach (GuidBench a in GuidBench.TestGuids)
            {
                foreach (GuidBench b in GuidBench.TestGuids)
                {
                    bool expected = GuidBench.GreaterThanNormal(a, b);
                    Console.WriteLine($"{i++}: {a} > {b} = {expected}");
                    bool actual = testMethod(a, b);
                    if (expected != actual)
                    {
                        // Debug break and rerun for debugging.
                        Debugger.Break();
                        testMethod(a, b);
                    }
                    Assert.That(expected, Is.EqualTo(actual));
                }
            }
        }
    }
}

public class GuidCompareToBenchmarks
{
    private readonly static GuidBench[] _guids = GuidBench.TestGuids;

    [Benchmark]
    public void CompareToNormal()
    {
        GuidBench[] guids = _guids;
        int forcedSideEffect = 0;
        foreach (GuidBench a in guids)
        {
            foreach (GuidBench b in guids)
            {
                if (a.CompareToNormal(b) == 0) ++forcedSideEffect;
            }
        }

        if (forcedSideEffect == 0) throw new Exception();
    }

    [Benchmark]
    public void CompareToLong()
    {
        GuidBench[] guids = _guids;
        int forcedSideEffect = 0;
        foreach (GuidBench a in guids)
        {
            foreach (GuidBench b in guids)
            {
                if (a.CompareToLong(b) == 0) ++forcedSideEffect;
            }
        }

        if (forcedSideEffect == 0) throw new Exception();
    }

    [Benchmark]
    public void CompareToLongGeneric()
    {
        GuidBench[] guids = _guids;
        int forcedSideEffect = 0;
        foreach (GuidBench a in guids)
        {
            foreach (GuidBench b in guids)
            {
                if (a.CompareToLongGeneric(b) == 0) ++forcedSideEffect;
            }
        }

        if (forcedSideEffect == 0) throw new Exception();
    }

    [Benchmark]
    public void CompareToLongInlined()
    {
        GuidBench[] guids = _guids;
        int forcedSideEffect = 0;
        foreach (GuidBench a in guids)
        {
            foreach (GuidBench b in guids)
            {
                if (a.CompareToLongInlined(b) == 0) ++forcedSideEffect;
            }
        }

        if (forcedSideEffect == 0) throw new Exception();
    }
}

public class GuidGreaterThanBenchmarks
{
    private readonly static GuidBench[] _guids = GuidBench.TestGuids;

    [Benchmark]
    public void GreaterThanNormal()
    {
        GuidBench[] guids = _guids;
        int forcedSideEffect = 0;
        foreach (GuidBench a in guids)
        {
            foreach (GuidBench b in guids)
            {
                if (GuidBench.GreaterThanNormal(a, b)) ++forcedSideEffect;
            }
        }

        if (forcedSideEffect == 0) throw new Exception();
    }

    [Benchmark]
    public void GreaterThanVector128()
    {
        GuidBench[] guids = _guids;
        int forcedSideEffect = 0;
        foreach (GuidBench a in guids)
        {
            foreach (GuidBench b in guids)
            {
                if (GuidBench.GreaterThanVector128(a, b)) ++forcedSideEffect;
            }
        }

        if (forcedSideEffect == 0) throw new Exception();
    }

    [Benchmark]
    public void GreaterThanLongGenericOut()
    {
        GuidBench[] guids = _guids;
        int forcedSideEffect = 0;
        foreach (GuidBench a in guids)
        {
            foreach (GuidBench b in guids)
            {
                if (GuidBench.GreaterThanLongGenericOut(a, b)) ++forcedSideEffect;
            }
        }

        if (forcedSideEffect == 0) throw new Exception();
    }

    [Benchmark]
    public void GreaterThanInt128()
    {
        GuidBench[] guids = _guids;
        int forcedSideEffect = 0;
        foreach (GuidBench a in guids)
        {
            foreach (GuidBench b in guids)
            {
                if (GuidBench.GreaterThanInt128(a, b)) ++forcedSideEffect;
            }
        }

        if (forcedSideEffect == 0) throw new Exception();
    }

    [Benchmark]
    public void GreaterThanInt128Optimized()
    {
        GuidBench[] guids = _guids;
        int forcedSideEffect = 0;
        foreach (GuidBench a in guids)
        {
            foreach (GuidBench b in guids)
            {
                if (GuidBench.GreaterThanInt128Optimized(a, b)) ++forcedSideEffect;
            }
        }

        if (forcedSideEffect == 0) throw new Exception();
    }

    [Benchmark]
    public void GreaterThanAsVector128()
    {
        GuidBench[] guids = _guids;
        int forcedSideEffect = 0;
        foreach (GuidBench a in guids)
        {
            foreach (GuidBench b in guids)
            {
                if (GuidBench.GreaterThanAsVector128(a, b)) ++forcedSideEffect;
            }
        }

        if (forcedSideEffect == 0) throw new Exception();
    }
}
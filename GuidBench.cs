using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Attributes;
using NUnit.Framework;
using NUnit.Framework.Internal;

internal unsafe readonly struct GuidBench
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

    private static readonly Vector128<byte> _littleEndianShuffleMask = Vector128.Create<byte>(
        [3, 2, 1, 0, 5, 4, 7, 6, 8, 9, 10, 11, 12, 13, 14, 15]);

    public static bool GreaterThanMax(in GuidBench left, in GuidBench right)
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

        if (Vector128.IsHardwareAccelerated)
        {
            Vector128<byte> a = Vector128.LoadUnsafe(ref Unsafe.As<GuidBench, byte>(ref Unsafe.AsRef(in left)));
            Vector128<byte> b = Vector128.LoadUnsafe(ref Unsafe.As<GuidBench, byte>(ref Unsafe.AsRef(in right)));

            Vector128<byte> max = Vector128.Max(a, b);
            Vector128<byte> equalsMaxA = Vector128.Equals(max, a);
            Vector128<byte> equalsMaxB = Vector128.Equals(max, b);
            Vector128<byte> compared = Vector128.Equals(equalsMaxA, equalsMaxB);

            uint maskMaxA = equalsMaxA.ExtractMostSignificantBits();
            uint maskMaxB = equalsMaxB.ExtractMostSignificantBits();
            uint maskMask = compared.ExtractMostSignificantBits();
            uint maskedA = maskMaxA ^ maskMask;
            uint maskedB = maskMaxB ^ maskMask;
            int zerosA = BitOperations.TrailingZeroCount(maskedA);
            int zerosB = BitOperations.TrailingZeroCount(maskedB);

            bool result = zerosA < zerosB;
            return result;
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

    public static bool GreaterThanMaxSse2(in GuidBench left, in GuidBench right)
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

        if (Sse2.IsSupported)
        {
            Vector128<byte> a = Vector128.LoadUnsafe(ref Unsafe.As<GuidBench, byte>(ref Unsafe.AsRef(in left)));
            Vector128<byte> b = Vector128.LoadUnsafe(ref Unsafe.As<GuidBench, byte>(ref Unsafe.AsRef(in right)));

            Vector128<byte> max = Sse2.Max(a, b);
            Vector128<byte> equalsMaxA = Sse2.CompareEqual(max, a);
            Vector128<byte> equalsMaxB = Sse2.CompareEqual(max, b);
            Vector128<byte> compared = Sse2.CompareEqual(equalsMaxA, equalsMaxB);

            uint maskMaxA = (uint)Sse2.MoveMask(equalsMaxA);
            uint maskMaxB = (uint)Sse2.MoveMask(equalsMaxB);
            uint maskMask = (uint)Sse2.MoveMask(compared);
            uint maskedA = maskMaxA ^ maskMask;
            uint maskedB = maskMaxB ^ maskMask;
            int zerosA = BitOperations.TrailingZeroCount(maskedA);
            int zerosB = BitOperations.TrailingZeroCount(maskedB);

            bool result = zerosA < zerosB;
            return result;
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

    public static bool GreaterThanVector128Shuffle(in GuidBench left, in GuidBench right)
    {
        if (Vector128.IsHardwareAccelerated)
        {
            Vector128<byte> a = Vector128.LoadUnsafe(ref Unsafe.As<GuidBench, byte>(ref Unsafe.AsRef(in left)));
            Vector128<byte> b = Vector128.LoadUnsafe(ref Unsafe.As<GuidBench, byte>(ref Unsafe.AsRef(in right)));
            if (BitConverter.IsLittleEndian)
            {
                Vector128<byte> shuffleMask = _littleEndianShuffleMask;
                a = Vector128.Shuffle(a, shuffleMask);
                b = Vector128.Shuffle(b, shuffleMask);
            }
            Vector128<byte> cmp = Vector128.Equals(a, b);
            uint mask = cmp.ExtractMostSignificantBits();
            if (mask == 0xFFFF)
            {
                return false;
            }

            int firstDiffBit = BitOperations.TrailingZeroCount(~mask);
            return a[firstDiffBit] > b[firstDiffBit];
        }

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

    private static readonly Vector256<byte> _littleEndianShuffleMask256 = Vector256.Create<byte>([
        3, 2, 1, 0, 5, 4, 7, 6, 8, 9, 10, 11, 12, 13, 14, 15,
        3+16, 2+16, 1+16, 0+16, 5+16, 4+16, 7+16, 6+16, 8+16, 9+16, 10+16, 11+16, 12+16, 13+16, 14+16, 15+16
    ]);

    public static bool GreaterThan256Avx2(in GuidBench left, in GuidBench right)
    {
        if (Avx2.IsSupported)
        {
            Vector128<byte> a = Vector128.LoadUnsafe(ref Unsafe.As<GuidBench, byte>(ref Unsafe.AsRef(in left)));
            Vector128<byte> b = Vector128.LoadUnsafe(ref Unsafe.As<GuidBench, byte>(ref Unsafe.AsRef(in right)));

            if (BitConverter.IsLittleEndian)
            {
                Vector256<byte> wideVector = Avx.InsertVector128(Vector256<byte>.Zero, a, 0);
                wideVector = Avx2.InsertVector128(wideVector, b, 1);
                wideVector = Avx2.Shuffle(wideVector, _littleEndianShuffleMask256);
                Vector256<byte> swapped = Avx2.Permute2x128(wideVector, wideVector, 0x01);
                Vector256<byte> wideCmp = Avx2.CompareEqual(wideVector, swapped);
                int wideMask = ~Avx2.MoveMask(wideCmp);
                if (wideMask == 0) return false;
                int firstDiffBitWide = BitOperations.TrailingZeroCount(wideMask);
                // Console.WriteLine($"a:                {a}");
                // Console.WriteLine($"b:                {b}");
                // Console.WriteLine($"wideVector:       {wideVector}");
                // Console.WriteLine($"swapped:          {swapped}");
                // Console.WriteLine($"comparisonResult: {comparisonResult}");
                // Console.WriteLine($"wideMask:         {wideMask}");
                // Console.WriteLine($"firstDiffBitWide: {firstDiffBitWide}");
                return wideVector[firstDiffBitWide] > wideVector[firstDiffBitWide + 16];
            }
            Vector128<byte> cmp = Sse2.CompareEqual(a, b);
            int mask = Sse2.MoveMask(cmp);
            if (mask == 0xFFFF) return false;
            int firstDiffBit = BitOperations.TrailingZeroCount(~mask);
            return a[firstDiffBit] > b[firstDiffBit];
        }

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

    public static bool GreaterThanVector128Sse2(in GuidBench left, in GuidBench right)
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

        if (Sse2.IsSupported)
        {
            Vector128<byte> vecLeft = Vector128.LoadUnsafe(ref Unsafe.As<GuidBench, byte>(ref Unsafe.AsRef(in left)));
            Vector128<byte> vecRight = Vector128.LoadUnsafe(ref Unsafe.As<GuidBench, byte>(ref Unsafe.AsRef(in right)));
            Vector128<byte> cmp = Sse2.CompareEqual(vecLeft, vecRight);
            uint cmpMask = (uint)Sse2.MoveMask(cmp);
            if (cmpMask == 0xFFFF)
            {
                return false;
            }

            int firstDiffBit = BitOperations.TrailingZeroCount(~cmpMask);
            return vecLeft[firstDiffBit] > vecRight[firstDiffBit];
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

    public static bool GreaterThanSse2IndirectTzcnt(in GuidBench left, in GuidBench right)
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

        if (Sse2.IsSupported)
        {
            Vector128<byte> vecLeft = Vector128.LoadUnsafe(ref Unsafe.As<GuidBench, byte>(ref Unsafe.AsRef(in left)));
            Vector128<byte> vecRight = Vector128.LoadUnsafe(ref Unsafe.As<GuidBench, byte>(ref Unsafe.AsRef(in right)));
            Vector128<byte> cmp = Sse2.CompareEqual(vecLeft, vecRight);
            int cmpMask = Sse2.MoveMask(cmp);
            if (cmpMask == 0xFFFF)
            {
                return false;
            }

            int firstDiffBit = BitOperations.TrailingZeroCount(~cmpMask);
            return vecLeft[firstDiffBit] > vecRight[firstDiffBit];
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

    public static bool GreaterThanVector128(in GuidBench left, in GuidBench right)
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

        if (Vector128.IsHardwareAccelerated)
        {
            // Only compare ordered-bytes to avoid expensive Shuffle operations on little endian machines.
            Vector128<byte> vecLeft = Vector128.LoadUnsafe(ref Unsafe.As<GuidBench, byte>(ref Unsafe.AsRef(in left)));
            Vector128<byte> vecRight = Vector128.LoadUnsafe(ref Unsafe.As<GuidBench, byte>(ref Unsafe.AsRef(in right)));
            Vector128<byte> cmp = Vector128.Equals(vecLeft, vecRight);
            uint cmpMask = cmp.ExtractMostSignificantBits();
            if (cmpMask == 0xFFFF)
            {
                return false;
            }

            int firstDiffBit = BitOperations.TrailingZeroCount(~cmpMask);
            return vecLeft[firstDiffBit] > vecRight[firstDiffBit];
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
            // Align to byte boundaries.
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

    public static bool GreaterThanLongGeneric(in GuidBench left, in GuidBench right)
    {
        (uint a, uint b) = GetDifferentTailBytes(in left, in right);
        return a > b;
    }

    private static (uint left, uint right) GetDifferentTailBytes(in GuidBench left, in GuidBench right)
    {
        if (left._a != right._a)
        {
            return ((uint)left._a, (uint)right._a);
        }

        if (left._b != right._b)
        {
            return ((uint)left._b, (uint)right._b);
        }

        if (left._c != right._c)
        {
            return ((uint)left._c, (uint)right._c);
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
                return (0, 0);
            }

            int firstDifferentBit = BitOperations.TrailingZeroCount(diff);
            // Align to byte boundaries.
            firstDifferentBit -= firstDifferentBit % 8;
            ulong leftDifferentByte = (leftTail >> firstDifferentBit) & 0xFF;
            ulong rightDifferentByte = (rightTail >> firstDifferentBit) & 0xFF;

            return ((byte)leftDifferentByte, (byte)rightDifferentByte);
        }

        if (left._d != right._d)
        {
            return (left._d, right._d);
        }

        if (left._e != right._e)
        {
            return (left._e, right._e);
        }

        if (left._f != right._f)
        {
            return (left._f, right._f);
        }

        if (left._g != right._g)
        {
            return (left._g, right._g);
        }

        if (left._h != right._h)
        {
            return (left._h, right._h);
        }

        if (left._i != right._i)
        {
            return (left._i, right._i);
        }

        if (left._j != right._j)
        {
            return (left._j, right._j);
        }

        if (left._k != right._k)
        {
            return (left._k, right._k);
        }

        return (0, 0);
    }

    public static bool GreaterThanLongGenericOut(in GuidBench left, in GuidBench right)
    {
        GetMostSignificantDifference(in left, in right, out uint a, out uint b);
        return a > b;
    }

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
            // Use ulong to prevent a non-inlined call to TrailingZeroCount for `tzcnt`, which happens with long.
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
            // Align to byte boundaries.
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

    [StructLayout(LayoutKind.Explicit)]
    private struct GuidTailStruct
    {
        [FieldOffset(0)]
        public ulong _abc;
        [FieldOffset(0)]
        public uint _a;
        [FieldOffset(4)]
        public ushort _b;
        [FieldOffset(6)]
        public ushort _c;
        [FieldOffset(8)]
        public ulong _defghijk;
    }

    public static bool GreaterThanLongStruct(in GuidBench left, in GuidBench right)
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

        if (BitConverter.IsLittleEndian)
        {
            // Only compare ordered-bytes to avoid expensive Shuffle operations.
            // Use ulong to prevent a non-inlined call to TrailingZeroCount for `tzcnt`, which happens with long.
            ulong leftTail = Unsafe.As<GuidBench, GuidTailStruct>(ref Unsafe.AsRef(in left))._defghijk;
            ulong rightTail = Unsafe.As<GuidBench, GuidTailStruct>(ref Unsafe.AsRef(in right))._defghijk;

            ulong diff = leftTail ^ rightTail;
            if (diff == 0)
            {
                return false;
            }

            int firstDifferentBit = BitOperations.TrailingZeroCount(diff);
            // Align to byte boundaries.
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

        throw new UnreachableException();
    }

    public static bool GreaterThanVector64(in GuidBench left, in GuidBench right)
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

        if (Vector64.IsHardwareAccelerated)
        {
            // Only compare ordered-bytes to avoid expensive Shuffle operations.
            // Use ulong to prevent a non-inlined call to TrailingZeroCount for `tzcnt`, which happens with long.
            ref byte leftBytes = ref Unsafe.As<byte, byte>(ref Unsafe.AsRef(in left._d));
            Vector64<byte> vecLeft = Vector64.LoadUnsafe(ref leftBytes);

            ref byte rightBytes = ref Unsafe.As<byte, byte>(ref Unsafe.AsRef(in right._d));
            Vector64<byte> vecRight = Vector64.LoadUnsafe(ref rightBytes);

            Vector64<byte> cmp = Vector64.Equals(vecLeft, vecRight);
            uint mask = cmp.ExtractMostSignificantBits();
            if (mask == 0xFF)
            {
                return false;
            }

            int firstDiffBit = BitOperations.TrailingZeroCount(~mask);
            return vecLeft[firstDiffBit] > vecRight[firstDiffBit];
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

    // Does not work.
    public static bool LongLongGreaterThan(in GuidBench left, in GuidBench right)
    {
        Vector128<byte> aa = Vector128.LoadUnsafe(ref Unsafe.As<GuidBench, byte>(ref Unsafe.AsRef(in left)));
        Vector128<byte> bb = Vector128.LoadUnsafe(ref Unsafe.As<GuidBench, byte>(ref Unsafe.AsRef(in right)));

        Vector128<byte> a = Ssse3.Shuffle(aa, _littleEndianShuffleMask);
        Vector128<byte> b = Ssse3.Shuffle(bb, _littleEndianShuffleMask);

        UInt128 ax = Unsafe.As<Vector128<byte>, UInt128>(ref a);
        UInt128 bx = Unsafe.As<Vector128<byte>, UInt128>(ref b);

        Console.WriteLine($"aa: {aa}");
        Console.WriteLine($"bb: {bb}");
        Console.WriteLine($"a:  {a}");
        Console.WriteLine($"b:  {b}");
        Console.WriteLine($"ax: {ax:X32}");
        Console.WriteLine($"bx: {bx:X32}");

        return ax > bx;
    }

    // Does not work.
    public static bool Avx2GreaterThan(in GuidBench left, in GuidBench right)
    {
        Vector128<byte> a = Vector128.LoadUnsafe(ref Unsafe.As<GuidBench, byte>(ref Unsafe.AsRef(in left)));
        Vector128<byte> b = Vector128.LoadUnsafe(ref Unsafe.As<GuidBench, byte>(ref Unsafe.AsRef(in right)));

        Vector256<short> aWide = Avx2.ConvertToVector256Int16(a);
        Vector256<short> bWide = Avx2.ConvertToVector256Int16(b);

        Vector256<short> res = Avx2.CompareGreaterThan(aWide, bWide);
        Vector256<short> res2 = Avx2.CompareGreaterThan(bWide, aWide);
        Vector256<short> eq = Avx2.CompareEqual(aWide, bWide);
        Vector256<short> resultx = Avx2.Xor(res, eq);
        Vector256<short> resultx2 = Avx2.Xor(res2, eq);
        Vector256<byte> asdasd = resultx.AsByte();
        uint finalMask = (uint)Avx2.MoveMask(resultx.AsByte());
        uint finalMask2 = (uint)Avx2.MoveMask(resultx2.AsByte());
        Console.WriteLine($"finalMask : {ToBinary32(finalMask, 32)}");
        Console.WriteLine($"finalMask2: {ToBinary32(finalMask2, 32)}");

        int zerosA = BitOperations.TrailingZeroCount(finalMask);
        int zerosB = BitOperations.TrailingZeroCount(finalMask2);

        bool result = zerosA < zerosB;

        if (result != GreaterThanNormal(left, right))
        {
            Debugger.Break();
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetResultGeneric<T>(T me, T them)
        where T : INumber<T>
    {
        return me < them ? -1 : 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetResult(uint me, uint them) => me < them ? -1 : 1;

    public int CompareToNormal(GuidBench value)
    {
        if (value._a != _a)
        {
            return GetResult((uint)_a, (uint)value._a);
        }

        if (value._b != _b)
        {
            return GetResult((uint)_b, (uint)value._b);
        }

        if (value._c != _c)
        {
            return GetResult((uint)_c, (uint)value._c);
        }

        if (value._d != _d)
        {
            return GetResult(_d, value._d);
        }

        if (value._e != _e)
        {
            return GetResult(_e, value._e);
        }

        if (value._f != _f)
        {
            return GetResult(_f, value._f);
        }

        if (value._g != _g)
        {
            return GetResult(_g, value._g);
        }

        if (value._h != _h)
        {
            return GetResult(_h, value._h);
        }

        if (value._i != _i)
        {
            return GetResult(_i, value._i);
        }

        if (value._j != _j)
        {
            return GetResult(_j, value._j);
        }

        if (value._k != _k)
        {
            return GetResult(_k, value._k);
        }

        return 0;
    }

    public int CompareToLongGeneric(GuidBench value)
    {
        if (value._a != _a)
        {
            return GetResultGeneric(_a, value._a);
        }

        if (value._b != _b)
        {
            return GetResultGeneric(_b, value._b);
        }

        if (value._c != _c)
        {
            return GetResultGeneric(_c, value._c);
        }

        if (BitConverter.IsLittleEndian)
        {
            // Only compare ordered-bytes to avoid expensive Shuffle operations.
            // Use ulong to prevent a non-inlined call to TrailingZeroCount for `tzcnt`, which happens with long.
            ulong leftTail = Unsafe.As<byte, ulong>(ref Unsafe.AsRef(in _d));
            ulong rightTail = Unsafe.As<byte, ulong>(ref Unsafe.AsRef(in value._d));

            ulong diff = leftTail ^ rightTail;
            if (diff == 0)
            {
                return 0;
            }

            int firstDifferentBit = BitOperations.TrailingZeroCount(diff);
            // Align to byte boundaries.
            firstDifferentBit -= firstDifferentBit % 8;
            ulong leftDifferentByte = (leftTail >> firstDifferentBit) & 0xFF;
            ulong rightDifferentByte = (rightTail >> firstDifferentBit) & 0xFF;

            return GetResultGeneric(leftDifferentByte, rightDifferentByte);
        }

        if (value._d != _d)
        {
            return GetResultGeneric(_d, value._d);
        }

        if (value._e != _e)
        {
            return GetResultGeneric(_e, value._e);
        }

        if (value._f != _f)
        {
            return GetResultGeneric(_f, value._f);
        }

        if (value._g != _g)
        {
            return GetResultGeneric(_g, value._g);
        }

        if (value._h != _h)
        {
            return GetResultGeneric(_h, value._h);
        }

        if (value._i != _i)
        {
            return GetResultGeneric(_i, value._i);
        }

        if (value._j != _j)
        {
            return GetResultGeneric(_j, value._j);
        }

        if (value._k != _k)
        {
            return GetResultGeneric(_k, value._k);
        }

        return 0;
    }

    public int CompareToLong(GuidBench value)
    {
        if (value._a != _a)
        {
            return GetResult((uint)_a, (uint)value._a);
        }

        if (value._b != _b)
        {
            return GetResult((uint)_b, (uint)value._b);
        }

        if (value._c != _c)
        {
            return GetResult((uint)_c, (uint)value._c);
        }

        if (BitConverter.IsLittleEndian)
        {
            // Only compare ordered-bytes to avoid expensive Shuffle operations.
            // Use ulong to prevent a non-inlined call to TrailingZeroCount for `tzcnt`, which happens with long.
            ulong leftTail = Unsafe.As<byte, ulong>(ref Unsafe.AsRef(in _d));
            ulong rightTail = Unsafe.As<byte, ulong>(ref Unsafe.AsRef(in value._d));

            ulong diff = leftTail ^ rightTail;
            if (diff == 0)
            {
                return 0;
            }

            int firstDifferentBit = BitOperations.TrailingZeroCount(diff);
            // Align to byte boundaries.
            firstDifferentBit -= firstDifferentBit % 8;
            ulong leftDifferentByte = (leftTail >> firstDifferentBit) & 0xFF;
            ulong rightDifferentByte = (rightTail >> firstDifferentBit) & 0xFF;

            return GetResult((uint)leftDifferentByte, (uint)rightDifferentByte);
        }

        if (value._d != _d)
        {
            return GetResult(_d, value._d);
        }

        if (value._e != _e)
        {
            return GetResult(_e, value._e);
        }

        if (value._f != _f)
        {
            return GetResult(_f, value._f);
        }

        if (value._g != _g)
        {
            return GetResult(_g, value._g);
        }

        if (value._h != _h)
        {
            return GetResult(_h, value._h);
        }

        if (value._i != _i)
        {
            return GetResult(_i, value._i);
        }

        if (value._j != _j)
        {
            return GetResult(_j, value._j);
        }

        if (value._k != _k)
        {
            return GetResult(_k, value._k);
        }

        return 0;
    }

    public int CompareToLongInlined(GuidBench value)
    {
        if (value._a != _a)
        {
            return _a < value._a ? -1 : 1;
        }

        if (value._b != _b)
        {
            return _b < value._b ? -1 : 1;
        }

        if (value._c != _c)
        {
            return _c < value._c ? -1 : 1;
        }

        if (BitConverter.IsLittleEndian)
        {
            // Only compare ordered-bytes to avoid expensive Shuffle operations.
            // Use ulong to prevent a non-inlined call to TrailingZeroCount for `tzcnt`, which happens with long.
            ulong leftTail = Unsafe.As<byte, ulong>(ref Unsafe.AsRef(in _d));
            ulong rightTail = Unsafe.As<byte, ulong>(ref Unsafe.AsRef(in value._d));

            ulong diff = leftTail ^ rightTail;
            if (diff == 0)
            {
                return 0;
            }

            int firstDifferentBit = BitOperations.TrailingZeroCount(diff);
            // The comparison is per byte, so align our comparison to a byte boundary.
            firstDifferentBit -= firstDifferentBit % 8;
            ulong leftDifferentByte = (leftTail >> firstDifferentBit) & 0xFF;
            ulong rightDifferentByte = (rightTail >> firstDifferentBit) & 0xFF;

            return leftDifferentByte < rightDifferentByte ? -1 : 1;
        }

        if (value._d != _d)
        {
            return _d < value._d ? -1 : 1;
        }

        if (value._e != _e)
        {
            return _e < value._e ? -1 : 1;
        }

        if (value._f != _f)
        {
            return _f < value._f ? -1 : 1;
        }

        if (value._g != _g)
        {
            return _g < value._g ? -1 : 1;
        }

        if (value._h != _h)
        {
            return _h < value._h ? -1 : 1;
        }

        if (value._i != _i)
        {
            return _i < value._i ? -1 : 1;
        }

        if (value._j != _j)
        {
            return _j < value._j ? -1 : 1;
        }

        if (value._k != _k)
        {
            return _k < value._k ? -1 : 1;
        }

        return 0;
    }

    public static bool GreaterThanLongCompareTo(in GuidBench left, in GuidBench right)
    {
        return left.CompareToLongInlined(right) == 1;
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
    public void GreaterThanMax()
    {
        GuidBench[] guids = _guids;
        int forcedSideEffect = 0;
        foreach (GuidBench a in guids)
        {
            foreach (GuidBench b in guids)
            {
                if (GuidBench.GreaterThanMax(a, b)) ++forcedSideEffect;
            }
        }

        if (forcedSideEffect == 0) throw new Exception();
    }

    [Benchmark]
    public void GreaterThanMaxSse2()
    {
        if (!Sse2.IsSupported) return;

        GuidBench[] guids = _guids;
        int forcedSideEffect = 0;
        foreach (GuidBench a in guids)
        {
            foreach (GuidBench b in guids)
            {
                if (GuidBench.GreaterThanMaxSse2(a, b)) ++forcedSideEffect;
            }
        }

        if (forcedSideEffect == 0) throw new Exception();
    }

    [Benchmark]
    public void GreaterThanVector128Shuffle()
    {
        GuidBench[] guids = _guids;
        int forcedSideEffect = 0;
        foreach (GuidBench a in guids)
        {
            foreach (GuidBench b in guids)
            {
                if (GuidBench.GreaterThanVector128Shuffle(a, b)) ++forcedSideEffect;
            }
        }

        if (forcedSideEffect == 0) throw new Exception();
    }

    [Benchmark]
    public void GreaterThan256Avx2()
    {
        GuidBench[] guids = _guids;
        int forcedSideEffect = 0;
        foreach (GuidBench a in guids)
        {
            foreach (GuidBench b in guids)
            {
                if (GuidBench.GreaterThan256Avx2(a, b)) ++forcedSideEffect;
            }
        }

        if (forcedSideEffect == 0) throw new Exception();
    }

    [Benchmark]
    public void GreaterThanVector128Sse2()
    {
        GuidBench[] guids = _guids;
        int forcedSideEffect = 0;
        foreach (GuidBench a in guids)
        {
            foreach (GuidBench b in guids)
            {
                if (GuidBench.GreaterThanVector128Sse2(a, b)) ++forcedSideEffect;
            }
        }

        if (forcedSideEffect == 0) throw new Exception();
    }

    // [Benchmark]
    public void GreaterThanSse2IndirectTzcnt()
    {
        GuidBench[] guids = _guids;
        int forcedSideEffect = 0;
        foreach (GuidBench a in guids)
        {
            foreach (GuidBench b in guids)
            {
                if (GuidBench.GreaterThanSse2IndirectTzcnt(a, b)) ++forcedSideEffect;
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
    public void GreaterThanLong()
    {
        GuidBench[] guids = _guids;
        int forcedSideEffect = 0;
        foreach (GuidBench a in guids)
        {
            foreach (GuidBench b in guids)
            {
                if (GuidBench.GreaterThanLong(a, b)) ++forcedSideEffect;
            }
        }

        if (forcedSideEffect == 0) throw new Exception();
    }

    [Benchmark]
    public void GreaterThanLongStruct()
    {
        GuidBench[] guids = _guids;
        int forcedSideEffect = 0;
        foreach (GuidBench a in guids)
        {
            foreach (GuidBench b in guids)
            {
                if (GuidBench.GreaterThanLongStruct(a, b)) ++forcedSideEffect;
            }
        }

        if (forcedSideEffect == 0) throw new Exception();
    }

    [Benchmark]
    public void GreaterThanVector64()
    {
        GuidBench[] guids = _guids;
        int forcedSideEffect = 0;
        foreach (GuidBench a in guids)
        {
            foreach (GuidBench b in guids)
            {
                if (GuidBench.GreaterThanVector64(a, b)) ++forcedSideEffect;
            }
        }

        if (forcedSideEffect == 0) throw new Exception();
    }

    // [Benchmark]
    public void GreaterThanLongGeneric()
    {
        GuidBench[] guids = _guids;
        int forcedSideEffect = 0;
        foreach (GuidBench a in guids)
        {
            foreach (GuidBench b in guids)
            {
                if (GuidBench.GreaterThanLongGeneric(a, b)) ++forcedSideEffect;
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
    public void GreaterThanLongCompareTo()
    {
        GuidBench[] guids = _guids;
        int forcedSideEffect = 0;
        foreach (GuidBench a in guids)
        {
            foreach (GuidBench b in guids)
            {
                if (GuidBench.GreaterThanLongCompareTo(a, b)) ++forcedSideEffect;
            }
        }

        if (forcedSideEffect == 0) throw new Exception();
    }
}
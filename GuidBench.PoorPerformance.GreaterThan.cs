using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;

namespace Benchmarks;

internal unsafe readonly partial struct GuidBench
{
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
            // The comparisons are per byte, so align our comparison to a byte boundary.
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

}


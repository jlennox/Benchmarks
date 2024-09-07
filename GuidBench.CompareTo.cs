using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Benchmarks;

internal unsafe readonly partial struct GuidBench
{
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
            // The comparisons are per byte, so align our comparison to a byte boundary.
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
            // The comparisons are per byte, so align our comparison to a byte boundary.
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
            // The comparisons are per byte, so align our comparison to a byte boundary.
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
}
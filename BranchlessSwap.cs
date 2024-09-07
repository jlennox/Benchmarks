using System.Numerics;
using System.Runtime.Intrinsics;
using BenchmarkDotNet.Attributes;
using NUnit.Framework;

namespace Benchmarks;

// | Method                      | Mean      | Error     | StdDev    | Median    |
// |---------------------------- |----------:|----------:|----------:|----------:|
// | BranchedFlipNone            | 1.2207 ns | 0.0016 ns | 0.0013 ns | 1.2205 ns |
// | BranchedFlipOne             | 1.2237 ns | 0.0058 ns | 0.0054 ns | 1.2203 ns |
// | BranchedFlipTwo             | 1.2255 ns | 0.0093 ns | 0.0082 ns | 1.2215 ns |
// | BranchlessFlipNone          | 0.0016 ns | 0.0029 ns | 0.0026 ns | 0.0000 ns |
// | BranchlessFlipOne           | 0.0036 ns | 0.0049 ns | 0.0044 ns | 0.0009 ns |
// | BranchlessFlipTwo           | 0.0018 ns | 0.0032 ns | 0.0029 ns | 0.0002 ns |
// | BranchlessSIMDFlipNone      | 3.6049 ns | 0.0018 ns | 0.0017 ns | 3.6049 ns |
// | BranchlessSIMDFlipOne       | 3.6415 ns | 0.0373 ns | 0.0330 ns | 3.6482 ns |
// | BranchlessSIMDFlipTwo       | 3.6059 ns | 0.0020 ns | 0.0017 ns | 3.6059 ns |
// | BranchlessSIMDNoMulFlipNone | 3.6038 ns | 0.0026 ns | 0.0023 ns | 3.6030 ns |
// | BranchlessSIMDNoMulFlipOne  | 3.6204 ns | 0.0207 ns | 0.0183 ns | 3.6189 ns |
// | BranchlessSIMDNoMulFlipTwo  | 3.6037 ns | 0.0019 ns | 0.0015 ns | 3.6036 ns |


[Flags]
public enum DrawingFlags
{
    None = 0,
    FlipHorizontal = 1 << 0,
    FlipVertical = 1 << 1,
    NoTransparency = 1 << 2,
}

public class BranchlessSwap
{
    [Benchmark] public void BranchedFlipNone() => Branched(9, 9, 10, 10, DrawingFlags.None);
    [Benchmark] public void BranchlessFlipNone() => Branchless(9, 9, 10, 10, DrawingFlags.None);
    [Benchmark] public void BranchlessSIMDFlipNone() => BranchlessSIMD(9, 9, 10, 10, DrawingFlags.None);
    [Benchmark] public void BranchlessSIMDNoMulFlipNone() => BranchlessSIMDNoMul(9, 9, 10, 10, DrawingFlags.None);

    [Benchmark] public void BranchedFlipOne() => Branched(9, 9, 10, 10, DrawingFlags.FlipHorizontal);
    [Benchmark] public void BranchlessFlipOne() => Branchless(9, 9, 10, 10, DrawingFlags.FlipHorizontal);
    [Benchmark] public void BranchlessSIMDFlipOne() => BranchlessSIMD(9, 9, 10, 10, DrawingFlags.FlipHorizontal);
    [Benchmark] public void BranchlessSIMDNoMulFlipOne() => BranchlessSIMDNoMul(9, 9, 10, 10, DrawingFlags.FlipHorizontal);

    [Benchmark] public void BranchedFlipTwo() => Branched(9, 9, 10, 10, DrawingFlags.FlipHorizontal | DrawingFlags.FlipVertical);
    [Benchmark] public void BranchlessFlipTwo() => Branchless(9, 9, 10, 10, DrawingFlags.FlipHorizontal | DrawingFlags.FlipVertical);
    [Benchmark] public void BranchlessSIMDFlipTwo() => BranchlessSIMD(9, 9, 10, 10, DrawingFlags.FlipHorizontal | DrawingFlags.FlipVertical);
    [Benchmark] public void BranchlessSIMDNoMulFlipTwo() => BranchlessSIMDNoMul(9, 9, 10, 10, DrawingFlags.FlipHorizontal | DrawingFlags.FlipVertical);

    private static int Branched(int srcx, int srcy, int width, int height, DrawingFlags flags)
    {
        var flipHor = flags.HasFlag(DrawingFlags.FlipHorizontal);
        var flipVert = flags.HasFlag(DrawingFlags.FlipVertical);

        var right = srcx + width;
        var bottom = srcy + height;

        if (flipHor) (srcx, right) = (right, srcx);
        if (flipVert) (srcy, bottom) = (bottom, srcy);

        return srcx + srcy << 1 + right << 2 + bottom << 3;
    }

    private static int Branchless(int srcx, int srcy, int width, int height, DrawingFlags flags)
    {
        var flipHor = -BitOperations.PopCount((uint)(flags & DrawingFlags.FlipHorizontal));
        var flipVert = -BitOperations.PopCount((uint)(flags & DrawingFlags.FlipVertical));

        var right = srcx + width;
        var bottom = srcy + height;

        var tempSrcX = (flipHor & right) | (~flipHor & srcx);
        var tempRight = (flipHor & srcx) | (~flipHor & right);
        srcx = tempSrcX;
        right = tempRight;

        var tempSrcY = (flipVert & bottom) | (~flipVert & srcy);
        var tempBottom = (flipVert & srcy) | (~flipVert & bottom);
        srcy = tempSrcY;
        bottom = tempBottom;

        return srcx + srcy << 1 + right << 2 + bottom << 3;
    }

    private static int BranchlessSIMD(int srcx, int srcy, int width, int height, DrawingFlags flags)
    {
        var flipHor = BitOperations.PopCount((uint)(flags & DrawingFlags.FlipHorizontal));
        var flipVert = BitOperations.PopCount((uint)(flags & DrawingFlags.FlipVertical));

        var right = srcx + width;
        var bottom = srcy + height;

        var rect = Vector128.Create(srcx, srcy, right, bottom);
        var rectinverted = Vector128.Create(right, bottom, srcx, srcy);
        var flip = Vector128.Create(flipHor, flipVert, flipHor, flipVert);
        var flipInverted = flip ^ Vector128.Create(1);
        var flipped = (rect * flipInverted) | (flip * rectinverted);

        return flipped[0] + flipped[1] << 1 + flipped[2] << 2 + flipped[3] << 3;
    }

    private static int BranchlessSIMDNoMul(int srcx, int srcy, int width, int height, DrawingFlags flags)
    {
        unchecked
        {
            var flipHor = (int)(-BitOperations.PopCount((uint)(flags & DrawingFlags.FlipHorizontal)) & 0xFFFFFFFF);
            var flipVert = (int)(-BitOperations.PopCount((uint)(flags & DrawingFlags.FlipVertical)) & 0xFFFFFFFF);

            var right = srcx + width;
            var bottom = srcy + height;

            var rect = Vector128.Create(srcx, srcy, right, bottom);
            var rectinverted = Vector128.Create(right, bottom, srcx, srcy);
            var flip = Vector128.Create(flipHor, flipVert, flipHor, flipVert);
            var flipInverted = flip ^ Vector128.Create((int)0xFFFFFFFF);
            var flipped = (rect & flipInverted) | (flip & rectinverted);

            return flipped[0] + flipped[1] << 1 + flipped[2] << 2 + flipped[3] << 3;
        }
    }

    public static void Test()
    {
        foreach (var flags in new[] {
            DrawingFlags.None,
            DrawingFlags.FlipHorizontal,
            DrawingFlags.FlipVertical,
            DrawingFlags.FlipHorizontal | DrawingFlags.FlipVertical })
        {
            for (var x = 0; x < 2; x++)
            {
                for (var y = 10; y < 12; y++)
                {
                    for (var w = 5; w < 7; w++)
                    {
                        for (var h = 13; h < 15; h++)
                        {
                            var a = Branched(x, y, w, h, flags);
                            var b = Branchless(x, y, w, h, flags);
                            var c = BranchlessSIMD(x, y, w, h, flags);
                            var d = BranchlessSIMDNoMul(x, y, w, h, flags);
                            Assert.That(a, Is.EqualTo(b));
                            Assert.That(a, Is.EqualTo(c));
                            Assert.That(a, Is.EqualTo(d));
                        }
                    }
                }
            }
        }
    }
}

using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Attributes;
using SkiaSharp;

namespace Benchmarks;

// | Method                    | Mean     | Error     | StdDev    |
// |-------------------------- |---------:|----------:|----------:|
// | ColorFilterBench          | 9.722 us | 0.1484 us | 0.1239 us |
// | NormalBench               | 4.118 us | 0.0526 us | 0.0492 us |
// | NormalLessLoopingBench    | 4.008 us | 0.0328 us | 0.0291 us |
// | Vector256Bench            | 4.033 us | 0.0392 us | 0.0348 us |
// | Vector256LessLoopingBench | 3.905 us | 0.0301 us | 0.0251 us |

public unsafe class SKBitmapPaletteBench
{
    private static readonly byte[] _testImage = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x08, 0x08, 0x06, 0x00, 0x00, 0x00, 0xC4, 0x0F, 0xBE, 0x8B, 0x00, 0x00, 0x00, 0x04, 0x73, 0x42, 0x49, 0x54, 0x08, 0x08, 0x08, 0x08, 0x7C, 0x08, 0x64, 0x88, 0x00, 0x00, 0x00, 0x2A, 0x49, 0x44, 0x41, 0x54, 0x18, 0x95, 0x63, 0x60, 0xA0, 0x39, 0x60, 0x44, 0x17, 0x60, 0x66, 0x66, 0x66, 0xC6, 0xA9, 0x00, 0x5D, 0x92, 0x81, 0x81, 0x81, 0x81, 0x05, 0x9B, 0xB1, 0x7F, 0xFF, 0xFE, 0xFD, 0x0B, 0x63, 0x33, 0x11, 0x72, 0x03, 0x41, 0x05, 0x00, 0x3F, 0x5C, 0x04, 0x17, 0x6D, 0xD0, 0x43, 0xC1, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82];
    private static readonly SKBitmap _sourcebitmap = SKBitmap.Decode(_testImage).AsAlphaType(SKAlphaType.Unpremul);

    private static readonly int[] _palette = [0x50505050, 0x51515151, 0x52525252, 0x53535353];
    private static readonly SKColor[] _paletteC = [
        new SKColor(0x50505050),
        new SKColor(0x51515151),
        new SKColor(0x52525252),
        new SKColor(0x53535353),
    ];

    [Benchmark]
    public void Vector256Bench()
    {
        using var bitmap = _sourcebitmap.Clone();

        var locked = bitmap.Lock();
        var px = locked.Pixels;
        var nextLineDistance = (locked.Stride / sizeof(SKColor)) - bitmap.Width;

        var zeroCheck = Vector256.Create(0);
        var oneCheck = Vector256.Create(0x01010101);
        var twoCheck = Vector256.Create(0x02020202);
        var threeCheck = Vector256.Create(0x03030303);

        var zeroColor = Vector256.Create(_palette[0]);
        var oneColor = Vector256.Create(_palette[1]);
        var twoColor = Vector256.Create(_palette[2]);
        var threeColor = Vector256.Create(_palette[3]);

        for (var y = 0; y < locked.Height; ++y, px += nextLineDistance)
        {
            for (var x = 0; x < locked.Width; x += 8, px += 8)
            {
                var pixelVector = Vector256.Load((int*)px);
                // __m256i _mm256_blendv_epi8 (__m256i a, __m256i b, __m256i mask)
                // __m256i _mm256_cmpeq_epi32 (__m256i a, __m256i b)
                var result = Avx2.BlendVariable(pixelVector, zeroColor, Avx2.CompareEqual(pixelVector, zeroCheck));
                result = Avx2.BlendVariable(result, oneColor, Avx2.CompareEqual(pixelVector, oneCheck));
                result = Avx2.BlendVariable(result, twoColor, Avx2.CompareEqual(pixelVector, twoCheck));
                result = Avx2.BlendVariable(result, threeColor, Avx2.CompareEqual(pixelVector, threeCheck));
                // void _mm256_storeu_si256 (__m256i * mem_addr, __m256i a)
                Avx.Store((int*)px, result);
            }
        }
    }

    private static readonly Vector256<int> _zeroCheck = Vector256.Create(0);
    private static readonly Vector256<int> _oneCheck = Vector256.Create(0x01010101);
    private static readonly Vector256<int> _twoCheck = Vector256.Create(0x02020202);
    private static readonly Vector256<int> _threeCheck = Vector256.Create(0x03030303);

    [Benchmark]
    public void Vector256LessLoopingBench()
    {
        using var bitmap = _sourcebitmap.Clone();

        var locked = bitmap.Lock();
        ;
        var nextLineDistance = (locked.Stride / sizeof(SKColor)) - bitmap.Width;
        if (nextLineDistance != 0) throw new Exception();

        var zeroCheck = _zeroCheck;
        var oneCheck = _oneCheck;
        var twoCheck = _twoCheck;
        var threeCheck = _threeCheck;

        var zeroColor = Vector256.Create(_palette[0]);
        var oneColor = Vector256.Create(_palette[1]);
        var twoColor = Vector256.Create(_palette[2]);
        var threeColor = Vector256.Create(_palette[3]);

        var end = locked.End;

        for (var px = locked.Pixels; px < end; px += 8)
        {
            var pixelVector = Vector256.Load((int*)px);
            // __m256i _mm256_blendv_epi8 (__m256i a, __m256i b, __m256i mask)
            // __m256i _mm256_cmpeq_epi32 (__m256i a, __m256i b)
            var result = Avx2.BlendVariable(pixelVector, zeroColor, Avx2.CompareEqual(pixelVector, zeroCheck));
            result = Avx2.BlendVariable(result, oneColor, Avx2.CompareEqual(pixelVector, oneCheck));
            result = Avx2.BlendVariable(result, twoColor, Avx2.CompareEqual(pixelVector, twoCheck));
            result = Avx2.BlendVariable(result, threeColor, Avx2.CompareEqual(pixelVector, threeCheck));
            // void _mm256_storeu_si256 (__m256i * mem_addr, __m256i a)
            Avx.Store((int*)px, result);
        }
    }

    [Benchmark]
    public void NormalBench()
    {
        using var bitmap = _sourcebitmap.Clone();

        var locked = bitmap.Lock();
        var px = locked.Pixels;
        var nextLineDistance = (locked.Stride / sizeof(SKColor)) - bitmap.Width;
        var makeTransparent = true;

        var color0 = makeTransparent ? SKColors.Transparent : _paletteC[0];
        var color1 = _paletteC[1];
        var color2 = _paletteC[2];
        var color3 = _paletteC[3];

        var palette = stackalloc SKColor[] { color0, color1, color2, color3 };

        for (var y = 0; y < locked.Height; ++y, px += nextLineDistance)
        {
            for (var x = 0; x < locked.Width; ++x, ++px)
            {
                // Blue is the fastest to access because it does not use shifts.
                *px = palette[px->Blue];
            }
        }
    }

    [Benchmark]
    public void NormalLessLoopingBench()
    {
        using var bitmap = _sourcebitmap.Clone();
        var locked = bitmap.Lock();
        var px = locked.Pixels;
        var nextLineDistance = (locked.Stride / sizeof(SKColor)) - bitmap.Width;
        if (nextLineDistance != 0) throw new Exception();
        var makeTransparent = true;

        var color0 = makeTransparent ? SKColors.Transparent : _paletteC[0];
        var color1 = _paletteC[1];
        var color2 = _paletteC[2];
        var color3 = _paletteC[3];

        var palette = stackalloc SKColor[] { color0, color1, color2, color3 };
        var end = locked.End;
        for (; px < end; px += 8)
        {
            // Blue is the fastest to access because it does not use shifts.
            *px = palette[px->Blue];
            px[1] = palette[px[1].Blue];
            px[2] = palette[px[2].Blue];
            px[3] = palette[px[3].Blue];
            px[4] = palette[px[4].Blue];
            px[5] = palette[px[5].Blue];
            px[6] = palette[px[6].Blue];
            px[7] = palette[px[7].Blue];
        }
    }

    [Benchmark]
    public void ShaderBench()
    {
        using var bitmap = _sourcebitmap.Clone();

        var locked = bitmap.Lock();
        var px = locked.Pixels;
        var asd = px[5];
        var asd2 = px[4];
        var asd3 = px[6];

        var effect = SKRuntimeEffect.BuildColorFilter("""
                uniform vec4 color0;
                uniform vec4 color1;
                uniform vec4 color2;
                uniform vec4 color3;

                half4 main(half4 color) {
                    return color;
                }
            """);

        var uniforms = new SKRuntimeEffectUniforms(effect.Effect); // effect.Uniforms
        // uniforms["color0"] = _paletteC[0].ToVector4();
        // uniforms["color1"] = _paletteC[1].ToVector4();
        // uniforms["color2"] = _paletteC[2].ToVector4();
        // uniforms["color3"] = _paletteC[3].ToVector4();

        // effect.Uniforms["123"] = 122;

        using var paint = new SKPaint { ColorFilter = effect.Build() };
        using var canvas = new SKCanvas(bitmap);
        canvas.DrawPaint(paint);

        bitmap.SavePng(@"C:\users\joe\desktop\delete\_shader.png");
    }

    private static readonly byte[] _alphaTransform = new byte[256];
    private static readonly byte[] _redTransform = new byte[256];
    private static readonly byte[] _greenTransform = new byte[256];
    private static readonly byte[] _blueTransform = new byte[256];

    [Benchmark]
    public void ColorFilterBench()
    {
        using var bitmap = _sourcebitmap.Clone();

        var color0 = _paletteC[0];
        var color1 = _paletteC[1];
        var color2 = _paletteC[2];
        var color3 = _paletteC[3];

        var makeTransparent = true;
        _redTransform[0] = color0.Red;
        _redTransform[1] = color1.Red;
        _redTransform[2] = color2.Red;
        _redTransform[3] = color3.Red;

        _greenTransform[0] = color0.Green;
        _greenTransform[1] = color1.Green;
        _greenTransform[2] = color2.Green;
        _greenTransform[3] = color3.Green;

        _blueTransform[0] = color0.Blue;
        _blueTransform[1] = color1.Blue;
        _blueTransform[2] = color2.Blue;
        _blueTransform[3] = color3.Blue;

        if (makeTransparent)
        {
            Array.Fill(_alphaTransform, (byte)0, 0, 4);
        }
        else
        {
            _alphaTransform[0] = color0.Alpha;
            _alphaTransform[1] = color1.Alpha;
            _alphaTransform[2] = color2.Alpha;
            _alphaTransform[3] = color3.Alpha;
        }

        using var paint = new SKPaint
        {
            ColorFilter = SKColorFilter.CreateTable(_alphaTransform, _redTransform, _greenTransform, _blueTransform),
        };

        using var canvas = new SKCanvas(bitmap);
        canvas.DrawPaint(paint);
    }

    public static void Test()
    {
        var asd = new SKBitmapPaletteBench();
        asd.ShaderBench();
        asd.Vector256Bench();
        asd.NormalBench();
        asd.ColorFilterBench();
    }
}

internal static class SKBitmapExtensions
{
    internal readonly unsafe record struct SKBitmapLock(nint Data, int Stride, int Width, int Height)
    {
        public int Length => Stride * Height;
        public SKColor* Pixels => (SKColor*)Data;
        public SKColor* End => (SKColor*)(Data + Length);
        public SKColor* PtrFromPoint(int x, int y) => (SKColor*)(Data + y * Stride + x * sizeof(SKColor));
    }

    public static SKBitmapLock Lock(this SKBitmap bitmap)
    {
        return new SKBitmapLock(bitmap.GetPixels(), bitmap.RowBytes, bitmap.Width, bitmap.Height);
    }

    public static SKBitmap Clone(this SKBitmap original)
    {
        var bitmap = new SKBitmap(original.Width, original.Height, original.ColorType, original.AlphaType);
        using var canvas = new SKCanvas(bitmap);
        canvas.DrawBitmap(original, 0, 0);
        return bitmap;
    }

    public static SKBitmap AsAlphaType(this SKBitmap original, SKAlphaType alphaType)
    {
        var bitmap = new SKBitmap(original.Width, original.Height, original.ColorType, alphaType);
        using var canvas = new SKCanvas(bitmap);
        canvas.DrawBitmap(original, 0, 0);
        return bitmap;
    }

    public static void SavePng(this SKBitmap bitmap, string path, int quality = 100)
    {
        using (var image = SKImage.FromBitmap(bitmap))
        using (var data = image.Encode(SKEncodedImageFormat.Png, quality))
        using (var outputStream = File.OpenWrite(path))
        {
            data.SaveTo(outputStream);
        }
    }

    public static Vector4 ToVector4(this SKColor color)
    {
        return new Vector4(
            color.Red / 255f,
            color.Green / 255f,
            color.Blue / 255f,
            color.Alpha / 255f
        );
    }
}
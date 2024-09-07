using System.Reflection;
using SkiaSharp;

namespace Benchmarks;

internal readonly struct Asset
{
    private static readonly Lazy<string[]> _resourceNames = new(() => Assembly.GetExecutingAssembly().GetManifestResourceNames());

    public static Stream GetEmbeddedResource(string name)
    {
        var resourceName = _resourceNames.Value.FirstOrDefault(t => t.EndsWith(name))
            ?? throw new FileNotFoundException($"Resource not found: {name}");

        return Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"Resource not found: {name}"); ;
    }

    public static SKBitmap GetEmbeddedSKBitmpa(string name, SKAlphaType alphatype)
    {
        using var stream = GetEmbeddedResource(name);
        return SKBitmap.Decode(stream).AsAlphaType(alphatype);
    }
}
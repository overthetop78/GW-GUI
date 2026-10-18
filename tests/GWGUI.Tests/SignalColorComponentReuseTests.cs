using System.Runtime.InteropServices;
using System.Security.Cryptography;
using GWGUI.App.Rendering.Emulation.Processing;

namespace GWGUI.Tests;

public sealed class SignalColorComponentReuseTests
{
    [Theory]
    [InlineData("NTSC", "3B414D036FCD8F475054C0BA8492D3B119A6E2BF9EB0E6C35835F7AEBA556262")]
    [InlineData("PAL", "2C0EBE55CD3243EBAF36B6467C9A3111982D5CC496F49693781876050F72BBD6")]
    [InlineData("Component", "DBBCE3CCF42B8350B07CEF9899A818976A7C0E5A9DC436CB4B5F6CD6DCBAD82C")]
    public void PreservesCpuResultsForAllIntensitiesAndEdgeDimensions(string signal, string expected)
    {
        var bytes = new List<byte>();
        foreach (var width in new[] { 1, 5 })
        foreach (var height in new[] { 1, 3 })
        foreach (var intensity in new[] { -1, 0, 17, 50, 100 })
        {
            var colors = new float[width * height * 3];
            for (var i = 0; i < colors.Length; i++) colors[i] = (i % 17 - 3) / 11f;
            switch (signal)
            {
                case "NTSC": SignalStandardNtsc.Apply(colors, width, height, intensity); break;
                case "PAL": SignalStandardPal.Apply(colors, width, height, intensity); break;
                case "Component": SignalConnectionComponent.Apply(colors, width, height, intensity); break;
                default: throw new ArgumentOutOfRangeException(nameof(signal));
            }
            bytes.AddRange(MemoryMarshal.AsBytes(colors.AsSpan()).ToArray());
        }
        var actual = Convert.ToHexString(SHA256.HashData(bytes.ToArray()));
        Assert.Equal(expected, actual);
    }
}

using System.Reflection;
using GWGUI.Scp.Recognition.Definitions;

namespace GWGUI.Tests;

public sealed class DiskImageFileExtensionsTests
{
    [Fact]
    public void EveryExtensionStartsWithDotAndUsesLowercase()
    {
        var extensions = PublicConstants();

        Assert.NotEmpty(extensions);
        Assert.All(extensions, extension =>
        {
            Assert.StartsWith(".", extension);
            Assert.Equal(extension.ToLowerInvariant(), extension);
        });
    }

    [Fact]
    public void EveryExtensionValueIsDeclaredOnce()
    {
        var duplicates = PublicConstants()
            .GroupBy(extension => extension, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        Assert.Empty(duplicates);
    }

    private static string[] PublicConstants() =>
        typeof(DiskImageFileExtensions)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
            .Select(field => Assert.IsType<string>(field.GetRawConstantValue()))
            .ToArray();
}

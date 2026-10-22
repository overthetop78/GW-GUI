using GWGUI.Emulation;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class InternationalOptionTablesTests
{
    [Fact]
    public void NullLegacyTableDoesNotReadOrMerge()
    {
        var options = EmulationSdk.OptionProcessing.CopyInternationalDefinitions<string>(0, 0, 1,
            _ => throw new InvalidOperationException(), (_, _) => throw new InvalidOperationException());
        Assert.Empty(options);
    }

    [Fact]
    public void NullVersionTwoTableDoesNotReadOrMergeAndClearsCategories()
    {
        static IReadOnlyList<string> Read(nint pointer, out IReadOnlyList<string> categories)
            => throw new InvalidOperationException();
        var options = EmulationSdk.OptionProcessing.CopyInternationalDefinitions<string, string>(0, 0, 1,
            Read, (_, _) => throw new InvalidOperationException(),
            (_, _) => throw new InvalidOperationException(), out var categories);
        Assert.Empty(options);
        Assert.Empty(categories);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LegacyReadsBothPointersBeforeCopyingEnglishThenLocal(bool noLocalTable)
    {
        var table = Marshal.AllocHGlobal(2 * IntPtr.Size);
        try
        {
            var localPointer = noLocalTable ? nint.Zero : (nint)22;
            Marshal.WriteIntPtr(table, 0, 11);
            Marshal.WriteIntPtr(table, IntPtr.Size, localPointer);
            var reads = new List<nint>();
            IReadOnlyList<string> english = ["english"];
            IReadOnlyList<string> local = noLocalTable ? [] : ["local"];
            IReadOnlyList<string> merged = ["merged"];
            var result = EmulationSdk.OptionProcessing.CopyInternationalDefinitions<string>(table, 0, 1,
                pointer =>
                {
                    reads.Add(pointer);
                    if (pointer != 11) return local;
                    Marshal.WriteIntPtr(table, IntPtr.Size, 99);
                    return english;
                }, (first, second) =>
                {
                    Assert.Equal(new nint[] { 11, localPointer }, reads);
                    Assert.Same(english, first);
                    Assert.Same(local, second);
                    return merged;
                });
            Assert.Same(merged, result);
        }
        finally { Marshal.FreeHGlobal(table); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void VersionTwoCopiesEnglishThenLocalAndMergesOptionsBeforeCategories(bool noLocalTable)
    {
        var table = Marshal.AllocHGlobal(2 * IntPtr.Size);
        try
        {
            var localPointer = noLocalTable ? nint.Zero : (nint)22;
            Marshal.WriteIntPtr(table, 0, 11);
            Marshal.WriteIntPtr(table, IntPtr.Size, localPointer);
            var reads = new List<nint>();
            var events = new List<string>();
            IReadOnlyList<string> englishOptions = ["english option"];
            IReadOnlyList<string> localOptions = noLocalTable ? [] : ["local option"];
            IReadOnlyList<string> englishCategories = ["english category"];
            IReadOnlyList<string> localCategories = noLocalTable ? [] : ["local category"];
            IReadOnlyList<string> mergedOptions = ["merged option"];
            IReadOnlyList<string> mergedCategories = ["merged category"];
            IReadOnlyList<string> Read(nint pointer, out IReadOnlyList<string> categories)
            {
                reads.Add(pointer);
                events.Add("read");
                if (pointer == 11)
                {
                    Marshal.WriteIntPtr(table, IntPtr.Size, 99);
                    categories = englishCategories;
                    return englishOptions;
                }
                categories = localCategories;
                return localOptions;
            }
            var result = EmulationSdk.OptionProcessing.CopyInternationalDefinitions<string, string>(table, 0, 1,
                Read, (first, second) =>
                {
                    events.Add("options");
                    Assert.Same(englishOptions, first);
                    Assert.Same(localOptions, second);
                    return mergedOptions;
                }, (first, second) =>
                {
                    events.Add("categories");
                    Assert.Same(englishCategories, first);
                    Assert.Same(localCategories, second);
                    return mergedCategories;
                }, out var categories);
            Assert.Equal(new nint[] { 11, localPointer }, reads);
            Assert.Equal(new[] { "read", "read", "options", "categories" }, events);
            Assert.Same(mergedOptions, result);
            Assert.Same(mergedCategories, categories);
        }
        finally { Marshal.FreeHGlobal(table); }
    }
}

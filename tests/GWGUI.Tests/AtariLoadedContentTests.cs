using GWGUI.Emulation.Atari;
using System.IO;
using System.Runtime.InteropServices;

namespace GWGUI.Tests;

public sealed class AtariLoadedContentTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Content_UsesPathOrMemoryAccordingToCoreContract(bool needsFullPath)
    {
        var root = Path.Combine(Path.GetTempPath(), "GWGUI-Atari-Content", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var path = Path.Combine(root, "game.bin");
        File.WriteAllBytes(path, [1, 2, 3, 4]);
        try
        {
            using var content = AtariContentFunctions.Create(path, needsFullPath,
                new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "bin" });
            var info = Marshal.PtrToStructure<ExternalCoreApi.GameInfo>(content.GameInfo);

            Assert.Equal(Path.GetFullPath(path), Marshal.PtrToStringUTF8(info.Path));
            Assert.Equal(needsFullPath ? nint.Zero : (nint)4, (nint)info.Size);
            Assert.Equal(needsFullPath, info.Data == 0);
            if (!needsFullPath)
            {
                var bytes = new byte[4];
                Marshal.Copy(info.Data, bytes, 0, bytes.Length);
                Assert.Equal(new byte[] { 1, 2, 3, 4 }, bytes);
            }

            content.Dispose();
            Assert.Equal(nint.Zero, content.GameInfo);
            using var exclusive = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            Assert.Equal(4, exclusive.Length);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void UnsupportedExtension_IsStructuredContentError()
    {
        var path = Path.GetTempFileName();
        try
        {
            var error = Assert.Throws<AtariEmulationException>(() => AtariContentFunctions.Create(
                path, true, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "rom" }));
            Assert.Equal(AtariErrorCategory.Content, error.Category);
            Assert.Equal(AtariErrorCode.ContentUnsupported, error.Code);
            Assert.Equal(Path.GetExtension(path).TrimStart('.'), error.Context[AtariConstants.ExtensionContextKey]);
            Assert.Equal("rom", error.Context[AtariConstants.SupportedExtensionsContextKey]);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void MissingMedia_IsStructuredContentError()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.st");

        var error = Assert.Throws<AtariEmulationException>(() => AtariContentFunctions.Create(
            path, true, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "st" }));

        Assert.Equal(AtariErrorCategory.Content, error.Category);
        Assert.Equal(AtariErrorCode.ContentNotFound, error.Code);
        Assert.Equal(path, error.Context[AtariConstants.PathContextKey]);
    }
}

using System.IO;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Atari;
using GWGUI.Emulation.Atari.Cores;
using GWGUI.Emulation.Common;

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
            using var content = AtariLoadedContent.Create(path, needsFullPath,
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
            var error = Assert.Throws<AtariEmulationException>(() => AtariLoadedContent.Create(
                path, true, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "rom" }));
            Assert.Equal(AtariErrorKind.Content, error.Kind);
            Assert.Equal(AtariErrorCode.ContentUnsupported, error.Code);
        }
        finally
        {
            File.Delete(path);
        }
    }
}

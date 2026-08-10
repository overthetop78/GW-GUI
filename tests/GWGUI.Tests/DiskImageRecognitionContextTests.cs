using System.IO;
using GWGUI.MediaEngine.Recognition;

namespace GWGUI.Tests;

/// <summary>Vérifie les informations et la lecture partagée du contexte public de reconnaissance.</summary>
public sealed class DiskImageRecognitionContextTests
{
    [Fact]
    public async Task PreservesPathLengthNormalizedExtensionRequestedFormatAndSharedBytes()
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-context-{Guid.NewGuid():N}.DSK");
        var expectedBytes = new byte[] { 0x12, 0x34, 0x56, 0x78 };
        try
        {
            await File.WriteAllBytesAsync(path, expectedBytes);

            var context = new DiskImageRecognitionContext(path, "test.format");
            var firstRead = await context.ReadBytesAsync();
            var secondRead = await context.ReadBytesAsync();

            Assert.Equal(path, context.Path);
            Assert.Equal(expectedBytes.Length, context.Length);
            Assert.Equal(".dsk", context.Extension);
            Assert.Equal("test.format", context.RequestedFormatId);
            Assert.Equal(expectedBytes, firstRead);
            Assert.Same(firstRead, secondRead);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task PropagatesCancellationBeforeTheFirstContentRead()
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-context-{Guid.NewGuid():N}.img");
        try
        {
            await File.WriteAllBytesAsync(path, [0x01]);
            var context = new DiskImageRecognitionContext(path, null);
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                context.ReadBytesAsync(cancellation.Token));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}

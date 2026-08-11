using System.IO;
using GWGUI.MediaEngine.Recognition;

namespace GWGUI.Tests;

/// <summary>Vérifie les informations et la lecture partagée du contexte public de reconnaissance.</summary>
public sealed class DiskImageRecognitionContextTests
{
    /// <summary>Vérifie les propriétés, la normalisation et le partage du contenu lu.</summary>
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
            Assert.Equal(expectedBytes, firstRead.ToArray());
            Assert.Equal(firstRead, secondRead);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    /// <summary>Vérifie le rejet des chemins nuls, vides ou composés uniquement d'espaces.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RejectsInvalidPaths(string? path) => Assert.ThrowsAny<ArgumentException>(() => new DiskImageRecognitionContext(path!, null));

    /// <summary>Vérifie que les appels concurrents reçoivent la même mémoire issue de l'unique lecture.</summary>
    [Fact]
    public async Task SharesOneReadAcrossConcurrentCallers()
    {
        var path = Path.GetTempFileName();
        try
        {
            await File.WriteAllBytesAsync(path, Enumerable.Range(0, 256).Select(index => (byte)index).ToArray());
            var context = new DiskImageRecognitionContext(path, null);
            var reads = await Task.WhenAll(Enumerable.Range(0, 32).Select(_ => context.ReadBytesAsync()));

            Assert.All(reads, read => Assert.Equal(reads[0], read));
            Assert.Equal(typeof(Task<ReadOnlyMemory<byte>>), typeof(DiskImageRecognitionContext).GetMethod(nameof(DiskImageRecognitionContext.ReadBytesAsync))!.ReturnType);
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>Vérifie qu'une erreur de la première lecture reste mémorisée après recréation du fichier.</summary>
    [Fact]
    public async Task ReusesTheFirstReadFailure()
    {
        var path = Path.GetTempFileName();
        try
        {
            var context = new DiskImageRecognitionContext(path, null);
            File.Delete(path);
            await Assert.ThrowsAsync<FileNotFoundException>(() => context.ReadBytesAsync());
            await File.WriteAllBytesAsync(path, [0x42]);
            await Assert.ThrowsAsync<FileNotFoundException>(() => context.ReadBytesAsync());
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    /// <summary>Vérifie qu'une annulation de la première lecture reste mémorisée.</summary>
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

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => context.ReadBytesAsync(cancellation.Token));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => context.ReadBytesAsync());
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}

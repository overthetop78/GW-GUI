using System.IO;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

/// <summary>Vérifie l'ordre, la reprise après rejet et la propagation des erreurs du registre de reconnaissance.</summary>
public sealed class DiskImageRecognitionRegistryTests
{
    [Fact]
    public async Task ContinuesAfterRejectedReaderAndPassesTheSameContentToTheNextPolicy()
    {
        var path = await CreateTemporaryImageAsync();
        try
        {
            var incompatible = new FakePolicy(canRead: false);
            var rejected = new FakePolicy(canRead: true, reject: true);
            var accepted = new FakePolicy(canRead: true);
            var registry = new DiskImageRecognitionRegistry([incompatible, rejected, accepted]);

            var image = await registry.ReadAsync(path, "accepted.format", CancellationToken.None);

            Assert.Equal("accepted.format", image.FormatId);
            Assert.Equal(1, incompatible.CanReadCalls);
            Assert.Equal(0, incompatible.ReadCalls);
            Assert.Equal(1, rejected.CanReadCalls);
            Assert.Equal(1, rejected.ReadCalls);
            Assert.Equal(1, accepted.CanReadCalls);
            Assert.Equal(1, accepted.ReadCalls);
            Assert.Same(rejected.ReceivedBytes, accepted.ReceivedBytes);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ReportsUnsupportedFormatWhenNoPolicyProducesAnImage()
    {
        var path = await CreateTemporaryImageAsync();
        try
        {
            var incompatible = new FakePolicy(canRead: false);
            var rejected = new FakePolicy(canRead: true, reject: true);
            var registry = new DiskImageRecognitionRegistry([incompatible, rejected]);

            await Assert.ThrowsAsync<NotSupportedException>(() =>
                registry.ReadAsync(path, null, CancellationToken.None));

            Assert.Equal(1, incompatible.CanReadCalls);
            Assert.Equal(0, incompatible.ReadCalls);
            Assert.Equal(1, rejected.CanReadCalls);
            Assert.Equal(1, rejected.ReadCalls);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task CancellationStopsTheLoopBeforeAnyPolicyCall()
    {
        var path = await CreateTemporaryImageAsync();
        try
        {
            var policy = new FakePolicy(canRead: true);
            var registry = new DiskImageRecognitionRegistry([policy]);
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                registry.ReadAsync(path, null, cancellation.Token));

            Assert.Equal(0, policy.CanReadCalls);
            Assert.Equal(0, policy.ReadCalls);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task PropagatesFileAccessErrors()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), $"gwgui-missing-{Guid.NewGuid():N}.img");
        var registry = new DiskImageRecognitionRegistry([new FakePolicy(canRead: true)]);

        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            registry.ReadAsync(missingPath, null, CancellationToken.None));
    }

    private static async Task<string> CreateTemporaryImageAsync()
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-registry-{Guid.NewGuid():N}.img");
        await File.WriteAllBytesAsync(path, [0x11, 0x22, 0x33]);
        return path;
    }

    /// <summary>Politique instrumentée permettant de compter chaque étape et de choisir son résultat.</summary>
    private sealed class FakePolicy(bool canRead, bool reject = false) : IDiskImageRecognitionPolicy
    {
        /// <summary>Nombre d'appels de présélection reçus.</summary>
        public int CanReadCalls { get; private set; }
        /// <summary>Nombre d'appels de lecture reçus.</summary>
        public int ReadCalls { get; private set; }
        /// <summary>Référence du tableau partagé reçu pendant la lecture.</summary>
        public byte[]? ReceivedBytes { get; private set; }

        /// <summary>Compte la présélection et retourne la décision configurée.</summary>
        public ValueTask<bool> CanReadAsync(
            DiskImageRecognitionContext context,
            CancellationToken cancellationToken)
        {
            CanReadCalls++;
            return ValueTask.FromResult(canRead);
        }

        /// <summary>Compte la lecture, mémorise le contenu puis le rejette ou produit une image minimale.</summary>
        public async Task<SectorImage> ReadAsync(
            DiskImageRecognitionContext context,
            CancellationToken cancellationToken)
        {
            ReadCalls++;
            ReceivedBytes = await context.ReadBytesAsync(cancellationToken);
            if (reject) throw new InvalidDataException("Rejet factice du candidat.");
            return new(
                context.RequestedFormatId ?? "accepted.format",
                ReceivedBytes.Length,
                1,
                1,
                1,
                [new SectorBlock(0, new SectorAddress(0, 0, 1), ReceivedBytes)]);
        }
    }
}

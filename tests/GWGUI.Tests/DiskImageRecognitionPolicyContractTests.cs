using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.SectorImages;
using System.IO;

namespace GWGUI.Tests;

/// <summary>Vérifie le contrat public séparant la présélection et la lecture d'une politique de reconnaissance.</summary>
public sealed class DiskImageRecognitionPolicyContractTests
{
    [Fact]
    public async Task FakePolicyExposesPreselectionAndReadingAfterContractMove()
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-policy-{Guid.NewGuid():N}.img");
        try
        {
            await File.WriteAllBytesAsync(path, [0x5a]);
            var context = new DiskImageRecognitionContext(path, "test.format");
            IDiskImageRecognitionPolicy policy = new FakeRecognitionPolicy();

            Assert.True(await policy.CanReadAsync(context, CancellationToken.None));
            var image = await policy.ReadAsync(context, CancellationToken.None);

            Assert.Equal("test.format", image.FormatId);
            Assert.Equal(new byte[] { 0x5a }, image.GetBlock(0).ToArray());
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task SynchronousAndAsynchronousPreselectionKeepTheirDistinctExecutionPaths()
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-policy-{Guid.NewGuid():N}.img");
        try
        {
            await File.WriteAllBytesAsync(path, [0x5a]);
            var context = new DiskImageRecognitionContext(path, null);
            IDiskImageRecognitionPolicy synchronous = new IncompatiblePolicy();
            IDiskImageRecognitionPolicy asynchronous = new ExaminingPolicy();

            Assert.False(await synchronous.CanReadAsync(context, CancellationToken.None));
            Assert.True(await asynchronous.CanReadAsync(context, CancellationToken.None));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ReadingCanRejectACandidateWithEitherSupportedException(bool unsupported)
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-policy-{Guid.NewGuid():N}.img");
        try
        {
            await File.WriteAllBytesAsync(path, [0x5a]);
            var policy = new RejectingPolicy(unsupported);
            var context = new DiskImageRecognitionContext(path, null);

            Assert.True(await policy.CanReadAsync(context, CancellationToken.None));
            if (unsupported) await Assert.ThrowsAsync<NotSupportedException>(() => policy.ReadAsync(context, CancellationToken.None));
            else await Assert.ThrowsAsync<InvalidDataException>(() => policy.ReadAsync(context, CancellationToken.None));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    /// <summary>Politique minimale utilisée pour exercer les deux membres du contrat.</summary>
    private sealed class FakeRecognitionPolicy : IDiskImageRecognitionPolicy
    {
        /// <summary>Accepte le contexte de test.</summary>
        public ValueTask<bool> CanReadAsync(
            DiskImageRecognitionContext context,
            CancellationToken cancellationToken) => ValueTask.FromResult(true);

        /// <summary>Transforme l'unique octet du contexte en image sectorielle minimale.</summary>
        public async Task<SectorImage> ReadAsync(
            DiskImageRecognitionContext context,
            CancellationToken cancellationToken)
        {
            var bytes = await context.ReadBytesAsync(cancellationToken);
            return new(
                context.RequestedFormatId!,
                1,
                1,
                1,
                1,
                [new SectorBlock(0, new SectorAddress(0, 0, 1), bytes.ToArray())]);
        }
    }

    /// <summary>Politique répondant immédiatement qu'un candidat est incompatible.</summary>
    private sealed class IncompatiblePolicy : IDiskImageRecognitionPolicy
    {
        /// <summary>Rejette immédiatement la présélection.</summary>
        public ValueTask<bool> CanReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken) => ValueTask.FromResult(false);

        /// <summary>Ne doit jamais être appelée après le rejet.</summary>
        public Task<SectorImage> ReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken) => throw new InvalidOperationException();
    }

    /// <summary>Politique examinant de manière asynchrone les octets partagés.</summary>
    private sealed class ExaminingPolicy : IDiskImageRecognitionPolicy
    {
        /// <summary>Accepte uniquement le contenu attendu.</summary>
        public async ValueTask<bool> CanReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken) => (await context.ReadBytesAsync(cancellationToken)).Span[0] == 0x5a;

        /// <summary>Produit l'image validée.</summary>
        public Task<SectorImage> ReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken) => new FakeRecognitionPolicy().ReadAsync(context, cancellationToken);
    }

    /// <summary>Politique validant la présélection puis rejetant la lecture.</summary>
    private sealed class RejectingPolicy(bool unsupported) : IDiskImageRecognitionPolicy
    {
        /// <summary>Accepte le candidat avant sa validation complète.</summary>
        public ValueTask<bool> CanReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken) => ValueTask.FromResult(true);

        /// <summary>Rejette la lecture avec l'une des deux exceptions prévues par le contrat.</summary>
        public Task<SectorImage> ReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken) => unsupported ? throw new NotSupportedException() : throw new InvalidDataException();
    }
}

using System.IO;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.SectorImages;

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
                [new SectorBlock(0, new SectorAddress(0, 0, 1), bytes)]);
        }
    }
}

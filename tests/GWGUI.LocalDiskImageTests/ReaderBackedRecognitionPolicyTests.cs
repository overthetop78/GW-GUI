using GWGUI.MediaEngine.Containers.Apple;
using GWGUI.MediaEngine.Containers.Coherent;
using GWGUI.MediaEngine.Containers.Dec.Rx02;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Recognition.Policies;
using GWGUI.MediaEngine.SectorImages;
using System.IO;

namespace GWGUI.Tests;

/// <summary>VÃ©rifie la dÃ©lÃ©gation commune des politiques de reconnaissance adossÃ©es Ã  un Reader.</summary>
public sealed class ReaderBackedRecognitionPolicyTests
{
    /// <summary>VÃ©rifie qu'une fonction de lecture est obligatoire.</summary>
    [Fact]
    public void RejectsNullReadFunction() => Assert.Throws<ArgumentNullException>(() => new TestPolicy(null!));

    /// <summary>VÃ©rifie que la dÃ©lÃ©gation reÃ§oit exactement le contexte et le jeton fournis.</summary>
    [Fact]
    public async Task PassesTheSameContextAndCancellationTokenToReadFunction()
    {
        var path = Path.GetTempFileName();
        try
        {
            var context = new DiskImageRecognitionContext(path, "test.format");
            using var cancellation = new CancellationTokenSource();
            DiskImageRecognitionContext? receivedContext = null;
            CancellationToken receivedToken = default;
            var expected = CreateImage("expected");
            var policy = new TestPolicy((candidate, token) =>
            {
                receivedContext = candidate;
                receivedToken = token;
                return Task.FromResult(expected);
            });

            var actual = await policy.ReadAsync(context, cancellation.Token);

            Assert.Same(context, receivedContext);
            Assert.Equal(cancellation.Token, receivedToken);
            Assert.Same(expected, actual);
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>VÃ©rifie que la politique Apple rÃ©utilise les octets chargÃ©s pendant la prÃ©sÃ©lection.</summary>
    [Fact]
    public async Task ApplePolicyReadsFromTheContextAfterTheFileIsRemoved()
    {
        var source = Path.Combine(FindImageTestRoot(), "Apple II", "AMR Hard Drive Utility Disk 3.5.2mg");
        await AssertReadsAfterRemovalAsync(source, path => new AppleImageRecognitionPolicy(new AppleDiskImageReader()), path => new AppleDiskImageReader().ReadAsync(path));
    }

    /// <summary>VÃ©rifie que la politique COHERENT rÃ©utilise les octets chargÃ©s pendant la prÃ©sÃ©lection.</summary>
    [Fact]
    public async Task CoherentPolicyReadsFromTheContextAfterTheFileIsRemoved()
    {
        var source = Directory.EnumerateFiles(FindImageTestRoot(), "COHERENT - Volume 1 - High Resolution.bin", SearchOption.AllDirectories).Single();
        await AssertReadsAfterRemovalAsync(source, path => new CoherentImageRecognitionPolicy(new CoherentRawImageReader()), path => new CoherentRawImageReader().ReadAsync(path));
    }

    /// <summary>VÃ©rifie que la politique DEC RX02 rÃ©utilise les octets chargÃ©s pendant la prÃ©sÃ©lection.</summary>
    [Fact]
    public async Task DecRx02PolicyReadsFromTheContextAfterTheFileIsRemoved()
    {
        var source = Path.Combine(FindImageTestRoot(), "validated_images", "DEC", "MINC", "8 pouces - RX02 - DEC RT-11 - 500 Kio", "BA-J837B-BC_MINC_MA_DEMO_23_V2.0_BIN_RX2.img");
        await AssertReadsAfterRemovalAsync(source, path => new DecRx02ImageRecognitionPolicy(new DecRx02Reader()), path => new DecRx02Reader().ReadAsync(path));
    }

    /// <summary>Compare la faÃ§ade par chemin Ã  la politique, puis retire le fichier aprÃ¨s la prÃ©sÃ©lection.</summary>
    private static async Task AssertReadsAfterRemovalAsync(string source, Func<string, IDiskImageRecognitionPolicy> createPolicy, Func<string, Task<SectorImage>> readByPath)
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-reader-policy-{Guid.NewGuid():N}{Path.GetExtension(source)}");
        try
        {
            File.Copy(source, path);
            var expected = await readByPath(path);
            var context = new DiskImageRecognitionContext(path, null);
            var policy = createPolicy(path);
            Assert.True(await policy.CanReadAsync(context, CancellationToken.None));
            File.Delete(path);

            var actual = await policy.ReadAsync(context, CancellationToken.None);

            Assert.Equal(expected.FormatId, actual.FormatId);
            Assert.Equal(expected.BlockSize, actual.BlockSize);
            Assert.Equal(expected.Cylinders, actual.Cylinders);
            Assert.Equal(expected.Heads, actual.Heads);
            Assert.Equal(expected.SectorsPerTrack, actual.SectorsPerTrack);
            Assert.Equal(expected.Capacity, actual.Capacity);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    /// <summary>Recherche le corpus local non versionnÃ©.</summary>
    private static string FindImageTestRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "image_test");
            if (Directory.Exists(candidate)) return candidate;
        }
        throw new DirectoryNotFoundException("Le dossier local image_test est introuvable.");
    }

    /// <summary>CrÃ©e une image minimale utilisÃ©e pour vÃ©rifier l'identitÃ© de la valeur retournÃ©e.</summary>
    private static SectorImage CreateImage(string formatId) => new(formatId, 1, 1, 1, 1, [new SectorBlock(0, new SectorAddress(0, 0, 1), [0x01])]);

    /// <summary>Expose la politique abstraite pour contrÃ´ler son contrat commun.</summary>
    private sealed class TestPolicy(Func<DiskImageRecognitionContext, CancellationToken, Task<SectorImage>> read) : ReaderBackedRecognitionPolicy(read)
    {
        /// <summary>Accepte toujours le contexte de test.</summary>
        public override ValueTask<bool> CanReadAsync(DiskImageRecognitionContext context, CancellationToken cancellationToken) => ValueTask.FromResult(true);
    }
}

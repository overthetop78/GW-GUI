using System.IO;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Recognition.Policies;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

/// <summary>Vérifie la reconnaissance publique des conteneurs Apple signés et des candidats bruts.</summary>
public sealed class AppleImageRecognitionTests
{
    [Theory]
    [InlineData("Apple II/AMR Hard Drive Utility Disk 3.5.2mg")]
    [InlineData("Apple II/3DChart! (1984)(Spectral Graphics Software)(US)(Disk 1 of 2).woz")]
    [InlineData("Apple II/816-Paint v3.1 (1987)(Baudville)(IIE)[128K][5.25''].woz")]
    public async Task RecognizesTwoImgAndWozByContentWithAnUnusualExtension(string relativePath)
    {
        await AssertRecognizedAfterRenamingAsync(ImagePath(relativePath));
    }

    [Fact]
    public async Task RecognizesDiskCopyByPrivateWordWithAnUnusualExtension()
    {
        var path = Directory.EnumerateFiles(FindImageTestRoot(), "*LisaGuide.image", SearchOption.AllDirectories)
            .Single();

        await AssertRecognizedAfterRenamingAsync(path);
    }

    [Theory]
    [InlineData(".do")]
    [InlineData(".po")]
    [InlineData(".d13")]
    [InlineData(".nib")]
    [InlineData(".dsk")]
    [InlineData(".img")]
    public async Task RawAppleExtensionAloneDoesNotMakeInvalidContentReadable(string extension)
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-invalid-apple-{Guid.NewGuid():N}{extension}");
        try
        {
            await File.WriteAllBytesAsync(path, [0x00]);

            var registry = new DiskImageRecognitionRegistry([new AppleImageRecognitionPolicy(new AppleDiskImageReader()), new AcceptedPolicy()]);

            var image = await registry.ReadAsync(path, null, CancellationToken.None);

            Assert.Equal("fallback", image.FormatId);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    /// <summary>Vérifie la lecture des représentations brutes Apple II possédant une capacité valide.</summary>
    [Theory]
    [InlineData(".do", 143_360)]
    [InlineData(".po", 143_360)]
    [InlineData(".d13", 116_480)]
    public async Task RecognizesValidRawAppleImages(string extension, int length)
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-valid-apple-{Guid.NewGuid():N}{extension}");
        try
        {
            await File.WriteAllBytesAsync(path, new byte[length]);

            var explored = await DiskImageExplorer.CreateDefault().ExploreAsync(path);

            Assert.NotEqual("unknown", explored.Image.FormatId);
            Assert.Equal(length, explored.Image.Capacity);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    /// <summary>Vérifie qu'une image NIB complète est présélectionnée puis validée.</summary>
    [Fact]
    public async Task RecognizesACompleteNibImage()
    {
        var path = ImagePath("Apple II/Merlin (1983)(Southwestern Data Systems)(US)(Side A).nib");

        var explored = await DiskImageExplorer.CreateDefault().ExploreAsync(path);

        Assert.NotEqual("unknown", explored.Image.FormatId);
    }

    /// <summary>Vérifie que les familles Apple explicitement demandées sont traitées de la même manière pour DSK et IMG.</summary>
    [Theory]
    [InlineData("apple2.", ".dsk")]
    [InlineData("apple2.", ".img")]
    [InlineData("apple3.", ".dsk")]
    [InlineData("apple3.", ".img")]
    [InlineData("applelisa.", ".dsk")]
    [InlineData("applelisa.", ".img")]
    [InlineData("applemac.", ".dsk")]
    [InlineData("applemac.", ".img")]
    [InlineData("mac.", ".dsk")]
    [InlineData("mac.", ".img")]
    public async Task ExplicitAppleFamiliesPreselectDskAndImg(string formatId, string extension)
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-explicit-apple-{Guid.NewGuid():N}{extension}");
        try
        {
            await File.WriteAllBytesAsync(path, [0x00]);
            var policy = new AppleImageRecognitionPolicy(new AppleDiskImageReader());

            Assert.True(await policy.CanReadAsync(new(path, formatId), CancellationToken.None));
            await Assert.ThrowsAsync<DiskImageCandidatesRejectedException>(() => DiskImageExplorer.CreateDefault().ExploreAsync(path, formatId));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task RegistryTriesTheNextPolicyWhenAppleReaderRejectsTheCandidate()
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-apple-candidate-{Guid.NewGuid():N}.do");
        try
        {
            await File.WriteAllBytesAsync(path, [0x00]);
            var apple = new AppleReaderCandidatePolicy();
            var fallback = new AcceptedPolicy();
            var registry = new DiskImageRecognitionRegistry([apple, fallback]);

            var image = await registry.ReadAsync(path, null, CancellationToken.None);

            Assert.Equal("fallback", image.FormatId);
            Assert.Equal(1, apple.ReadCalls);
            Assert.Equal(1, fallback.ReadCalls);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    private static async Task AssertRecognizedAfterRenamingAsync(string sourcePath)
    {
        var unusualPath = Path.Combine(Path.GetTempPath(), $"gwgui-apple-{Guid.NewGuid():N}.unexpected");
        try
        {
            File.Copy(sourcePath, unusualPath);
            var expected = await new AppleDiskImageReader().ReadAsync(sourcePath);

            var explored = await DiskImageExplorer.CreateDefault().ExploreAsync(unusualPath);

            Assert.Equal(expected.FormatId, explored.Image.FormatId);
            Assert.Equal(expected.BlockSize, explored.Image.BlockSize);
            Assert.Equal(expected.BlockCount, explored.Image.BlockCount);
            Assert.Equal(expected.Capacity, explored.Image.Capacity);
            foreach (var expectedBlock in expected.AvailableBlocks)
            {
                Assert.True(explored.Image.TryGetBlock(expectedBlock.LogicalBlock, out var actualBlock));
                Assert.Equal(expectedBlock.Address, actualBlock.Address);
                Assert.Equal(expectedBlock.Data, actualBlock.Data);
            }
        }
        finally
        {
            if (File.Exists(unusualPath)) File.Delete(unusualPath);
        }
    }

    private static string ImagePath(string relativePath) =>
        Path.Combine(FindImageTestRoot(), relativePath.Replace('/', Path.DirectorySeparatorChar));

    private static string FindImageTestRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "image_test");
            if (Directory.Exists(candidate)) return candidate;
        }
        throw new DirectoryNotFoundException("Le dossier local image_test est introuvable.");
    }

    /// <summary>Présélectionne le fichier puis délègue sa validation au lecteur Apple public.</summary>
    private sealed class AppleReaderCandidatePolicy : IDiskImageRecognitionPolicy
    {
        /// <summary>Nombre de lectures Apple tentées.</summary>
        public int ReadCalls { get; private set; }

        /// <summary>Présélectionne le candidat de test.</summary>
        public ValueTask<bool> CanReadAsync(
            DiskImageRecognitionContext context,
            CancellationToken cancellationToken) => ValueTask.FromResult(true);

        /// <summary>Tente la lecture Apple, qui doit rejeter le contenu invalide.</summary>
        public Task<SectorImage> ReadAsync(
            DiskImageRecognitionContext context,
            CancellationToken cancellationToken)
        {
            ReadCalls++;
            return new AppleDiskImageReader().ReadAsync(context.Path, cancellationToken);
        }
    }

    /// <summary>Politique suivante produisant une image connue après le rejet Apple.</summary>
    private sealed class AcceptedPolicy : IDiskImageRecognitionPolicy
    {
        /// <summary>Nombre de lectures de secours effectuées.</summary>
        public int ReadCalls { get; private set; }

        /// <summary>Accepte le candidat transmis par le registre.</summary>
        public ValueTask<bool> CanReadAsync(
            DiskImageRecognitionContext context,
            CancellationToken cancellationToken) => ValueTask.FromResult(true);

        /// <summary>Produit une image minimale prouvant que le registre a poursuivi la boucle.</summary>
        public Task<SectorImage> ReadAsync(
            DiskImageRecognitionContext context,
            CancellationToken cancellationToken)
        {
            ReadCalls++;
            return Task.FromResult(new SectorImage(
                "fallback",
                1,
                1,
                1,
                1,
                [new SectorBlock(0, new SectorAddress(0, 0, 1), new byte[] { 0x01 })]));
        }
    }
}

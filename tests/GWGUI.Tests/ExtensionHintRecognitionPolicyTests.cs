using System.IO;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Recognition.Policies;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

/// <summary>Vérifie la présélection par extension à travers le registre de reconnaissance.</summary>
public sealed class ExtensionHintRecognitionPolicyTests
{
    /// <summary>Vérifie qu'une extension enregistrée appelle une seule fois le Reader associé.</summary>
    [Fact]
    public async Task RegisteredExtensionCallsAssociatedReaderOnce()
    {
        var calls = 0;
        var path = await CreateImageAsync(".img");
        try
        {
            var registry = new DiskImageRecognitionRegistry([new ExtensionHintRecognitionPolicy((_, _) => { calls++; return Task.FromResult(CreateImage("selected")); }, ".img")]);
            Assert.Equal("selected", (await registry.ReadAsync(path, null, CancellationToken.None)).FormatId);
            Assert.Equal(1, calls);
        }
        finally { File.Delete(path); }
    }

    /// <summary>Vérifie qu'une extension absente n'appelle pas le Reader.</summary>
    [Fact]
    public async Task UnregisteredExtensionDoesNotCallReader()
    {
        var calls = 0;
        var path = await CreateImageAsync(".bin");
        try
        {
            var registry = new DiskImageRecognitionRegistry([new ExtensionHintRecognitionPolicy((_, _) => { calls++; return Task.FromResult(CreateImage("unexpected")); }, ".img")]);
            await Assert.ThrowsAsync<NotSupportedException>(() => registry.ReadAsync(path, null, CancellationToken.None));
            Assert.Equal(0, calls);
        }
        finally { File.Delete(path); }
    }

    /// <summary>Vérifie que deux extensions configurées présélectionnent le même Reader.</summary>
    [Theory]
    [InlineData(".ssd")]
    [InlineData(".dsd")]
    public async Task MultipleExtensionsSelectTheSameReader(string extension)
    {
        var path = await CreateImageAsync(extension);
        try
        {
            var registry = new DiskImageRecognitionRegistry([new ExtensionHintRecognitionPolicy((_, _) => Task.FromResult(CreateImage("bbc")), ".ssd", ".dsd")]);
            Assert.Equal("bbc", (await registry.ReadAsync(path, null, CancellationToken.None)).FormatId);
        }
        finally { File.Delete(path); }
    }

    /// <summary>Vérifie que la casse de l'indice ou du chemin ne change pas la présélection.</summary>
    [Fact]
    public async Task ExtensionComparisonIsCaseInsensitive()
    {
        var path = await CreateImageAsync(".SsD");
        try
        {
            var registry = new DiskImageRecognitionRegistry([new ExtensionHintRecognitionPolicy((_, _) => Task.FromResult(CreateImage("case-insensitive")), ".SSD")]);
            Assert.Equal("case-insensitive", (await registry.ReadAsync(path, null, CancellationToken.None)).FormatId);
        }
        finally { File.Delete(path); }
    }

    /// <summary>Vérifie qu'un Reader rejetant le contenu laisse le registre essayer le candidat suivant.</summary>
    [Fact]
    public async Task RejectedContentContinuesWithNextCandidate()
    {
        var path = await CreateImageAsync(".img");
        try
        {
            var rejected = new ExtensionHintRecognitionPolicy((_, _) => throw new InvalidDataException("rejected"), ".img");
            var accepted = new ExtensionHintRecognitionPolicy((_, _) => Task.FromResult(CreateImage("accepted")), ".img");
            var registry = new DiskImageRecognitionRegistry([rejected, accepted]);
            Assert.Equal("accepted", (await registry.ReadAsync(path, null, CancellationToken.None)).FormatId);
        }
        finally { File.Delete(path); }
    }

    /// <summary>Vérifie que la politique conserve sa propre copie des extensions reçues.</summary>
    [Fact]
    public async Task ConstructorCopiesExtensionCollection()
    {
        var extensions = new[] { ".img" };
        var policy = new ExtensionHintRecognitionPolicy((_, _) => Task.FromResult(CreateImage("copied")), extensions);
        extensions[0] = ".bin";
        var path = await CreateImageAsync(".img");
        try
        {
            var registry = new DiskImageRecognitionRegistry([policy]);
            Assert.Equal("copied", (await registry.ReadAsync(path, null, CancellationToken.None)).FormatId);
        }
        finally { File.Delete(path); }
    }

    /// <summary>Crée une image sectorielle minimale retournée par les Readers instrumentés.</summary>
    private static SectorImage CreateImage(string formatId) => new(formatId, 1, 1, 1, 1, [new SectorBlock(0, new SectorAddress(0, 0, 1), [0x42])]);

    /// <summary>Crée un fichier temporaire portant l'extension demandée.</summary>
    private static async Task<string> CreateImageAsync(string extension)
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-extension-policy-{Guid.NewGuid():N}{extension}");
        await File.WriteAllBytesAsync(path, [0x42]);
        return path;
    }
}

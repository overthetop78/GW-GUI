using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Exploration;
using GWGUI.MediaEngine.Exploration.Scp;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using GWGUI.MediaEngine.Recognition;
using System.IO;

namespace GWGUI.Tests;

/// <summary>VÃ©rifie les politiques de reconnaissance par lâ€™API publique du moteur.</summary>
public sealed class DiskImageRecognitionPolicyTests
{
    [Theory]
    [MemberData(nameof(RecognizedImages))]
    public async Task PublicRecognitionSelectsThePolicyMatchingTheImageContent(
        string relativePath,
        string expectedFormatId)
    {
        var path = Path.Combine(FindImageTestRoot(), relativePath);

        var result = await DiskImageExplorer.CreateDefault().ExploreAsync(path);

        Assert.Equal(expectedFormatId, result.Image.FormatId);
    }

    [Fact]
    public async Task ScpSignatureIsRecognizedWithAnUnusualExtension()
    {
        var source = Path.Combine(FindImageTestRoot(), "validated_images", "MSX", "MSX",
            "3.5 pouces - MSX-DOS FAT12 - 720 Kio", "seeds-of-evil-msx [test].scp");
        var destination = Path.Combine(Path.GetTempPath(), $"gwgui-scp-signature-{Guid.NewGuid():N}.media");
        File.Copy(source, destination);
        try
        {
            var result = await DiskImageExplorer.CreateDefault().ExploreAsync(destination, DiskImageFormatIds.Msx2Dd);

            Assert.Equal(DiskImageFormatIds.Msx2Dd, result.Image.FormatId);
        }
        finally
        {
            File.Delete(destination);
        }
    }

    [Fact]
    public async Task ScpExtensionWithoutScpSignatureIsNotRecognizedAsScp()
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-invalid-scp-{Guid.NewGuid():N}.scp");
        await File.WriteAllBytesAsync(path, "not an SCP container"u8.ToArray());
        try
        {
            var result = await DiskImageExplorer.CreateDefault().ExploreAsync(path, DiskImageFormatIds.Msx2Dd);

            Assert.Equal(DiskImageFormatIds.Unknown, result.Image.FormatId);
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>VÃ©rifie qu'une extension 86F ou CP2 ne transforme pas un contenu rejetÃ© en image reconnue.</summary>
    [Theory]
    [InlineData(".86f")]
    [InlineData(".cp2")]
    public async Task ExtensionHintDoesNotBypassReaderValidation(string extension)
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-invalid-container-{Guid.NewGuid():N}{extension}");
        await File.WriteAllBytesAsync(path, "invalid container"u8.ToArray());
        try
        {
            var exception = await Assert.ThrowsAsync<DiskImageCandidatesRejectedException>(() => DiskImageExplorer.CreateDefault().ExploreAsync(path));
            Assert.Single(exception.Failures);
            Assert.IsType<InvalidDataException>(exception.Failures[0].Exception);
        }
        finally { File.Delete(path); }
    }

    public static TheoryData<string, string> RecognizedImages => new()
    {
        {
            "adfs_ArchimedesWorld_199211.adf",
            DiskImageFormatIds.AcornAdfs800
        },
        {
            Path.Combine("_generated", "cpcdsk", "variable-sectors-and-integrity.edsk"),
            DiskImageFormatIds.AmstradCpc
        },
        {
            Path.Combine("_generated", "apple", "containers", "dos-order.2mg"),
            DiskImageFormatIds.AppleIIDos33
        },
        {
            Path.Combine("COHERENT - ordinateur à identifier", "COHERENT - Volume 1 - Low Resolution.bin"),
            DiskImageFormatIds.Commodore900Coherent
        },
        {
            Path.Combine("validated_images", "DEC", "MINC", "8 pouces - RX02 - DEC RT-11 - 500 Kio",
                "BA_J836B-BC_MINC_MA_SYS_23_V2.0_BIN_RX2.img"),
            DiskImageFormatIds.DecRx02
        },
        {
            Path.Combine("validated_images", "MSX", "MSX", "3.5 pouces - MSX-DOS FAT12 - 720 Kio",
                "seeds-of-evil-msx.dsk"),
            DiskImageFormatIds.Msx2Dd
        },
        {
            Path.Combine("IBM PC", "PFS Write C00 (1985) (5.25-360k) disk01.cp2"),
            DiskImageFormatIds.Ibm360
        }
    };

    private static string FindImageTestRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "image_test");
            if (Directory.Exists(candidate)) return candidate;
        }
        throw new DirectoryNotFoundException("Le dossier local image_test est introuvable.");
    }
}

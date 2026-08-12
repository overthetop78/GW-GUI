using System.IO;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Reconstruction.Atari;

namespace GWGUI.Tests;

/// <summary>Vérifie le routage public des captures SCP Atari.</summary>
public sealed class AtariScpSectorImageReaderTests
{
    /// <summary>Vérifie une lecture Atari 8 bits explicitement demandée.</summary>
    [Fact]
    public async Task ReadsExplicitAtari8BitImage() => Assert.Equal(DiskImageFormatIds.Atari90, (await Reader().ReadAsync(ImagePath("Atari", "Atari 8-bit", "5.25 pouces - Atari DOS - 90 Kio", "Nibbler [test].scp"), DiskImageFormatIds.Atari90)).FormatId);

    /// <summary>Vérifie une lecture Atari ST explicitement demandée.</summary>
    [Fact]
    public async Task ReadsExplicitAtariStImage() => Assert.Equal(DiskImageFormatIds.AtariSt720, (await Reader().ReadAsync(ImagePath("Atari", "Atari ST", "3.5 pouces - Atari TOS FAT12 - 720 Kio", "seeds-of-evil-atari-st [test].scp"), DiskImageFormatIds.AtariSt720)).FormatId);

    /// <summary>Vérifie la sélection automatique d'une capture Atari.</summary>
    [Fact]
    public async Task ReadsAtariImageAutomatically() => Assert.StartsWith(DiskImageFormatIds.AtariPrefix, (await Reader().ReadAsync(ImagePath("Atari", "Atari 8-bit", "5.25 pouces - Atari DOS - 90 Kio", "Nibbler [test].scp"))).FormatId, StringComparison.Ordinal);

    /// <summary>Vérifie le refus d'un identifiant extérieur aux familles Atari.</summary>
    [Fact]
    public async Task RejectsNonAtariFormat() => await Assert.ThrowsAsync<ArgumentException>(() => Reader().ReadAsync("unused.scp", DiskImageFormatIds.AmigaDos));

    private static AtariScpSectorImageReader Reader() => new(new ScpReader(), new FluxDecoderRegistry());

    private static string ImagePath(params string[] parts)
    {
        var path = Path.GetFullPath(Path.Combine([AppContext.BaseDirectory, "..", "..", "..", "..", "..", "image_test", "validated_images", .. parts]));
        Assert.True(File.Exists(path), $"Image SCP Atari obligatoire absente : {path}");
        return path;
    }
}

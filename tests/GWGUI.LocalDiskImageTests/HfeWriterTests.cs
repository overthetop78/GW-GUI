using GWGUI.MediaEngine.Composition;
using GWGUI.MediaEngine.Containers.Acorn.BbcDfs;
using GWGUI.MediaEngine.Containers.Atari.St;
using GWGUI.MediaEngine.Containers.Hfe;
using GWGUI.MediaEngine.Conversion.Hfe;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using GWGUI.MediaEngine.SectorImages;
using System.Buffers.Binary;
using System.IO;

namespace GWGUI.Tests;

/// <summary>Vérifie l'écriture HFE version 1 et le redécodage de ses pistes FM/MFM.</summary>
public sealed class HfeWriterTests
{
    [Theory]
    [InlineData("Atari", "Atari ST", "3.5 pouces - Atari TOS FAT12 - 720 Kio", "seeds-of-evil-atari-st.st", FluxCodecIds.IsoMfm, HfeFormat.IsoMfmEncoding)]
    [InlineData("Acorn", "BBC Micro", "5.25 pouces - Acorn DFS - 200 Kio", "seeds-of-evil-bbc.ssd", FluxCodecIds.IsoFm, HfeFormat.IsoFmEncoding)]
    public async Task ConversionWritesAValidTrackTableAndPreservesEverySector(string family, string machine, string directory, string fileName, string codecId, byte encoding)
    {
        var sourcePath = ImagePath(family, machine, directory, fileName);
        var outputPath = Path.Combine(Path.GetTempPath(), $"gwgui-{Guid.NewGuid():N}.hfe");
        try
        {
            Assert.True(HfeConversionService.CanCreate(DiskImageFormatIds.RawHfe, DiskImageFileExtensions.Hfe));
            await MediaEngineFactory.CreateHfeConversionService().ConvertAsync(sourcePath, outputPath);
            var bytes = await File.ReadAllBytesAsync(outputPath);
            Assert.True(bytes.AsSpan(HfeLayout.SignatureOffset, HfeLayout.SignatureLength).SequenceEqual(HfeFormat.Signature));
            Assert.Equal(HfeFormat.Revision, bytes[HfeLayout.RevisionOffset]);
            Assert.Equal(encoding, bytes[HfeLayout.EncodingOffset]);
            Assert.Equal(1, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(HfeLayout.TrackListOffset)));
            var hfe = await new HfeReader().ReadAsync(outputPath);
            Assert.Equal(encoding, hfe.Encoding);
            Assert.Equal(hfe.Cylinders * hfe.Heads, hfe.Tracks.Count);
            var source = fileName.EndsWith(DiskImageFileExtensions.St, StringComparison.OrdinalIgnoreCase) ? await new AtariStReader().ReadAsync(sourcePath) : await new BbcDfsReader().ReadAsync(sourcePath);
            AssertSectorsEqual(source, hfe, codecId);
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    private static void AssertSectorsEqual(SectorImage source, HfeImage hfe, string codecId)
    {
        var decoder = new FluxDecoderRegistry();
        var decoded = hfe.Tracks.SelectMany(track => decoder.Decode(codecId, track.Revolution).Sectors.Where(sector => sector.Data is not null && sector.IntegrityValid == true).Select(sector => (Address: new SectorAddress(sector.Cylinder, sector.Head, sector.Number), Data: sector.Data!.ToArray()))).GroupBy(item => item.Address).ToDictionary(group => group.Key, group => group.First().Data);
        foreach (var block in source.AvailableBlocks)
        {
            Assert.True(decoded.TryGetValue(block.Address, out var data), $"Secteur HFE absent : {block.Address}.");
            Assert.Equal(block.Data, data);
        }
    }

    private static string ImagePath(string family, string machine, string directory, string fileName) => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "image_test", "validated_images", family, machine, directory, fileName));
}

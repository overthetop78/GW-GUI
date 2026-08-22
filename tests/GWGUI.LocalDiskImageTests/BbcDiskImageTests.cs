using GWGUI.MediaEngine.Containers.Acorn.BbcDfs;
using GWGUI.MediaEngine.Containers.ImageDisk;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Exploration;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.Geometries.Acorn;
using GWGUI.MediaEngine.Reconstruction.Iso;
using GWGUI.MediaEngine.SectorImages;
using System.IO;

namespace GWGUI.Tests;

public sealed class BbcDiskImageTests
{
    /// <summary>VÃ©rifie la sÃ©lection automatique des quatre gÃ©omÃ©tries BBC DFS sans identifiant demandÃ©.</summary>
    [Theory]
    [InlineData(40, 1, "acorn.dfs.ss")]
    [InlineData(80, 1, "acorn.dfs.ss80")]
    [InlineData(40, 2, "acorn.dfs.ds")]
    [InlineData(80, 2, "acorn.dfs.ds80")]
    public void AutomaticallySelectsEveryBbcDfsGeometry(int cylinders, int heads, string formatId)
    {
        var address = new SectorAddress(cylinders - 1, heads - 1, 0);
        var sector = new DecodedSector(checked((byte)address.Cylinder), checked((byte)address.Head), address.Number, 1, BbcDfsGeometry.SectorSize, true, 0, Data: new byte[BbcDfsGeometry.SectorSize]);
        var candidates = new Dictionary<SectorAddress, List<IsoSectorCandidate>> { [address] = [new(sector, 1)] };

        var image = new BbcIsoScpSectorImagePolicy().Build(null, new(candidates, candidates));

        Assert.Equal(formatId, image.FormatId);
        Assert.Equal(cylinders, image.Cylinders);
        Assert.Equal(heads, image.Heads);
    }

    /// <summary>VÃ©rifie les quatre capacitÃ©s SSD/DSD, leurs formats et l'ordre des faces sur plusieurs cylindres.</summary>
    [Theory]
    [InlineData(".ssd", 40, 1, "acorn.dfs.ss")]
    [InlineData(".ssd", 80, 1, "acorn.dfs.ss80")]
    [InlineData(".dsd", 40, 2, "acorn.dfs.ds")]
    [InlineData(".dsd", 80, 2, "acorn.dfs.ds80")]
    public async Task ReadsEverySsdAndDsdGeometry(string extension, int cylinders, int heads, string formatId)
    {
        var data = new byte[cylinders * heads * BbcDfsGeometry.TrackSize];
        for (var cylinder = 0; cylinder < cylinders; cylinder++)
            for (var head = 0; head < heads; head++)
                for (var sector = 0; sector < BbcDfsGeometry.SectorsPerTrack; sector++) data[BbcDfsLayout.SourceOffset(cylinder, head, sector, heads)] = checked((byte)(cylinder + head + sector));
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-bbc-{Guid.NewGuid():N}{extension}");
        try
        {
            await File.WriteAllBytesAsync(path, data);
            var image = await new BbcDfsReader().ReadAsync(path);
            Assert.Equal(formatId, image.FormatId);
            Assert.Equal(cylinders, image.Cylinders);
            Assert.Equal(heads, image.Heads);
            Assert.Equal(data.Length, image.Capacity);
            foreach (var cylinder in new[] { 0, cylinders - 1 })
                for (var head = 0; head < heads; head++)
                {
                    var logical = (cylinder * heads + head) * BbcDfsGeometry.SectorsPerTrack;
                    var block = image.AvailableBlocks.Single(candidate => candidate.LogicalBlock == logical);
                    Assert.Equal(new(cylinder, head, 0), block.Address);
                    Assert.Equal(data[BbcDfsLayout.SourceOffset(cylinder, head, 0, heads)], block.Data[0]);
                }
        }
        finally { File.Delete(path); }
    }

    /// <summary>VÃ©rifie une piste tronquÃ©e, 41/79 cylindres et une extension inconnue.</summary>
    [Fact]
    public async Task RejectsInvalidBbcContainers()
    {
        await AssertRejected(new byte[BbcDfsGeometry.TrackSize - 1], ".ssd", typeof(InvalidDataException));
        await AssertRejected(new byte[41 * BbcDfsGeometry.TrackSize], ".ssd", typeof(InvalidDataException));
        await AssertRejected(new byte[79 * BbcDfsGeometry.TrackSize], ".ssd", typeof(InvalidDataException));
        await AssertRejected(new byte[40 * BbcDfsGeometry.TrackSize], ".bad", typeof(NotSupportedException));
    }

    [Theory]
    [InlineData("Acorn BBC - Applications - [SSD] (TOSEC-v2013-10-16)/Cheat it Again Joe - Vol. 1 (1988)(Impact Posters)/Cheat it Again Joe - Vol. 1 (1988)(Impact Posters).ssd", 40, 1)]
    [InlineData("validated_images/Acorn/BBC Micro/5.25 pouces - Acorn DFS - 200 Kio/seeds-of-evil-bbc.ssd", 80, 1)]
    [InlineData("Acorn BBC - Educational - [DSD] (TOSEC-v2013-10-22)/CeeFAX ASM Notes (19xx)(-)[h 8-Bit]/CeeFAX ASM Notes (19xx)(-)[h 8-Bit].dsd", 40, 2)]
    [InlineData("Acorn BBC - Multimedia - [DSD] (TOSEC-v2013-10-22)/8004 Words (19xx)(8-Bit)/8004 Words (19xx)(8-Bit).dsd", 80, 2)]
    public async Task ReadsKnownImageTestCorpusGeometry(string relativePath, int cylinders, int heads)
    {
        var path = Path.Combine(TestRoot(), relativePath.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(path), $"Image BBC DFS obligatoire absente : {path}");
        var image = await new BbcDfsReader().ReadAsync(path);
        var document = await DiskImageExplorer.CreateDefault().ExploreAsync(path);
        Assert.Equal(cylinders, image.Cylinders);
        Assert.Equal(heads, image.Heads);
        Assert.Equal(cylinders * heads * BbcDfsGeometry.SectorsPerTrack, image.AvailableBlocks.Count);
        Assert.True(document.FileSystemRecognized);
        Assert.NotEmpty(document.Volume.Entries);
    }

    [Fact]
    public async Task RealBbcSsdExposesItsDfsCatalogue()
    {
        var path = Path.Combine(TestRoot(), "BBC Micro", "seeds-of-evil-bbc.ssd");
        if (!File.Exists(path)) return;
        var document = await DiskImageExplorer.CreateDefault().ExploreAsync(path);
        Assert.True(document.FileSystemRecognized);
        Assert.Equal(GWGUI.MediaEngine.FileSystems.Definitions.FileSystemIds.AcornDfs, document.Volume.FileSystemId);
        Assert.Equal("The Seeds of", document.Volume.Name);
        Assert.Equal(204_800, document.Volume.Capacity);
        Assert.Contains(document.Volume.Entries, entry => entry.Name == "BUILD" && entry.Size > 0);
        Assert.Contains(document.Volume.Entries, entry => entry.Name == "!BOOT" && entry.Size > 0);
        Assert.All(document.Volume.Entries, entry => Assert.False(string.IsNullOrWhiteSpace(entry.Name)));
    }

    private static string TestRoot() => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "image_test"));

    /// <summary>Ã‰crit un conteneur temporaire et vÃ©rifie le type exact de son rejet.</summary>
    private static async Task AssertRejected(byte[] data, string extension, Type exceptionType)
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-bbc-{Guid.NewGuid():N}{extension}");
        try { await File.WriteAllBytesAsync(path, data); var exception = await Record.ExceptionAsync(() => new BbcDfsReader().ReadAsync(path)); Assert.NotNull(exception); Assert.IsType(exceptionType, exception); }
        finally { File.Delete(path); }
    }
}

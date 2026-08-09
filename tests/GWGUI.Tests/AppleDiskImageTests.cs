using System.IO;
using GWGUI.Scp;
using GWGUI.Scp.FileSystems;
using GWGUI.Scp.FileSystems.Readers;
using GWGUI.Scp.Images;
using GWGUI.Scp.SectorImages;
using GWGUI.Scp.Decoding;
using GWGUI.Scp.Encoding;
using GWGUI.Domain.Formats;
using GWGUI.Domain.Write;
using GWGUI.App.Controls;
using GWGUI.App.Localization;

namespace GWGUI.Tests;

public sealed class AppleDiskImageTests
{
    [Fact]
    public void DiskMetadataReportsAppleSystemAndRwts18Protection()
    {
        var protectedImage = new SectorImage("apple2.rwts18", 768, 35, 1, 6, []);
        var ordinaryImage = new SectorImage("apple2.dos33", 256, 35, 1, 16, []);

        Assert.Equal(new DiskImageMetadata("Apple II", "Brøderbund RWTS18"), DiskImageMetadata.From(protectedImage));
        Assert.Equal(new DiskImageMetadata("Apple II", null), DiskImageMetadata.From(ordinaryImage));
    }

    [Fact]
    public void DecodedPhysicalAppleDiskDoesNotPresentItsFileSystemAsUnknown()
    {
        var image = new SectorImage("apple2.dos33", 256, 35, 1, 16, []);
        var document = new ExploredDiskImage("airheart.scp", image,
            new FileSystemVolume("", "apple2.dos33", 143_360, 0, null, null, [], []), false,
            DetectedImageFormatIds: ["apple2.dos33"]);

        Assert.Equal("Apple II", document.Metadata.SystemName);
        Assert.NotEqual(LocExtension.Get("Explorer.Unknown"), ExplorerDetailsPresenter.FileSystemText(document));
        Assert.Equal(LocExtension.Get("Explorer.PhysicalSectorsNoFileSystem"), ExplorerDetailsPresenter.FileSystemText(document));
    }

    [Fact]
    public void SharedCatalogContainsAppleFormatsAndDetectsAppleContainers()
    {
        var catalog = new BuiltInImageFormatCatalog();
        Assert.Contains(catalog.Formats, format => format.Id == "apple2.appledos.113");
        Assert.Contains(catalog.Formats, format => format.Id == "apple2.appledos.140");
        Assert.Contains(catalog.Formats, format => format.Id == "apple2.prodos.140");
        Assert.DoesNotContain(catalog.Formats, format => format.Id == "apple2.rwts18");
        var classifications = new DiskClassificationCatalog(catalog.Formats);
        var dos33 = Assert.IsType<DiskFormat>(classifications.ResolveFormat("apple2.rwts18"));
        Assert.Equal("apple2.appledos.140", dos33.Id);
        var protection = Assert.Single(classifications.ProtectionsFor("Apple II", dos33.Id));
        Assert.Equal("apple2.rwts18", protection.Id);
        Assert.Contains(catalog.Formats, format => format.Id == "apple3.sos");
        Assert.Contains(catalog.Formats, format => format.Id == "mac.400");
        Assert.Contains(catalog.Formats, format => format.Id == "mac.800");
        Assert.Contains(catalog.Formats, format => format.Id == "mac.1440");
        var detector = new ImageFormatDetector(catalog);
        Assert.Equal("apple2.appledos.113", detector.Detect("disk.d13", 116_480).Format?.Id);
        Assert.Equal("apple2.appledos.140", detector.Detect("disk.do", 143_360).Format?.Id);
        Assert.Equal("apple2.prodos.140", detector.Detect("disk.po", 143_360).Format?.Id);
        Assert.Equal("mac.400", detector.Detect("disk.image", 419_284).Format?.Id);
        Assert.Equal("mac.400", detector.Detect("disk.dc42", 419_284).Format?.Id);
        Assert.Equal("apple2.prodos.140", GwFormatArgument.FromCatalogId("apple3.sos"));
        Assert.Equal("ibm.1440", GwFormatArgument.FromCatalogId("mac.1440"));
    }

    [Fact]
    public async Task Dos32RawImageUsesThirteenSectorGeometry()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.d13");
        try
        {
            await File.WriteAllBytesAsync(path, new byte[35 * 13 * 256]);
            var image = await new AppleDiskImageReader().ReadAsync(path);
            Assert.Equal("apple2.dos32", image.FormatId);
            Assert.Equal(35 * 13, image.AvailableBlocks.Count);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task Dos32ScpUsesThirteenSectorGeometry()
    {
        var sectors = Enumerable.Range(0, 13)
            .Select(number => new TrackSector(number, Enumerable.Range(0, 256).Select(index => (byte)(number + index)).ToArray()))
            .ToArray();
        var encoded = new FluxEncoderRegistry().Encode("apple2.gcr",
            new TrackEncodeRequest(0, 0, sectors, Attributes: new Dictionary<string, int> { ["sectorsPerTrack"] = 13 }));
        var scp = new ScpImage(new(0, 0, 1, 0, 0, ScpFlags.IndexAligned, 16, 0, 0, 0),
            [new ScpTrack(0, 0, 0, [encoded.Revolution])], true, 0);

        var image = await new AppleScpSectorImageReader(new MemoryScpReader(scp), new FluxDecoderRegistry()).ReadAsync("memory.scp");

        Assert.Equal("apple2.dos32", image.FormatId);
        Assert.Equal(13, image.SectorsPerTrack);
        Assert.Equal(13, image.AvailableBlocks.Count);
    }

    [Fact]
    public async Task Rwts18ScpUsesSixPhysicalSectorsOfThreePages()
    {
        var sectors = Enumerable.Range(0, 6)
            .Select(number => new TrackSector(number, Enumerable.Range(0, 768)
                .Select(index => (byte)(number * 31 + index * 17)).ToArray()))
            .ToArray();
        var encoded = new FluxEncoderRegistry().Encode("apple2.rwts18", new TrackEncodeRequest(4, 0, sectors));
        var scp = new ScpImage(new(0, 0, 1, 0, 0, ScpFlags.IndexAligned, 16, 0, 0, 0),
            [new ScpTrack(8, 4, 0, [encoded.Revolution])], true, 0);

        var image = await new AppleScpSectorImageReader(new MemoryScpReader(scp), new FluxDecoderRegistry())
            .ReadAsync("memory.scp", "apple2.rwts18");

        Assert.Equal("apple2.rwts18", image.FormatId);
        Assert.Equal(768, image.BlockSize);
        Assert.Equal(6, image.SectorsPerTrack);
        Assert.Equal(6, image.AvailableBlocks.Count);
        foreach (var expected in sectors)
            Assert.Equal(expected.Data, image.GetBlock(4 * 6 + expected.Number).ToArray());
    }

    [Theory]
    [InlineData(".nib")]
    [InlineData(".woz")]
    public async Task Rwts18UsesRealAppleNibbleContainers(string extension)
    {
        var blocks = Enumerable.Range(0, 35).SelectMany(track => Enumerable.Range(0, 6)
            .Select(sector => new SectorBlock(track * 6 + sector, new(track, 0, sector),
                Enumerable.Range(0, 768).Select(index => (byte)(track * 7 + sector * 31 + index * 17)).ToArray())))
            .ToArray();
        var source = new SectorImage("apple2.rwts18", 768, 35, 1, 6, blocks);
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}{extension}");
        try
        {
            await new AppleNibbleImageWriter().WriteAsync(source, path);
            var decoded = await new AppleDiskImageReader().ReadAsync(path);
            Assert.Equal("apple2.rwts18", decoded.FormatId);
            Assert.Equal(35 * 6, decoded.AvailableBlocks.Count);
            foreach (var block in blocks) Assert.Equal(block.Data, decoded.GetBlock(block.LogicalBlock).ToArray());
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task ConversionServiceDecodesNibAndReencodesWoz()
    {
        var blocks = Enumerable.Range(0, 35).SelectMany(track => Enumerable.Range(0, 6)
            .Select(sector => new SectorBlock(track * 6 + sector, new(track, 0, sector),
                Enumerable.Repeat((byte)(track + sector), 768).ToArray())))
            .ToArray();
        var image = new SectorImage("apple2.rwts18", 768, 35, 1, 6, blocks);
        var source = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.nib");
        var output = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.woz");
        try
        {
            await new AppleNibbleImageWriter().WriteAsync(image, source);
            await new AppleRwts18ConversionService().ConvertAsync(source, output);
            var decoded = await new AppleDiskImageReader().ReadAsync(output);
            Assert.Equal("apple2.rwts18", decoded.FormatId);
            Assert.Equal(blocks.Length, decoded.AvailableBlocks.Count);
        }
        finally { File.Delete(source); File.Delete(output); }
    }

    [Fact]
    public async Task Dos33ImageExposesItsCatalog()
    {
        var data = new byte[35 * 16 * 256];
        var vtoc = (17 * 16 + 0) * 256;
        data[vtoc + 1] = 17; data[vtoc + 2] = 15; data[vtoc + 0x34] = 35; data[vtoc + 0x35] = 16;
        data[vtoc + 0x36] = 0; data[vtoc + 0x37] = 1;
        var catalog = (17 * 16 + 15) * 256;
        data[catalog + 1] = 0; data[catalog + 2] = 0;
        data[catalog + 0x0b] = 1; data[catalog + 0x0c] = 0; data[catalog + 0x0d] = 0x04;
        System.Text.Encoding.ASCII.GetBytes("HELLO").CopyTo(data, catalog + 0x0e);
        for (var index = catalog + 0x13; index < catalog + 0x2c; index++) data[index] = 0xa0;
        data[catalog + 0x2c] = 1;

        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.dsk");
        try
        {
            await File.WriteAllBytesAsync(path, data);
            var image = await new AppleDiskImageReader().ReadAsync(path);
            Assert.Equal("apple2.dos33", image.FormatId);
            var volume = new FileSystemRegistry().Read(image);
            Assert.Equal("Apple DOS 3.3", volume.FileSystem);
            Assert.Contains(volume.Entries, entry => entry.Name == "HELLO");
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void AppleInformXzipImageExposesInterpreterAndStory()
    {
        const int storyLength = 65_536;
        var story = new byte[394 * 256];
        story[0] = 5;
        story[0x04] = 0x01; story[0x05] = 0x00;
        story[0x06] = 0x01; story[0x07] = 0x20;
        story[0x08] = 0x02; story[0x09] = 0x00;
        story[0x0a] = 0x03; story[0x0b] = 0x00;
        story[0x0c] = 0x04; story[0x0d] = 0x00;
        story[0x0e] = 0x05; story[0x0f] = 0x00;
        story[0x1a] = 0x40; story[0x1b] = 0x00;
        for (var index = 0x40; index < storyLength; index++) story[index] = (byte)(index * 17 + 3);
        var checksum = story.Skip(0x40).Take(storyLength - 0x40).Aggregate(0, (sum, value) => (sum + value) & 0xffff);
        story[0x1c] = (byte)(checksum >> 8); story[0x1d] = (byte)checksum;

        int[] interleave = [0, 7, 14, 6, 13, 5, 12, 4, 11, 3, 10, 2, 9, 1, 8, 15];
        var disk = new byte[35 * 16 * 256];
        for (var sector = 0; sector < 64; sector++)
            Array.Fill(disk, (byte)0xa5, sector * 256, 256);
        for (var sector = 0; sector < 394; sector++)
        {
            var stored = 64 + (sector & 0xff0) + Array.IndexOf(interleave, sector & 15);
            Array.Copy(story, sector * 256, disk, stored * 256, 256);
        }

        var blocks = Enumerable.Range(0, 35 * 16)
            .Select(logical => new SectorBlock(logical, new(logical / 16, 0, logical % 16),
                disk.AsSpan(logical * 256, 256).ToArray()))
            .ToArray();
        var image = new SectorImage("apple2.dos33", 256, 35, 1, 16, blocks);

        var volume = new FileSystemRegistry().Read(image);

        Assert.Equal("Apple II Inform/XZIP", volume.FileSystem);
        Assert.Equal(2, volume.Entries.Count);
        Assert.Equal(16_384, volume.Entries.Single(entry => entry.Name == "INTERPRETER.BIN").Size);
        var extractedStory = volume.Entries.Single(entry => entry.Name == "STORY.Z5");
        Assert.Equal(storyLength, extractedStory.Size);
        Assert.Equal(story.Take(storyLength), extractedStory.Content);
    }

    [Fact]
    public async Task RealAppleCorpusIsReadableWhenRequested()
    {
        var root = Environment.GetEnvironmentVariable("GWGUI_APPLE_CORPUS");
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root)) return;

        var explorer = DiskImageExplorer.CreateDefault();
        var appleRoots = Directory.EnumerateDirectories(root, "Apple *", SearchOption.TopDirectoryOnly).ToArray();
        var supported = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".d13", ".dsk", ".do", ".po", ".2mg", ".image", ".dc42", ".nib", ".woz" };
        var requestedFile = Environment.GetEnvironmentVariable("GWGUI_APPLE_FILE");
        var results = new List<string>();
        var failures = new List<string>();
        var recognizedNibbleImages = 0;
        var paths = appleRoots.SelectMany(directory => Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories))
                     .Where(path => supported.Contains(Path.GetExtension(path)))
                     .Where(path => string.IsNullOrWhiteSpace(requestedFile) || path.Contains(requestedFile, StringComparison.OrdinalIgnoreCase))
                     .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}_generated{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
                     .ToArray();
        foreach (var path in paths)
        {
            try
            {
                var explicitFormat = Path.GetExtension(path).Equals(".dsk", StringComparison.OrdinalIgnoreCase)
                    && path.Contains($"{Path.DirectorySeparatorChar}Apple II{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                    ? "apple2.dos33" : null;
                var document = await explorer.ExploreAsync(path, explicitFormat);
                var relative = Path.GetRelativePath(root, path);
                var rawNibbleContainer = Path.GetExtension(path) is ".woz" or ".nib";
                if (!document.FileSystemRecognized && document.Volume.Entries.Count == 0)
                    failures.Add($"NO DECODABLE STRUCTURE {relative}: {document.Image.FormatId}");
                else if (!document.FileSystemRecognized)
                    results.Add($"OPEN PHYSICAL STRUCTURE {relative}: {document.Image.FormatId}, {document.Volume.Entries.Count} tracks");
                else
                {
                    if (rawNibbleContainer) recognizedNibbleImages++;
                    results.Add($"OPEN {Path.GetRelativePath(root, path)}: {document.Volume.FileSystem}, {document.Volume.Entries.Count} root entries");
                }
            }
            catch (Exception exception)
            {
                failures.Add($"FAIL {Path.GetRelativePath(root, path)}: {exception.GetType().Name}: {exception.Message}");
            }
        }

        foreach (var result in results.Concat(failures)) Console.WriteLine(result);
        Assert.NotEmpty(results);
        if (paths.Any(path => Path.GetExtension(path) is ".woz" or ".nib"))
            Assert.True(recognizedNibbleImages > 0, "No filesystem was decoded from the WOZ/NIB corpus.");
        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public async Task RealSingleAppleDos33ImageAndFluxRemainEquivalentWhenRequested()
    {
        var dskPath = Environment.GetEnvironmentVariable("GWGUI_REAL_APPLE_DOS33_DSK");
        var scpPath = Environment.GetEnvironmentVariable("GWGUI_REAL_APPLE_DOS33_SCP");
        if (string.IsNullOrWhiteSpace(dskPath) || string.IsNullOrWhiteSpace(scpPath)) return;

        var explorer = DiskImageExplorer.CreateDefault();
        var source = await explorer.ExploreAsync(dskPath, "apple2.dos33");
        var flux = await explorer.ExploreAsync(scpPath);

        Assert.Equal(FlattenBlocks(source.Image.AvailableBlocks), FlattenBlocks(flux.Image.AvailableBlocks));
        Assert.Equal(source.FileSystemRecognized, flux.FileSystemRecognized);
        if (source.FileSystemRecognized)
        {
            Assert.Equal(source.Volume.Name, flux.Volume.Name);
            Assert.Equal(source.Volume.FileSystem, flux.Volume.FileSystem);
            Assert.Equal(source.Volume.Capacity, flux.Volume.Capacity);
            Assert.Equal(source.Volume.FreeBytes, flux.Volume.FreeBytes);
            Assert.Equal(Flatten(source.Volume.Entries), Flatten(flux.Volume.Entries));
            Assert.Equal(source.Volume.Warnings, flux.Volume.Warnings);
        }

        var visualization = new SectorImageFluxVisualizer().Create(source.Image);
        Assert.NotEmpty(visualization.Tracks);
        Assert.All(visualization.Tracks, track => Assert.Equal(0, track.Head));
    }

    [Fact]
    public async Task RealSingleAppleImageAndFluxRemainEquivalentWhenRequested()
    {
        var sourcePath = Environment.GetEnvironmentVariable("GWGUI_REAL_APPLE_SOURCE");
        var scpPath = Environment.GetEnvironmentVariable("GWGUI_REAL_APPLE_SCP");
        if (string.IsNullOrWhiteSpace(sourcePath) || string.IsNullOrWhiteSpace(scpPath)) return;

        var explorer = DiskImageExplorer.CreateDefault();
        var source = await explorer.ExploreAsync(sourcePath);
        var flux = await explorer.ExploreAsync(scpPath);

        Assert.Equal(source.Image.FormatId, flux.Image.FormatId);
        Assert.Equal(FlattenBlocks(source.Image.AvailableBlocks), FlattenBlocks(flux.Image.AvailableBlocks));
        Assert.Equal(source.FileSystemRecognized, flux.FileSystemRecognized);
        if (source.FileSystemRecognized)
        {
            Assert.Equal(source.Volume.Name, flux.Volume.Name);
            Assert.Equal(source.Volume.FileSystem, flux.Volume.FileSystem);
            Assert.Equal(source.Volume.Capacity, flux.Volume.Capacity);
            Assert.Equal(source.Volume.FreeBytes, flux.Volume.FreeBytes);
            Assert.Equal(Flatten(source.Volume.Entries), Flatten(flux.Volume.Entries));
            Assert.Equal(source.Volume.Warnings, flux.Volume.Warnings);
        }

        Assert.NotEmpty(new SectorImageFluxVisualizer().Create(source.Image).Tracks);
        Assert.NotEmpty(new SectorImageFluxVisualizer().Create(flux.Image).Tracks);
    }

    [Fact]
    public async Task MacWorksTaggedBootDiskIsNotMisidentifiedAsLisaOffice()
    {
        var root = Environment.GetEnvironmentVariable("GWGUI_APPLE_CORPUS");
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root)) return;
        var path = Directory.EnumerateFiles(root, "*MacWorks*.image", SearchOption.AllDirectories).FirstOrDefault();
        if (path is null) return;

        var image = await new AppleDiskImageReader().ReadAsync(path);

        Assert.Equal("applelisa.macworks", image.FormatId);
        Assert.Equal(800, image.AvailableBlocks.Count);
        Assert.False(new LisaFileSystemReader().CanRead(image));
        Assert.NotEmpty(new SectorImageFluxVisualizer().Create(image).Tracks);
        Assert.Equal("Apple Lisa", DiskImageMetadata.From(image).SystemName);

        var scpPath = Directory.EnumerateFiles(root, "*MacWorks*.scp", SearchOption.AllDirectories).FirstOrDefault();
        if (scpPath is null) return;
        var scp = await DiskImageExplorer.CreateDefault().ExploreAsync(scpPath);
        Assert.Equal("applelisa.macworks", scp.Image.FormatId);
        Assert.Equal("Apple Lisa", scp.Metadata.SystemName);
        Assert.False(scp.FileSystemRecognized);
        Assert.NotEmpty(scp.Volume.Entries);
        Assert.Equal(FlattenBlocks(image.AvailableBlocks), FlattenBlocks(scp.Image.AvailableBlocks));
        Assert.NotEmpty(new SectorImageFluxVisualizer().Create(scp.Image).Tracks);
    }

    [Fact]
    public async Task FlatLisaPayloadWithoutTagsRemainsVisualizableButIsNotInventedAsAFileSystem()
    {
        var root = Environment.GetEnvironmentVariable("GWGUI_APPLE_CORPUS");
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root)) return;
        var path = Directory.EnumerateFiles(root, "*.scp.img", SearchOption.AllDirectories)
            .FirstOrDefault(candidate => new FileInfo(candidate).Length == 409_600);
        if (path is null) return;

        var document = await DiskImageExplorer.CreateDefault().ExploreAsync(path);

        Assert.Equal("applelisa.raw", document.Image.FormatId);
        Assert.Equal(800, document.Image.AvailableBlocks.Count);
        Assert.False(document.FileSystemRecognized);
        Assert.NotEmpty(new SectorImageFluxVisualizer().Create(document.Image).Tracks);
    }

    [Fact]
    public async Task LisaOfficeDataOnlyFluxIsIdentifiedWithoutInventingItsLostCatalog()
    {
        var root = Environment.GetEnvironmentVariable("GWGUI_APPLE_CORPUS");
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root)) return;
        var sourcePath = Directory.EnumerateFiles(root, "*LisaGuide.image", SearchOption.AllDirectories).FirstOrDefault();
        var scpPath = Directory.EnumerateFiles(root, "*LisaGuide*.scp", SearchOption.AllDirectories).FirstOrDefault();
        if (sourcePath is null || scpPath is null) return;

        var explorer = DiskImageExplorer.CreateDefault();
        var source = await explorer.ExploreAsync(sourcePath);
        var scp = await explorer.ExploreAsync(scpPath);

        Assert.Equal("applelisa.office", source.Image.FormatId);
        Assert.True(source.FileSystemRecognized);
        Assert.Equal("applelisa.raw", scp.Image.FormatId);
        Assert.Equal("Apple Lisa", scp.Metadata.SystemName);
        Assert.False(scp.FileSystemRecognized);
        Assert.NotEmpty(scp.Volume.Entries);
        Assert.Equal(FlattenBlocks(source.Image.AvailableBlocks), FlattenBlocks(scp.Image.AvailableBlocks));
        Assert.NotEmpty(new SectorImageFluxVisualizer().Create(scp.Image).Tracks);
    }

    [Fact]
    public async Task WozSixAndTwoDecoderResynchronizesProtectedBitstreams()
    {
        var root = Environment.GetEnvironmentVariable("GWGUI_APPLE_CORPUS");
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root)) return;
        var path = Directory.EnumerateFiles(root, "*816-Paint*.woz", SearchOption.AllDirectories).FirstOrDefault();
        if (path is null) return;

        var image = await new AppleDiskImageReader().ReadAsync(path);

        Assert.NotEmpty(image.AvailableBlocks);
        Assert.NotEmpty(new SectorImageFluxVisualizer().Create(image).Tracks);
    }

    [Fact]
    public async Task ProtectedDos32CatalogRemainsReadableAndReportsNonstandardEntries()
    {
        var root = Environment.GetEnvironmentVariable("GWGUI_APPLE_CORPUS");
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root)) return;
        var path = Directory.EnumerateFiles(root, "*Apple World*.woz", SearchOption.AllDirectories).FirstOrDefault();
        if (path is null) return;

        var document = await DiskImageExplorer.CreateDefault().ExploreAsync(path);

        Assert.True(document.FileSystemRecognized);
        Assert.Equal("Apple DOS 3.2", document.Volume.FileSystem);
        Assert.Contains(document.Volume.Entries, entry => entry.Name == "AUTODEMO");
        Assert.Contains(document.Volume.Entries, entry => entry.Name == "THRDIM");
        Assert.NotEmpty(document.Volume.Warnings);
    }


    [Fact]
    public async Task RealAppleScpCorpusCanBeDecodedWhenRequested()
    {
        var root = Environment.GetEnvironmentVariable("GWGUI_APPLE_CORPUS");
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root)) return;

        var explorer = DiskImageExplorer.CreateDefault();
        var paths = Directory.EnumerateDirectories(root, "Apple *", SearchOption.TopDirectoryOnly)
            .SelectMany(directory => Directory.EnumerateFiles(directory, "*.scp", SearchOption.AllDirectories))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}_generated{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        var requestedFile = Environment.GetEnvironmentVariable("GWGUI_APPLE_FILE");
        if (!string.IsNullOrWhiteSpace(requestedFile))
            paths = paths.Where(path => path.Contains(requestedFile, StringComparison.OrdinalIgnoreCase)).ToArray();
        var results = new List<string>();
        var failures = new List<string>();
        foreach (var path in paths)
        {
            try
            {
                var relativePath = Path.GetRelativePath(root, path);
                var explicitFormat = relativePath.Contains($"Apple Lisa{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                    ? null
                    : relativePath.Contains($"Apple III{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                    ? "apple3.sos"
                    : relativePath.Contains($"Apple II{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                    ? null
                    : relativePath.Contains("400k", StringComparison.OrdinalIgnoreCase) ? "mac.400" : "mac.800";
                var document = await explorer.ExploreAsync(path, explicitFormat);
                if (!document.FileSystemRecognized)
                {
                    var indexes = document.Image.AvailableBlocks.Select(block => block.LogicalBlock).Order().ToArray();
                    var block2 = document.Image.TryGetBlock(2, out var probe)
                        ? Convert.ToHexString(probe.Data.Take(16).ToArray()) : "missing";
                    var addresses = document.Image.AvailableBlocks.Select(block => block.Address).ToArray();
                    var addressRange = addresses.Length == 0 ? "empty" : $"C{addresses.Min(address => address.Cylinder)}..{addresses.Max(address => address.Cylinder)} H{addresses.Min(address => address.Head)}..{addresses.Max(address => address.Head)} S{addresses.Min(address => address.Number)}..{addresses.Max(address => address.Number)}";
                    failures.Add($"NO FILESYSTEM SCP {Path.GetRelativePath(root, path)}: {document.Image.FormatId}; blocks={indexes.Length}; range={(indexes.Length == 0 ? "empty" : $"{indexes[0]}..{indexes[^1]}")}; addresses={addressRange}; block2={block2}");
                }
                else
                    results.Add($"OPEN SCP {Path.GetRelativePath(root, path)}: {document.Image.FormatId}, {document.Volume.FileSystem}");
            }
            catch (Exception exception)
            {
                failures.Add($"FAIL SCP {Path.GetRelativePath(root, path)}: {exception.GetType().Name}: {exception.Message}");
            }
        }

        foreach (var result in results.Concat(failures)) Console.WriteLine(result);
        Assert.NotEmpty(results);
        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }

    private static string[] Flatten(IEnumerable<GWGUI.Scp.FileSystems.FileSystemEntry> entries, string prefix = "") => entries
        .SelectMany(entry => new[]
        {
            $"{prefix}/{entry.Name}|{entry.Kind}|{entry.Size}|{entry.Comment}|{entry.Protection}|{entry.MetadataValid}|{Convert.ToBase64String(entry.Content?.ToArray() ?? [])}"
        }.Concat(Flatten(entry.Children, $"{prefix}/{entry.Name}")))
        .ToArray();

    private static string[] FlattenBlocks(IEnumerable<SectorBlock> blocks) => blocks
        .OrderBy(block => block.LogicalBlock)
        .Select(block => $"{block.LogicalBlock}|{Convert.ToBase64String(block.Data.ToArray())}")
        .ToArray();

    private sealed class MemoryScpReader(ScpImage image) : IScpReader
    {
        public Task<ScpImage> ReadAsync(string path, CancellationToken cancellationToken = default) => Task.FromResult(image);
    }
}

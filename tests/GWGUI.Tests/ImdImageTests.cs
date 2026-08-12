using GWGUI.MediaEngine.Visualization;
using GWGUI.MediaEngine.Exploration;
using System.IO;
using GWGUI.MediaEngine.Containers.ImageDisk;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Epson;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.Reconstruction.Iso;

namespace GWGUI.Tests;

public sealed class ImdImageTests
{
    [Fact]
    public void EpsonDetectionIgnoresCandidatesWithoutData()
    {
        var empty = new Dictionary<SectorAddress, List<IsoSectorCandidate>>();
        Assert.False(AutomaticIsoScpSectorImagePolicy.TryDetectEpsonFormat(empty, out _));
        empty[new(0, 0, 0)] = [new(new(0, 0, 0, 1, 256, null, 0), 1)];
        Assert.False(AutomaticIsoScpSectorImagePolicy.TryDetectEpsonFormat(empty, out _));

        var mixed = new Dictionary<SectorAddress, List<IsoSectorCandidate>> { [new(0, 0, 99)] = [new(new(0, 0, 99, 1, 256, null, 0), 1)] };
        var geometry = EpsonQx10GeometryCatalog.Layout320;
        for (var index = 0; index < geometry.Count; index++)
        {
            var number = geometry.FirstSector + index;
            mixed[new(0, 0, number)] = [new(new(0, 0, number, 1, geometry.SectorSize, true, 0, Data: new byte[geometry.SectorSize]), 1)];
        }
        Assert.True(AutomaticIsoScpSectorImagePolicy.TryDetectEpsonFormat(mixed, out _));
    }

    [Theory]
    [InlineData(ImdMode.Fm500Kbps)]
    [InlineData(ImdMode.Fm300Kbps)]
    [InlineData(ImdMode.Fm250Kbps)]
    [InlineData(ImdMode.Mfm500Kbps)]
    [InlineData(ImdMode.Mfm300Kbps)]
    [InlineData(ImdMode.Mfm250Kbps)]
    public void AcceptsEveryDefinedMode(ImdMode mode)
    {
        var image = ImdReader.Read(CreateSingleSectorImage(mode: mode));
        Assert.Single(image.AvailableBlocks);
    }

    [Theory]
    [InlineData(ImdSectorRecordType.Unavailable, false, false)]
    [InlineData(ImdSectorRecordType.Normal, true, true)]
    [InlineData(ImdSectorRecordType.Compressed, true, true)]
    [InlineData(ImdSectorRecordType.Deleted, true, true)]
    [InlineData(ImdSectorRecordType.CompressedDeleted, true, true)]
    [InlineData(ImdSectorRecordType.NormalWithError, true, false)]
    [InlineData(ImdSectorRecordType.CompressedWithError, true, false)]
    [InlineData(ImdSectorRecordType.DeletedWithError, true, false)]
    [InlineData(ImdSectorRecordType.CompressedDeletedWithError, true, false)]
    public void ReadsEverySectorRecordType(ImdSectorRecordType type, bool available, bool integrityValid)
    {
        var image = ImdReader.Read(CreateSingleSectorImage(recordType: type));
        Assert.Equal(available ? 1 : 0, image.AvailableBlocks.Count);
        if (!available)
        {
            Assert.Equal([0], image.MissingBlocks);
            return;
        }
        var block = Assert.Single(image.AvailableBlocks);
        Assert.Equal(integrityValid, block.IntegrityValid);
        Assert.Equal(128, block.Data.Count);
        Assert.All(block.Data, value => Assert.Equal(0x5A, value));
    }

    [Fact]
    public void ReadsOptionalCylinderHeadAndExplicitSizeMaps()
    {
        var image = ImdReader.Read(CreateSingleSectorImage(headFlags: ImdHeadFlags.HasCylinderMap | ImdHeadFlags.HasHeadMap, sizeCode: ImdLayout.ExplicitSectorSizeCode, explicitSize: 256, mappedCylinder: 2, mappedHead: 1));
        var block = Assert.Single(image.AvailableBlocks);
        Assert.Equal((2, 1, 1, 256), (block.Address.Cylinder, block.Address.Head, block.Address.Number, block.Data.Count));
        Assert.Equal((3, 2, 256L), (image.Cylinders, image.Heads, image.Capacity));
    }

    [Fact]
    public async Task UnavailableImdSectorRemainsMissing()
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-imd-{Guid.NewGuid():N}.imd");
        try
        {
            // One 128-byte sector is declared, but record type 0 means that
            // ImageDisk could not provide its contents.
            await File.WriteAllBytesAsync(path,
            [
                (byte)'I', (byte)'M', (byte)'D', 0x1a,
                0, 0, 0, 1, 0,
                1,
                0
            ]);

            var image = await new ImdReader().ReadAsync(path);

            Assert.Equal(1, image.BlockCount);
            Assert.Equal(128, image.Capacity);
            Assert.Empty(image.AvailableBlocks);
            Assert.Equal([0], image.MissingBlocks);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task PartialEpsonQx10ImagePreservesItsInvalidSectors()
    {
        var path = Path.Combine(RepositoryRoot(), "image_test", "validated_images", "Epson", "QX-10", "5.25 pouces - QX-10 396 Kio", "Valdocs 2.00 disk01-396.imd");
        if (!File.Exists(path)) return;
        var document = await DiskImageExplorer.CreateDefault().ExploreAsync(path);

        Assert.Equal("epson.qx10.396", document.Image.FormatId);
        Assert.True(document.FileSystemRecognized);
        Assert.NotEmpty(document.Volume.Entries);
        Assert.Equal(48, document.Image.AvailableBlocks.Count(block => block.IntegrityValid == false));
        Assert.NotEmpty(new SectorImageFluxVisualizer().Create(document.Image).Tracks);
    }

    [Theory]
    [InlineData(DiskImageFormatIds.EpsonQx10_320)]
    [InlineData(DiskImageFormatIds.EpsonQx10_400)]
    [InlineData(DiskImageFormatIds.EpsonQx10Booter)]
    [InlineData(DiskImageFormatIds.EpsonQx10_399)]
    [InlineData(DiskImageFormatIds.EpsonQx10_396)]
    [InlineData(DiskImageFormatIds.EpsonQx10Logo)]
    public void CommonDetectorRecognizesEveryEpsonGeometry(string formatId)
    {
        var geometry = EpsonQx10GeometryCatalog.Resolve(formatId);
        var sectors = new List<EpsonQx10SectorDescriptor>();
        for (var cylinder = 0; cylinder < geometry.Cylinders; cylinder++)
        {
            for (var head = 0; head < geometry.Heads; head++)
            {
                var track = geometry.Track(cylinder, head);
                for (var index = 0; index < track.Count; index++) sectors.Add(new(cylinder, head, track.FirstSector + index, track.SectorSize));
            }
        }
        Assert.True(EpsonQx10FormatDetector.TryDetect(sectors, out var detected));
        Assert.Equal(formatId, detected);
    }

    [Fact]
    public void EpsonGeometryResolves396AndRejectsUnknownIdentifier()
    {
        Assert.Equal(40, EpsonQx10GeometryCatalog.Resolve(DiskImageFormatIds.EpsonQx10_396).Cylinders);
        Assert.Throws<ArgumentException>(() => EpsonQx10GeometryCatalog.Resolve("epson.qx10.unknown"));
    }

    [Fact]
    public void EpsonCatalogContainsUniqueIdentifiersGeometriesAndCapacities()
    {
        Assert.Equal(6, EpsonQx10GeometryCatalog.All.Count);
        Assert.Equal(EpsonQx10GeometryCatalog.All.Count, EpsonQx10GeometryCatalog.All.Keys.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        var signatures = EpsonQx10GeometryCatalog.All.Values.Select(geometry => $"{geometry.Cylinders}:{geometry.Heads}:{string.Join(';', geometry.AllTracks.Select(track => $"{track.FirstSector},{track.Count},{track.SectorSize}"))}").ToArray();
        Assert.Equal(signatures.Length, signatures.Distinct(StringComparer.Ordinal).Count());
        Assert.All(EpsonQx10GeometryCatalog.All.Values, geometry => Assert.True(geometry.AllTracks.Sum(track => (long)track.Count * track.SectorSize) > 0));
    }

    [Fact]
    public void AmbiguousSingleSmallTrackUsesTheCatalogPriorityAndEmptyInputDoesNotMatch()
    {
        var sectors = Enumerable.Range(0, EpsonQx10GeometryCatalog.Layout320.Count).Select(index => new EpsonQx10SectorDescriptor(0, 0, EpsonQx10GeometryCatalog.Layout320.FirstSector + index, EpsonQx10GeometryCatalog.Layout320.SectorSize)).ToArray();
        Assert.True(EpsonQx10FormatDetector.TryDetect(sectors, out var formatId));
        Assert.Equal(DiskImageFormatIds.EpsonQx10_320, formatId);
        Assert.False(EpsonQx10FormatDetector.TryDetect([], out _));
    }

    [Fact]
    public void NonEpsonLayoutFallsBackToImd()
    {
        var image = ImdReader.Read(CreateSingleSectorImage());
        Assert.Equal(DiskImageFormatIds.Imd, image.FormatId);
    }

    [Fact]
    public void RejectsInvalidAndTruncatedSections()
    {
        var valid = CreateSingleSectorImage();
        var signature = valid.ToArray();
        signature[0] = (byte)'X';
        Assert.Contains("header", Assert.Throws<InvalidDataException>(() => ImdReader.Read(signature)).Message, StringComparison.OrdinalIgnoreCase);

        var header = valid.ToArray();
        header[4] = 6;
        Assert.Contains("track header", Assert.Throws<InvalidDataException>(() => ImdReader.Read(header)).Message, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("SectorNumberMap", Assert.Throws<InvalidDataException>(() => ImdReader.Read(valid[..9])).Message, StringComparison.OrdinalIgnoreCase);

        var sizeCode = valid.ToArray();
        sizeCode[8] = 7;
        Assert.Contains("size code", Assert.Throws<InvalidDataException>(() => ImdReader.Read(sizeCode)).Message, StringComparison.OrdinalIgnoreCase);

        var recordType = valid.ToArray();
        recordType[10] = 9;
        Assert.Contains("record type", Assert.Throws<InvalidDataException>(() => ImdReader.Read(recordType)).Message, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("SectorData", Assert.Throws<InvalidDataException>(() => ImdReader.Read(valid[..^1])).Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PropagatesCancellationDuringTrackTraversal()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Assert.ThrowsAny<OperationCanceledException>(() => ImdReader.Read(CreateSingleSectorImage(), cancellation.Token));
    }

    private static byte[] CreateSingleSectorImage(ImdMode mode = ImdMode.Fm500Kbps, ImdHeadFlags headFlags = 0, byte sizeCode = 0, int explicitSize = 128, int mappedCylinder = 0, int mappedHead = 0, ImdSectorRecordType recordType = ImdSectorRecordType.Normal)
    {
        var bytes = new List<byte> { (byte)'I', (byte)'M', (byte)'D', ImdFormat.CommentTerminator, (byte)mode, 0, (byte)headFlags, 1, sizeCode, 1 };
        if (headFlags.HasFlag(ImdHeadFlags.HasCylinderMap)) bytes.Add((byte)mappedCylinder);
        if (headFlags.HasFlag(ImdHeadFlags.HasHeadMap)) bytes.Add((byte)mappedHead);
        var sectorSize = sizeCode == ImdLayout.ExplicitSectorSizeCode ? explicitSize : ImdLayout.BaseSectorSize << sizeCode;
        if (sizeCode == ImdLayout.ExplicitSectorSizeCode)
        {
            bytes.Add((byte)explicitSize);
            bytes.Add((byte)(explicitSize >> 8));
        }
        bytes.Add((byte)recordType);
        if (recordType.HasData())
        {
            if (recordType.IsCompressed()) bytes.Add(0x5A);
            else bytes.AddRange(Enumerable.Repeat((byte)0x5A, sectorSize));
        }
        return [.. bytes];
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GWGUI.sln"))) directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("Repository root not found.");
    }
}

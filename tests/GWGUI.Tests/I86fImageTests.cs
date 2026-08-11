using System.Buffers.Binary;
using System.IO;
using GWGUI.MediaEngine.Containers.I86f;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Flux.Conversion;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

/// <summary>Vérifie le parsing, la conversion et la reconstruction publique des conteneurs 86F.</summary>
public sealed class I86fImageTests
{
    /// <summary>Vérifie les drapeaux, les pistes et la géométrie IBM de l'image MFM réelle.</summary>
    [Fact]
    public async Task RealMfmImageExposesExpectedContainerAndSectorGeometry()
    {
        var path = MfmImagePath();
        var container = await new I86fReader().ReadAsync(path);
        Assert.True(container.Flags.HasFlag(I86fFileFlags.TwoSided));
        Assert.True(container.Flags.HasFlag(I86fFileFlags.ExtraBitCellCount));
        Assert.False(container.Flags.HasFlag(I86fFileFlags.ReverseByteOrder));
        Assert.NotEmpty(container.Tracks);
        Assert.Equal(I86fTrackFlags.MfmEncoding, container.Tracks[0].Flags & I86fTrackFlags.EncodingMask);

        var image = await new I86fSectorImageReader(new I86fReader(), new FluxDecoderRegistry()).ReadAsync(path);
        Assert.Equal((DiskImageFormatIds.Ibm360, 512, 40, 2, 9), (image.FormatId, image.BlockSize, image.Cylinders, image.Heads, image.SectorsPerTrack));
        Assert.NotEmpty(image.AvailableBlocks);
    }

    /// <summary>Vérifie que le registre public route l'image réelle vers le lecteur sectoriel 86F.</summary>
    [Fact]
    public async Task PublicRegistryRoutesRealMfmImage()
    {
        var document = await DiskImageExplorer.CreateDefault().ExploreAsync(MfmImagePath());
        Assert.Equal(DiskImageFormatIds.Ibm360, document.Image.FormatId);
        Assert.NotEmpty(document.Image.AvailableBlocks);
    }

    /// <summary>Vérifie les modes une face, deux faces, entrée absente et compte explicite ou déduit.</summary>
    [Fact]
    public async Task ParserHandlesSidesMissingEntriesAndExplicitOrDerivedBitCounts()
    {
        var source = await File.ReadAllBytesAsync(MfmImagePath());
        var reader = new I86fReader();
        var twoSided = await reader.ReadAsync(MfmImagePath());
        var firstIndex = FirstPresentTrackIndex(source);
        var firstOffset = ReadTrackOffset(source, firstIndex);
        Assert.Equal(BinaryPrimitives.ReadInt32LittleEndian(source.AsSpan(firstOffset + I86fLayout.ExplicitBitCountOffset)), twoSided.Tracks.First(track => track.LogicalIndex == firstIndex).BitCount);

        var oneSidedBytes = source.ToArray();
        var flags = (I86fFileFlags)BinaryPrimitives.ReadUInt16LittleEndian(oneSidedBytes.AsSpan(I86fLayout.FileFlagsOffset, I86fLayout.FileFlagsLength));
        BinaryPrimitives.WriteUInt16LittleEndian(oneSidedBytes.AsSpan(I86fLayout.FileFlagsOffset, I86fLayout.FileFlagsLength), (ushort)(flags & ~I86fFileFlags.TwoSided));
        var oneSided = await ReadTemporaryContainerAsync(oneSidedBytes);
        Assert.False(oneSided.Flags.HasFlag(I86fFileFlags.TwoSided));
        Assert.All(oneSided.Tracks, track => Assert.InRange(track.LogicalIndex, 0, I86fLayout.TrackTableEntriesPerSide - 1));

        var missingTrackBytes = source.ToArray();
        BinaryPrimitives.WriteUInt32LittleEndian(missingTrackBytes.AsSpan(I86fLayout.TrackTableOffset + firstIndex * I86fLayout.TrackTableEntrySize, I86fLayout.TrackTableEntrySize), 0);
        var missingTrack = await ReadTemporaryContainerAsync(missingTrackBytes);
        Assert.DoesNotContain(missingTrack.Tracks, track => track.LogicalIndex == firstIndex);

        var derivedBytes = source.ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(derivedBytes.AsSpan(I86fLayout.FileFlagsOffset, I86fLayout.FileFlagsLength), (ushort)(flags & ~(I86fFileFlags.ExtraBitCellCount | I86fFileFlags.SpeedupOrExplicitBitCount)));
        var derived = await ReadTemporaryContainerAsync(derivedBytes);
        var nextOffset = NextPresentTrackOffset(source, firstIndex + 1);
        Assert.Equal((nextOffset - firstOffset - I86fLayout.StandardTrackHeaderSize) * I86fLayout.BitsPerByte, derived.Tracks.First(track => track.LogicalIndex == firstIndex).BitCount);
    }

    /// <summary>Vérifie que l'ordre inversé des octets de chaque mot est normalisé par le parser.</summary>
    [Fact]
    public async Task ParserNormalizesReversedWordBytes()
    {
        var source = await File.ReadAllBytesAsync(MfmImagePath());
        var normalOrder = await new I86fReader().ReadAsync(MfmImagePath());
        var reversedBytes = source.ToArray();
        var flags = (I86fFileFlags)BinaryPrimitives.ReadUInt16LittleEndian(reversedBytes.AsSpan(I86fLayout.FileFlagsOffset, I86fLayout.FileFlagsLength));
        BinaryPrimitives.WriteUInt16LittleEndian(reversedBytes.AsSpan(I86fLayout.FileFlagsOffset, I86fLayout.FileFlagsLength), (ushort)(flags | I86fFileFlags.ReverseByteOrder));
        var reversed = await ReadTemporaryContainerAsync(reversedBytes);

        Assert.Equal(reversed.Tracks[0].Bits.Take(I86fLayout.BitsPerByte), normalOrder.Tracks[0].Bits.Skip(I86fLayout.BitsPerByte).Take(I86fLayout.BitsPerByte));
        Assert.Equal(reversed.Tracks[0].Bits.Skip(I86fLayout.BitsPerByte).Take(I86fLayout.BitsPerByte), normalOrder.Tracks[0].Bits.Take(I86fLayout.BitsPerByte));
    }

    /// <summary>Vérifie la conversion exacte des cellules en intervalles et l'absence de révolution sans transition.</summary>
    [Fact]
    public void ConverterUsesFortyTicksPerCellAndRejectsTransitionlessTracks()
    {
        var revolution = Assert.IsType<GWGUI.MediaEngine.Containers.Scp.ScpRevolution>(I86fBitCellFluxConverter.Convert([false, true, false, false, true]));
        Assert.Equal(new uint[] { 80, 120 }, revolution.FluxIntervals);
        Assert.Equal((uint)200, revolution.IndexTimeTicks);
        Assert.Null(I86fBitCellFluxConverter.Convert([false, false, false]));
    }

    /// <summary>Vérifie le choix des décodeurs FM et MFM ainsi que les identifiants IBM et de repli.</summary>
    [Fact]
    public void DecoderAndFormatIdentifiersFollowFlagsAndGeometry()
    {
        Assert.Equal(FluxDecoderIds.IsoFm, I86fSectorImageReader.DecoderIdFor(I86fTrackFlags.None));
        Assert.Equal(FluxDecoderIds.IsoMfm, I86fSectorImageReader.DecoderIdFor(I86fTrackFlags.MfmEncoding));
        Assert.Equal("86f.256.40.1.10", DiskImageFormatIds.I86fFromGeometry(256, 40, 1, 10));
        Assert.Equal(DiskImageFormatIds.Ibm360, IbmPcImageReader.FormatIdForGeometry(40, 2, 9));
    }

    /// <summary>Vérifie le choix du meilleur candidat ISO selon son intégrité.</summary>
    [Fact]
    public void CommonBuilderSelectsTheValidCandidate()
    {
        var address = new SectorAddress(0, 0, 1);
        var invalid = new DecodedSector(0, 0, 1, 0, 128, false, 0, Data: Enumerable.Repeat((byte)0x11, 128).ToArray());
        var valid = new DecodedSector(0, 0, 1, 0, 128, true, 0, Data: Enumerable.Repeat((byte)0x22, 128).ToArray());
        var candidates = new Dictionary<SectorAddress, List<IsoSectorCandidate>> { [address] = [new(invalid, 0, 0), new(valid, 0, 0)] };
        var measured = IsoSectorImageBuilder.Measure(candidates);
        var image = IsoSectorImageBuilder.CreateUniform(DiskImageFormatIds.I86fFromGeometry(128, 1, 1, 1), candidates, measured.SectorSize, measured.Cylinders, measured.Heads, measured.SectorsPerTrack, _ => 0);
        Assert.All(image.GetBlock(0).ToArray(), value => Assert.Equal(0x22, value));
    }

    /// <summary>Vérifie les diagnostics de signature, table, position, nombre de bits et piste tronquée.</summary>
    [Fact]
    public async Task RejectsInvalidSignatureTableOffsetBitCountAndTrackLength()
    {
        var source = await File.ReadAllBytesAsync(MfmImagePath());
        var firstIndex = FirstPresentTrackIndex(source);
        var firstOffset = ReadTrackOffset(source, firstIndex);

        var signature = source.ToArray();
        signature[I86fFormat.SignatureOffset] ^= byte.MaxValue;
        await AssertRejectedAsync(signature, "signature");

        await AssertRejectedAsync(source[..(I86fLayout.TrackTableOffset + I86fLayout.TwoSideTrackTableEntries * I86fLayout.TrackTableEntrySize - 1)], "table");

        var offset = source.ToArray();
        BinaryPrimitives.WriteUInt32LittleEndian(offset.AsSpan(I86fLayout.TrackTableOffset + firstIndex * I86fLayout.TrackTableEntrySize, I86fLayout.TrackTableEntrySize), uint.MaxValue);
        await AssertRejectedAsync(offset, "offset");

        var bitCount = source.ToArray();
        BinaryPrimitives.WriteInt32LittleEndian(bitCount.AsSpan(firstOffset + I86fLayout.ExplicitBitCountOffset), 0);
        await AssertRejectedAsync(bitCount, "bit-cell count");

        var truncated = source.ToArray();
        BinaryPrimitives.WriteInt32LittleEndian(truncated.AsSpan(firstOffset + I86fLayout.ExplicitBitCountOffset), int.MaxValue);
        await AssertRejectedAsync(truncated, "truncated");
    }

    /// <summary>Vérifie que l'annulation est propagée avant le parcours des pistes.</summary>
    [Fact]
    public async Task PropagatesCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new I86fReader().ReadAsync(MfmImagePath(), cancellation.Token));
    }

    /// <summary>Vérifie qu'une variante invalide est rejetée avec le diagnostic attendu.</summary>
    private static async Task AssertRejectedAsync(byte[] data, string message)
    {
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() => ReadTemporaryContainerAsync(data));
        Assert.Contains(message, exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Lit une variante temporaire avec le parser public.</summary>
    private static async Task<I86fImage> ReadTemporaryContainerAsync(byte[] data)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.86f");
        try
        {
            await File.WriteAllBytesAsync(path, data);
            return await new I86fReader().ReadAsync(path);
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>Retourne l'index de la première piste présente.</summary>
    private static int FirstPresentTrackIndex(byte[] data)
    {
        for (var index = 0; index < I86fLayout.TwoSideTrackTableEntries; index++)
        {
            if (ReadTrackOffset(data, index) != 0) return index;
        }
        throw new InvalidDataException("L'image 86F de test ne contient aucune piste.");
    }

    /// <summary>Lit la position d'une piste dans la table.</summary>
    private static int ReadTrackOffset(byte[] data, int index) => checked((int)BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(I86fLayout.TrackTableOffset + index * I86fLayout.TrackTableEntrySize, I86fLayout.TrackTableEntrySize)));

    /// <summary>Retourne la prochaine position de piste présente ou la fin du fichier.</summary>
    private static int NextPresentTrackOffset(byte[] data, int start)
    {
        for (var index = start; index < I86fLayout.TwoSideTrackTableEntries; index++)
        {
            var offset = ReadTrackOffset(data, index);
            if (offset != 0) return offset;
        }
        return data.Length;
    }

    /// <summary>Retourne le chemin obligatoire de l'image MFM réelle.</summary>
    private static string MfmImagePath()
    {
        var path = Path.Combine(RepositoryRoot(), "image_test", "IBM PC", "Framework Premier 1.1 Fr - Systeme 1 [5.25].86f");
        return File.Exists(path) ? path : throw new FileNotFoundException("L'image 86F MFM de test est introuvable.", path);
    }

    /// <summary>Localise la racine du dépôt.</summary>
    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GWGUI.sln"))) directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("La racine du dépôt est introuvable.");
    }
}

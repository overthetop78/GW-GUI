using GWGUI.MediaEngine.Definitions;
using System.IO;
using GWGUI.MediaEngine.Containers.Dec.Rx02;
using GWGUI.MediaEngine.Geometries.Dec;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

/// <summary>Vérifie la reconnaissance RT-11 et la remise en ordre physique des dumps DEC RX02 locaux.</summary>
public sealed class DecRx02RecognitionTests
{
    [Fact]
    public async Task RecognizesRt11ContentWithAnUnusualExtensionAndReordersPhysicalSectors()
    {
        var sourcePath = Rx02ImagePath();
        var unusualPath = Path.Combine(Path.GetTempPath(), $"gwgui-rx02-{Guid.NewGuid():N}.unexpected");
        try
        {
            File.Copy(sourcePath, unusualPath);
            var raw = await File.ReadAllBytesAsync(sourcePath);

            var explored = await DiskImageExplorer.CreateDefault().ExploreAsync(unusualPath);

            Assert.Equal(DiskImageFormatIds.DecRx02, explored.Image.FormatId);
            Assert.Equal(512, explored.Image.BlockSize);
            Assert.Equal(77, explored.Image.Cylinders);
            Assert.Equal(1, explored.Image.Heads);
            Assert.Equal(13, explored.Image.SectorsPerTrack);
            Assert.Equal(1001, explored.Image.BlockCount);
            Assert.Equal(DecRx02Geometry.Capacity, explored.Image.Capacity);
            foreach (var logicalBlock in new[] { 0, 13, 500, 1000 })
            {
                Assert.True(explored.Image.TryGetBlock(logicalBlock, out var block));
                Assert.Equal(new SectorAddress(logicalBlock / 13, 0, logicalBlock % 13 + 1), block.Address);
                Assert.Equal(ExpectedLogicalBlock(raw, logicalBlock), block.Data);
            }
        }
        finally
        {
            if (File.Exists(unusualPath)) File.Delete(unusualPath);
        }
    }

    [Fact]
    public async Task SameSizedImageWithoutRt11StructureIsNotSelectedAutomatically()
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-not-rx02-{Guid.NewGuid():N}.unexpected");
        try
        {
            await using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                stream.SetLength(DecRx02Geometry.Capacity);
            var explored = await DiskImageExplorer.CreateDefault().ExploreAsync(path);

            Assert.Equal(DiskImageFormatIds.Unknown, explored.Image.FormatId);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task ExplicitRx02SelectionAcceptsTheExactCapacityWithoutAnRt11HomeBlock()
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-explicit-rx02-{Guid.NewGuid():N}.unexpected");
        try
        {
            await using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None)) stream.SetLength(DecRx02Geometry.Capacity);

            var explored = await DiskImageExplorer.CreateDefault().ExploreAsync(path, DiskImageFormatIds.DecRx02);

            Assert.Equal(DiskImageFormatIds.DecRx02, explored.Image.FormatId);
            Assert.Equal(DecRx02Geometry.Capacity, explored.Image.Capacity);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    private static byte[] ExpectedLogicalBlock(byte[] physicalDump, int logicalBlock)
    {
        var expected = new byte[512];
        for (var half = 0; half < 2; half++)
        {
            var logicalSector = logicalBlock * 2 + half;
            var logicalTrack = logicalSector / 26;
            var position = logicalSector % 26;
            position = (2 * position + (position >= 13 ? 1 : 0)) % 26;
            var sector = 1 + (position + 6 * logicalTrack) % 26;
            var track = logicalTrack + 1;
            if (track >= 77) track = 0;
            physicalDump.AsSpan((track * 26 + sector - 1) * 256, 256)
                .CopyTo(expected.AsSpan(half * 256));
        }
        return expected;
    }

    private static string Rx02ImagePath()
    {
        var path = Path.Combine(
            FindImageTestRoot(),
            "validated_images",
            "DEC",
            "MINC",
            "8 pouces - RX02 - DEC RT-11 - 500 Kio",
            "BA-J837B-BC_MINC_MA_DEMO_23_V2.0_BIN_RX2.img");
        return File.Exists(path)
            ? path
            : throw new FileNotFoundException("L'image RX02 RT-11 locale requise est absente.", path);
    }

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

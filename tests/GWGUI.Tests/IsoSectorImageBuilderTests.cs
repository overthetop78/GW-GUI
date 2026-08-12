using System.IO;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Reconstruction.Iso;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

/// <summary>Vérifie la mesure et la construction communes des images sectorielles ISO.</summary>
public sealed class IsoSectorImageBuilderTests
{
    /// <summary>Vérifie les valeurs majoritaires et les limites déduites des adresses.</summary>
    [Fact]
    public void MeasuresMajorityGeometryOrderAndZeroBasedNumbering()
    {
        var candidates = Candidates(
            Candidate(0, 0, 0, 512, true, 1), Candidate(0, 0, 1, 512, true, 2),
            Candidate(1, 1, 0, 512, null, 3), Candidate(1, 1, 1, 256, false, 4));

        var result = IsoSectorImageBuilder.Measure(candidates);

        Assert.Equal(512, result.SectorSize);
        Assert.Equal(2, result.Cylinders);
        Assert.Equal(2, result.Heads);
        Assert.Equal(2, result.SectorsPerTrack);
        Assert.Equal([0, 1], result.SectorOrder);
        Assert.True(result.ZeroBased);
    }

    /// <summary>Vérifie le filtrage géométrique, la numérotation, la capacité et la priorité d'intégrité.</summary>
    [Fact]
    public void CreatesUniformImageWithRequestedGeometryAndBestCandidate()
    {
        var address = new SectorAddress(0, 0, 1);
        var candidates = new Dictionary<SectorAddress, List<IsoSectorCandidate>>
        {
            [address] = [Candidate(address, 256, false, 1, 1), Candidate(address, 128, null, 2, 2), Candidate(address, 384, true, 3, 3)],
            [new(2, 0, 1)] = [Candidate(new(2, 0, 1), 128, true, 4, 4)]
        };

        var image = IsoSectorImageBuilder.CreateUniform("test.iso", candidates, 512, 1, 1, 2, item => item.Number - 1, allowVariableBlockSize: true, capacity: 2048);

        Assert.Equal(2048, image.Capacity);
        var block = Assert.Single(image.AvailableBlocks);
        Assert.Equal(0, block.LogicalBlock);
        Assert.Equal(384, block.Data.Count);
        Assert.True(block.IntegrityValid);
        Assert.Equal(3, block.Revolution);
    }

    /// <summary>Vérifie les réponses lorsque les candidats ou leurs données sont absents.</summary>
    [Fact]
    public void ReportsMissingCandidatesAndData()
    {
        var empty = new Dictionary<SectorAddress, List<IsoSectorCandidate>>();
        Assert.Throws<InvalidDataException>(() => IsoSectorImageBuilder.Measure(empty));
        Assert.Empty(IsoSectorImageBuilder.BestData(empty, new(0, 0, 1)));

        var address = new SectorAddress(0, 0, 1);
        var withoutData = new Dictionary<SectorAddress, List<IsoSectorCandidate>> { [address] = [new(new(0, 0, 1, 0, 128, true, 0), 0)] };
        Assert.Empty(IsoSectorImageBuilder.BestData(withoutData, address));
    }

    private static Dictionary<SectorAddress, List<IsoSectorCandidate>> Candidates(params IsoSectorCandidate[] candidates) => candidates.GroupBy(candidate => new SectorAddress(candidate.Sector.Cylinder, candidate.Sector.Head, candidate.Sector.Number)).ToDictionary(group => group.Key, group => group.ToList());

    private static IsoSectorCandidate Candidate(byte cylinder, byte head, int number, int size, bool? integrity, byte value) => Candidate(new(cylinder, head, number), size, integrity, value, 0);

    private static IsoSectorCandidate Candidate(SectorAddress address, int size, bool? integrity, byte value, int revolution) => new(new((byte)address.Cylinder, (byte)address.Head, address.Number, 0, size, integrity, 0, Data: Enumerable.Repeat(value, size).ToArray()), revolution);
}

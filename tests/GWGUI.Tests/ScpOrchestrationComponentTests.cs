using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.Exploration.Scp;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using GWGUI.MediaEngine.Recognition.Scp;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.SectorImages.Scp;
using System.IO;

namespace GWGUI.Tests;

/// <summary>Vérifie l'ordre déterministe et le classement des composants SCP extraits.</summary>
public sealed class ScpOrchestrationComponentTests
{
    [Fact]
    public void CandidateRegistryCopiesCollectionsAndPreservesFamilyOrder()
    {
        var iso = Candidate("iso", ScpFormatFamily.Iso);
        var apple = Candidate("apple", ScpFormatFamily.Apple);
        var fallback = Candidate("fallback", ScpFormatFamily.Iso);
        var defaults = new List<ScpSectorImageCandidate> { apple, iso };
        var families = new List<KeyValuePair<ScpFormatFamily, IReadOnlyList<ScpSectorImageCandidate>>> { new(ScpFormatFamily.Iso, [iso]), new(ScpFormatFamily.Apple, [apple]) };
        var registry = new ScpCandidateRegistry([new(id => id.StartsWith("apple", StringComparison.OrdinalIgnoreCase), apple)], defaults, families, [ScpFormatFamily.Iso, ScpFormatFamily.Apple], fallback);
        defaults.Clear();
        families.Clear();
        Assert.Null(registry.Selected(null));
        Assert.Same(apple, registry.Selected("apple2.dos33"));
        Assert.Same(fallback, registry.Selected("unknown"));
        Assert.Equal(["apple", "iso"], registry.Default().Select(candidate => candidate.Id));
        Assert.Equal(["iso", "apple"], registry.Automatic(new HashSet<ScpFormatFamily>()).Select(candidate => candidate.Id));
        Assert.Equal(["iso", "apple"], registry.Automatic(new HashSet<ScpFormatFamily> { ScpFormatFamily.Apple, ScpFormatFamily.Iso }).Select(candidate => candidate.Id));
    }

    [Fact]
    public void TrackSamplerUsesFirstLastAndUniformIntermediateTracks()
    {
        var revolution = new ScpRevolution(1, 0, []);
        var tracks = Enumerable.Range(0, 10).Select(index => new ScpTrack((byte)index, index, 0, [revolution])).ToArray();
        Assert.Equal([0, 1, 3, 5, 7, 9], ScpTrackSampler.Sample(tracks).Select(track => (int)track.TrackNumber));
        Assert.Empty(ScpTrackSampler.Sample([]));
        Assert.Empty(ScpTrackSampler.Sample([new ScpTrack(0, 0, 0, [])]));
    }

    [Fact]
    public void RankerKeepsFirstOnEqualScoresAndPreservesFormatOrderAndDiagnostics()
    {
        var first = Image("first", 1);
        var second = Image("second", 1);
        var volume = new FileSystemVolume("VOL", "fs", 1, 0, null, null, [], []);
        var match = new ExploredFileSystem("first", "reader", first, volume);
        var rejected = new ScpCandidateInspection("bad", null, [], new InvalidDataException("bad"));
        var result = ScpCandidateRanker.Rank([new("first-candidate", first, [match], null), new("second-candidate", second, [], null), rejected]);
        Assert.Same(first, result.BestDecoded);
        Assert.Same(first, result.BestRecognized);
        Assert.Equal(["first", "second"], result.DecodedImages.Select(image => image.FormatId));
        Assert.Same(first, result.DecodedImages[0]);
        Assert.Same(second, result.DecodedImages[1]);
        Assert.Single(result.Detected);
        Assert.Same(rejected, result.Rejected.Single());
    }

    [Fact]
    public void ProbeCatalogCoversEveryFamily()
    {
        Assert.Equal(8, ScpFamilyProbeCatalog.Definitions.Count);
        Assert.All(Enum.GetValues<ScpFormatFamily>(), family => Assert.Contains(ScpFamilyProbeCatalog.Definitions, definition => definition.Family == family));
    }

    private static ScpSectorImageCandidate Candidate(string id, ScpFormatFamily family) => new(id, family, (_, _, _) => Task.FromResult(Image(id, 1)));
    private static SectorImage Image(string formatId, int blocks) => new(formatId, 1, 1, 1, blocks, Enumerable.Range(0, blocks).Select(index => new SectorBlock(index, new SectorAddress(0, 0, index + 1), [0])));
}

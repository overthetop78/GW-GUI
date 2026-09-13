using GWGUI.Domain.Contracts;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Representations.Blocks;

namespace GWGUI.Tests.MediaEngine.Recognition;

public sealed class MediaRecognitionRegistryTests
{
    [Fact]
    public async Task SelectionCombinesEvidenceAndKeepsRegistrationOrder()
    {
        var path = TemporaryFile(".img", [0x47, 0x57, 0x01, 0x02]);
        try
        {
            var extension = new TestReader("extension", ["extension"], [".img"]);
            var firstProbe = new TestReader("first-probe", ["probe"], canRead: true);
            var secondProbe = new TestReader("second-probe", ["probe"], canRead: true);
            var signature = new TestReader("signature", ["signature"], signatures: [new byte[] { 0x47, 0x57 }]);
            var registry = new MediaRecognitionRegistry([extension, firstProbe, secondProbe, signature]);

            var candidates = await registry.SelectCandidatesAsync(Context(path));

            Assert.Equal(["signature", "first-probe", "second-probe", "extension"], candidates.Select(candidate => Assert.IsType<TestReader>(candidate.Reader).Id));
            Assert.Contains("file signature", candidates[0].Reason);
            Assert.Contains("file extension", candidates[^1].Reason);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task RequestedFormatAndAssociatedFileRestrictAndRankCandidates()
    {
        var path = TemporaryFile(".bin", [0x00]);
        var associatedPath = Path.ChangeExtension(path, ".cue");
        try
        {
            var requested = new TestReader("requested", ["wanted"], [".other"]);
            var rejected = new TestReader("rejected", ["other"], [".bin"], associatedExtensions: [".cue"]);
            var registry = new MediaRecognitionRegistry([rejected, requested]);

            var candidates = await registry.SelectCandidatesAsync(Context(path, [associatedPath], "wanted"));

            var selected = Assert.Single(candidates);
            Assert.Same(requested, selected.Reader);
            Assert.Contains("explicit format", selected.Reason);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task MatchingExtensionBreaksATieBetweenSuccessfulProbes()
    {
        var path = TemporaryFile(".ima", [0x00]);
        try
        {
            var genericProbe = new TestReader("generic-probe", ["generic"], canRead: true);
            var imaProbe = new TestReader("ima-probe", ["ibm.720"], [".ima"], canRead: true);
            var registry = new MediaRecognitionRegistry([genericProbe, imaProbe]);

            var candidates = await registry.SelectCandidatesAsync(Context(path));

            Assert.Same(imaProbe, candidates[0].Reader);
            Assert.Contains("file extension", candidates[0].Reason);
            Assert.Contains("reader probe", candidates[0].Reason);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task AssociatedFileIsRecognitionEvidenceWithoutOpeningIt()
    {
        var path = TemporaryFile(".bin", [0x00]);
        try
        {
            var reader = new TestReader("associated", ["associated"], associatedExtensions: [".cue"]);
            var registry = new MediaRecognitionRegistry([reader]);

            var candidates = await registry.SelectCandidatesAsync(Context(path, [Path.ChangeExtension(path, ".cue")]));

            Assert.Contains("associated file", Assert.Single(candidates).Reason);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task RecognitionReadsOnlyTheFirstSuccessfulCandidate()
    {
        var path = TemporaryFile(".img", [0x00]);
        try
        {
            var first = new TestReader("first", ["first"], [".img"]);
            var second = new TestReader("second", ["second"], [".img"]);
            var registry = new MediaRecognitionRegistry([first, second]);

            var result = await registry.RecognizeAsync(Context(path));

            Assert.Same(first, result.Reader);
            Assert.Equal(1, first.ReadCount);
            Assert.Equal(0, second.ReadCount);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task SelectionHonorsCancellationBeforeConsultingReaders()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var registry = new MediaRecognitionRegistry([new TestReader("reader", ["reader"], canRead: true)]);

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            registry.SelectCandidatesAsync(Context("unused.bin", knownLength: 0), cancellation.Token));
    }

    private static MediaRecognitionContext Context(
        string path,
        IReadOnlyList<string>? associatedPaths = null,
        string? requestedFormatId = null,
        long? knownLength = null) =>
        new(new MediaSourceDescriptor(path, associatedPaths ?? [], knownLength, requestedFormatId));

    private static string TemporaryFile(string extension, byte[] content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-recognition-{Guid.NewGuid():N}{extension}");
        File.WriteAllBytes(path, content);
        return path;
    }

    private sealed class TestReader : IMediaImageReader
    {
        private readonly bool canRead;

        public TestReader(
            string id,
            IEnumerable<string> formatIds,
            IEnumerable<string>? extensions = null,
            bool canRead = false,
            IReadOnlyList<ReadOnlyMemory<byte>>? signatures = null,
            IEnumerable<string>? associatedExtensions = null)
        {
            Id = id;
            FormatIds = formatIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
            Extensions = extensions?.ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new HashSet<string>();
            this.canRead = canRead;
            Signatures = signatures ?? [];
            AssociatedFileExtensions = associatedExtensions?.ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new HashSet<string>();
        }

        public string Id { get; }

        public int ReadCount { get; private set; }

        public IReadOnlySet<string> FormatIds { get; }

        public IReadOnlySet<string> Extensions { get; }

        public IReadOnlyList<ReadOnlyMemory<byte>> Signatures { get; }

        public IReadOnlySet<string> AssociatedFileExtensions { get; }

        public IReadOnlySet<MediaKind> MediaKinds { get; } = new HashSet<MediaKind> { MediaKind.HardDisk };

        public IReadOnlySet<MediaRepresentationKind> RepresentationKinds { get; } =
            new HashSet<MediaRepresentationKind> { MediaRepresentationKind.Blocks };

        public bool SupportsFormatId(string formatId) => FormatIds.Contains(formatId);

        public ValueTask<bool> CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(canRead);
        }

        public Task<MediaImageDocument> ReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ReadCount++;
            return Task.FromResult(new MediaImageDocument(
                context.Source,
                FormatIds.First(),
                MediaKind.HardDisk,
                new BlockMediaImageRepresentation(context.Length, context.Length == 0 ? [] : [(0, context.Length)]),
                [],
                [],
                new Dictionary<string, string>()));
        }
    }
}

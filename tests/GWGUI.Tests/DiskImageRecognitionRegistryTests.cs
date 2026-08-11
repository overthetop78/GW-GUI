using System.IO;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

/// <summary>Vérifie l'ordre, la reprise après rejet et la propagation des erreurs du registre de reconnaissance.</summary>
public sealed class DiskImageRecognitionRegistryTests
{
    [Fact]
    public async Task ContinuesAfterRejectedReaderAndPassesTheSameContentToTheNextPolicy()
    {
        var path = await CreateTemporaryImageAsync();
        try
        {
            var incompatible = new FakePolicy(canRead: false);
            var rejected = new FakePolicy(canRead: true, new InvalidDataException("Rejet factice du candidat."));
            var accepted = new FakePolicy(canRead: true);
            var registry = new DiskImageRecognitionRegistry([incompatible, rejected, accepted]);

            var image = await registry.ReadAsync(path, "accepted.format", CancellationToken.None);

            Assert.Equal("accepted.format", image.FormatId);
            Assert.Equal(1, incompatible.CanReadCalls);
            Assert.Equal(0, incompatible.ReadCalls);
            Assert.Equal(1, rejected.CanReadCalls);
            Assert.Equal(1, rejected.ReadCalls);
            Assert.Equal(1, accepted.CanReadCalls);
            Assert.Equal(1, accepted.ReadCalls);
            Assert.Equal(rejected.ReceivedBytes, accepted.ReceivedBytes);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ReportsTheCandidateIdentityAndOriginalErrorWhenOneCandidateRejectsTheImage()
    {
        var path = await CreateTemporaryImageAsync();
        try
        {
            var incompatible = new FakePolicy(canRead: false);
            var original = new InvalidDataException("Rejet factice du candidat.");
            var rejected = new FakePolicy(canRead: true, original);
            var registry = new DiskImageRecognitionRegistry([incompatible, rejected]);

            var exception = await Assert.ThrowsAsync<DiskImageCandidatesRejectedException>(() => registry.ReadAsync(path, null, CancellationToken.None));

            var failure = Assert.Single(exception.Failures);
            Assert.Contains(nameof(FakePolicy), failure.PolicyName, StringComparison.Ordinal);
            Assert.Same(original, failure.Exception);
            Assert.Equal(1, incompatible.CanReadCalls);
            Assert.Equal(0, incompatible.ReadCalls);
            Assert.Equal(1, rejected.CanReadCalls);
            Assert.Equal(1, rejected.ReadCalls);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("missing.format")]
    public async Task ReportsTheMissingCandidateWithOrWithoutAnExplicitFormat(string? requestedFormat)
    {
        var path = await CreateTemporaryImageAsync();
        try
        {
            var registry = new DiskImageRecognitionRegistry([new FakePolicy(canRead: false)]);

            var exception = await Assert.ThrowsAsync<NotSupportedException>(() => registry.ReadAsync(path, requestedFormat, CancellationToken.None));

            Assert.Contains(requestedFormat ?? path, exception.Message, StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ReportsEveryCandidateAndOriginalErrorWhenSeveralCandidatesRejectTheImage()
    {
        var path = await CreateTemporaryImageAsync();
        try
        {
            var first = new InvalidDataException("premier rejet");
            var second = new NotSupportedException("second rejet");
            var registry = new DiskImageRecognitionRegistry([new FakePolicy(true, first), new FakePolicy(true, second)]);

            var exception = await Assert.ThrowsAsync<DiskImageCandidatesRejectedException>(() => registry.ReadAsync(path, "requested.format", CancellationToken.None));

            Assert.Equal(2, exception.InnerExceptions.Count);
            Assert.All(exception.Failures, failure => Assert.Contains(nameof(FakePolicy), failure.PolicyName, StringComparison.Ordinal));
            Assert.Same(first, exception.Failures[0].Exception);
            Assert.Same(second, exception.Failures[1].Exception);
            Assert.Equal(".img", exception.Extension);
            Assert.Equal("requested.format", exception.RequestedFormatId);
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>Vérifie le rejet d'une collection ou d'une entrée nulle.</summary>
    [Fact]
    public void RejectsNullPolicyCollectionsAndEntries()
    {
        Assert.Throws<ArgumentNullException>(() => new DiskImageRecognitionRegistry(null!));
        Assert.Throws<ArgumentException>(() => new DiskImageRecognitionRegistry([null!]));
    }

    /// <summary>Vérifie que la copie du registre ne suit pas les modifications de la collection source.</summary>
    [Fact]
    public async Task IgnoresLaterChangesToTheInjectedCollection()
    {
        var path = await CreateTemporaryImageAsync();
        try
        {
            var accepted = new FakePolicy(true);
            var policies = new List<IDiskImageRecognitionPolicy> { accepted };
            var registry = new DiskImageRecognitionRegistry(policies);
            policies.Clear();
            policies.Add(new FakePolicy(false));

            _ = await registry.ReadAsync(path, null, CancellationToken.None);
            Assert.Equal(1, accepted.ReadCalls);
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>Vérifie qu'une erreur de programmation n'est pas transformée en rejet de format.</summary>
    [Fact]
    public async Task PropagatesUnexpectedExceptionsImmediately()
    {
        var path = await CreateTemporaryImageAsync();
        try
        {
            var error = new InvalidOperationException("erreur factice");
            var registry = new DiskImageRecognitionRegistry([new FakePolicy(true, error), new FakePolicy(true)]);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => registry.ReadAsync(path, null, CancellationToken.None));
            Assert.Same(error, actual);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task CancellationStopsTheLoopBeforeAnyPolicyCall()
    {
        var path = await CreateTemporaryImageAsync();
        try
        {
            var policy = new FakePolicy(canRead: true);
            var registry = new DiskImageRecognitionRegistry([policy]);
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                registry.ReadAsync(path, null, cancellation.Token));

            Assert.Equal(0, policy.CanReadCalls);
            Assert.Equal(0, policy.ReadCalls);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task PropagatesFileAccessErrors()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), $"gwgui-missing-{Guid.NewGuid():N}.img");
        var registry = new DiskImageRecognitionRegistry([new FakePolicy(canRead: true)]);

        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            registry.ReadAsync(missingPath, null, CancellationToken.None));
    }

    private static async Task<string> CreateTemporaryImageAsync()
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-registry-{Guid.NewGuid():N}.img");
        await File.WriteAllBytesAsync(path, [0x11, 0x22, 0x33]);
        return path;
    }

    /// <summary>Politique instrumentée permettant de compter chaque étape et de choisir son résultat.</summary>
    private sealed class FakePolicy(bool canRead, Exception? rejection = null) : IDiskImageRecognitionPolicy
    {
        /// <summary>Nombre d'appels de présélection reçus.</summary>
        public int CanReadCalls { get; private set; }
        /// <summary>Nombre d'appels de lecture reçus.</summary>
        public int ReadCalls { get; private set; }
        /// <summary>Référence du tableau partagé reçu pendant la lecture.</summary>
        public ReadOnlyMemory<byte>? ReceivedBytes { get; private set; }

        /// <summary>Compte la présélection et retourne la décision configurée.</summary>
        public ValueTask<bool> CanReadAsync(
            DiskImageRecognitionContext context,
            CancellationToken cancellationToken)
        {
            CanReadCalls++;
            return ValueTask.FromResult(canRead);
        }

        /// <summary>Compte la lecture, mémorise le contenu puis le rejette ou produit une image minimale.</summary>
        public async Task<SectorImage> ReadAsync(
            DiskImageRecognitionContext context,
            CancellationToken cancellationToken)
        {
            ReadCalls++;
            ReceivedBytes = await context.ReadBytesAsync(cancellationToken);
            if (rejection is not null) throw rejection;
            return new(
                context.RequestedFormatId ?? "accepted.format",
                ReceivedBytes.Value.Length,
                1,
                1,
                1,
                [new SectorBlock(0, new SectorAddress(0, 0, 1), ReceivedBytes.Value.ToArray())]);
        }
    }
}

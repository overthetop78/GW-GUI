using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Migration;

namespace GWGUI.Tests;

public sealed class MigrationContractTests
{
    [Fact]
    public void PlannerCopiesContentHierarchyAndMetadataWithoutChangingTheSource()
    {
        var modified = DateTimeOffset.Parse("1992-04-10T19:28:00+00:00");
        var file = new FileSystemEntry("Read Me", FileSystemEntryKind.File, 3, modified, "note", 7, 42, true, [], [1, 2, 3]);
        var directory = new FileSystemEntry("Docs", FileSystemEntryKind.Directory, 0, modified, string.Empty, 0, 9, true, [file]);
        var volume = new FileSystemVolume("SOURCE", "source.fs", 880 * 1024, 100, null, modified, [directory], []);

        var plan = MigrationPlanner.Create(volume, "target.fs");

        var migrated = Assert.Single(Assert.Single(plan.Entries).Children);
        Assert.Equal("Docs/Read Me", migrated.SourcePath);
        Assert.Equal([1, 2, 3], migrated.Content);
        Assert.Equal(modified, migrated.Modified);
        Assert.Equal("note", migrated.Comment);
        Assert.Equal(7u, migrated.RawAttributes);
    }

    [Fact]
    public void ValidatorBlocksMissingContentInvalidNamesAndCaseInsensitiveCollisions()
    {
        var entries = new[]
        {
            Entry("README", null),
            Entry("readme", [1]),
            Entry("bad/name", [2])
        };
        var report = MigrationValidator.Validate(new("source.fs", "target.fs", "VOL", entries), Capabilities());

        Assert.False(report.CanExecute);
        Assert.Contains(report.Losses, loss => loss.Kind == MigrationLossKind.MissingContent);
        Assert.Contains(report.Losses, loss => loss.Kind == MigrationLossKind.NameCollision);
        Assert.Contains(report.Losses, loss => loss.Kind == MigrationLossKind.InvalidName);
        Assert.Throws<InvalidOperationException>(() => MigrationValidator.EnsureExecutable(report));
    }

    [Fact]
    public void ValidatorRequiresExplicitAcceptanceForRepresentationalMetadataLosses()
    {
        var entry = new MigrationEntry("file", "file", FileSystemEntryKind.File, [1], DateTimeOffset.UtcNow, "comment", 1, true, []);
        var plan = new MigrationPlan("source.fs", "target.fs", "VOL", [entry]);

        var unaccepted = MigrationValidator.Validate(plan, Capabilities());
        var accepted = MigrationValidator.Validate(plan, Capabilities(), acceptMetadataLoss: true);

        Assert.False(unaccepted.CanExecute);
        Assert.True(accepted.CanExecute);
        Assert.Equal([MigrationLossKind.ModifiedDate, MigrationLossKind.Comment, MigrationLossKind.Attributes], accepted.Losses.Select(loss => loss.Kind));
        MigrationValidator.EnsureExecutable(accepted);
    }

    private static MigrationEntry Entry(string name, IReadOnlyList<byte>? content) => new(name, name, FileSystemEntryKind.File, content, null, string.Empty, 0, true, []);

    private static MigrationTargetCapabilities Capabilities() => new("target.fs", 12, 1_024, true, false, false, false, false, false, "/");
}

using System.IO;
using GWGUI.Emulation.Atari;

namespace GWGUI.Tests;

public sealed class AtariSessionMediaTests
{
    private static readonly IReadOnlySet<string> SupportedExtensions =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "st", "m3u" };

    [Fact]
    public void WritableMedia_UsesSessionCopyAndNeverChangesSourceImplicitly()
    {
        var root = CreateRoot();
        var source = Path.Combine(root, "disk.st");
        File.WriteAllBytes(source, [1, 2, 3]);
        try
        {
            var media = AtariSessionMediaFunctions.Prepare(CreateConfiguration(source),
                Path.Combine(root, "session"), SupportedExtensions);
            File.WriteAllBytes(media.RuntimePath, [4, 5, 6]);

            Assert.Equal(new byte[] { 1, 2, 3 }, File.ReadAllBytes(source));
            AtariSessionMediaFunctions.Save(media);
            Assert.Equal(new byte[] { 4, 5, 6 }, File.ReadAllBytes(source));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void Playlist_PreservesDiskOrderInSessionCopy()
    {
        var root = CreateRoot();
        var first = Path.Combine(root, "first.st");
        var second = Path.Combine(root, "second.st");
        var playlist = Path.Combine(root, "set.m3u");
        File.WriteAllBytes(first, [1]);
        File.WriteAllBytes(second, [2]);
        File.WriteAllLines(playlist, ["# Atari disks", "second.st", "first.st"]);
        try
        {
            var media = AtariSessionMediaFunctions.Prepare(CreateConfiguration(playlist),
                Path.Combine(root, "session"), SupportedExtensions);

            Assert.Equal([Path.GetFullPath(second), Path.GetFullPath(first)], media.SourcePaths);
            Assert.Equal(new byte[] { 2 }, File.ReadAllBytes(media.RuntimePaths[0]));
            Assert.Equal(new byte[] { 1 }, File.ReadAllBytes(media.RuntimePaths[1]));
            Assert.Equal(media.RuntimePaths.Select(Path.GetFileName), File.ReadAllLines(media.RuntimePath));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void ReadOnlyMedia_IsNotCopiedAndCanBeReopenedExclusively()
    {
        var root = CreateRoot();
        var source = Path.Combine(root, "disk.st");
        File.WriteAllBytes(source, [1]);
        try
        {
            var configuration = CreateConfiguration(source) with { IsReadOnly = true };
            var media = AtariSessionMediaFunctions.Prepare(configuration,
                Path.Combine(root, "session"), SupportedExtensions);

            Assert.Equal(Path.GetFullPath(source), media.RuntimePath);
            using var stream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.None);
            Assert.Equal(1, stream.Length);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void WriteProtectedSource_IsOnlyModifiedInWritableSessionCopy()
    {
        var root = CreateRoot();
        var source = Path.Combine(root, "protected.st");
        File.WriteAllBytes(source, [1, 2]);
        File.SetAttributes(source, File.GetAttributes(source) | FileAttributes.ReadOnly);
        try
        {
            var media = AtariSessionMediaFunctions.Prepare(CreateConfiguration(source),
                Path.Combine(root, "session"), SupportedExtensions);
            File.WriteAllBytes(media.RuntimePath, [3, 4]);

            Assert.Equal(new byte[] { 1, 2 }, File.ReadAllBytes(source));
            Assert.Equal(new byte[] { 3, 4 }, File.ReadAllBytes(media.RuntimePath));
            Assert.True(media.RequiresExplicitSave);
        }
        finally
        {
            File.SetAttributes(source, FileAttributes.Normal);
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void PlaylistWithMissingDisk_IsRejected()
    {
        var root = CreateRoot();
        var playlist = Path.Combine(root, "set.m3u");
        File.WriteAllLines(playlist, ["missing.st"]);
        try
        {
            Assert.Throws<FileNotFoundException>(() => AtariSessionMediaFunctions.Prepare(
                CreateConfiguration(playlist), Path.Combine(root, "session"), SupportedExtensions));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    private static AtariMediaConfiguration CreateConfiguration(string path) =>
        new(path, AtariMediaCategory.Floppy, GWGUI.Emulation.EmulationMediaSlot.Floppy0);

    private static string CreateRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "GWGUI-Atari-SessionMedia", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }
}

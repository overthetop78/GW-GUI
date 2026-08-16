using System.IO;
using GWGUI.Emulation.Atari.Cores;
using GWGUI.Emulation.Common;

namespace GWGUI.Tests;

public sealed class AtariExternalCoreCallbackTests
{
    [Fact]
    public void InstallCallbacks_UsesRequiredOrder()
    {
        var calls = new List<string>();
        var exports = CreateExports(calls);
        var root = Path.Combine(Path.GetTempPath(), $"gwgui-atari-callbacks-{Guid.NewGuid():N}");
        try
        {
            using var callbacks = new AtariExternalHostCallbacks(
                Path.Combine(root, "system"), Path.Combine(root, "content"), Path.Combine(root, "saves"),
                new Dictionary<string, string>());

            AtariCoreFunctions.InstallCallbacks(exports, callbacks);

            Assert.Equal(new[] { "environment", "video", "audio-sample", "audio-batch", "input-poll", "input-state" }, calls);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    private static AtariExternalCoreExports CreateExports(ICollection<string> calls) => new(
        _ => calls.Add("environment"),
        _ => calls.Add("video"),
        _ => calls.Add("audio-sample"),
        _ => calls.Add("audio-batch"),
        _ => calls.Add("input-poll"),
        _ => calls.Add("input-state"),
        () => { }, () => { },
        (out ExternalCoreApi.SystemInfo info) => info = default,
        (out ExternalCoreApi.SystemAvInfo info) => info = default,
        (_, _) => { }, () => { }, () => { }, _ => true, () => { }, () => default,
        _ => default, _ => default, () => default, (_, _) => true, (_, _) => true);
}

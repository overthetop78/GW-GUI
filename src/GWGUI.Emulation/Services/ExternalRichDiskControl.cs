using System.IO;

namespace GWGUI.Emulation.Services;

internal sealed class ExternalRichDiskControl(IEmulationDiskControlPolicy policy, Func<string, IEmulationLoadedContent> createContent, Func<string, bool>? mediaExists = null) : ExternalCoreDiskControl(policy, mediaExists)
{
    protected override bool CountBeforeIndexValidation => false;
    protected override bool MediaExists(string path) => _mediaExists?.Invoke(path) ?? File.Exists(path);

    public override void Insert(string path)
    {
        var index = PrepareInsertion(path, out var wasEjected);
        using var content = createContent(path);
        var inserted = false;
        try
        {
            ReplaceAndInsert(index, content.GameInfo);
            inserted = true;
        }
        finally
        {
            if (!inserted && !wasEjected) SetEjectState!(false);
        }
    }
}

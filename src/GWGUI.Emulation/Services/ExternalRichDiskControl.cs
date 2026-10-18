using System.IO;

namespace GWGUI.Emulation.Services;

internal abstract class ExternalRichDiskControl<TContent> : ExternalCoreDiskControl where TContent : IDisposable
{
    internal bool IsEjected => Ejected;
    protected override bool CountBeforeIndexValidation => false;
    protected override bool MediaExists(string path) => File.Exists(path);
    protected abstract TContent CreateContent(string path);
    protected abstract nint GetGameInfo(TContent content);

    internal override void Insert(string path)
    {
        var index = PrepareInsertion(path, out var wasEjected);
        using var content = CreateContent(path);
        var inserted = false;
        try
        {
            ReplaceAndInsert(index, GetGameInfo(content));
            inserted = true;
        }
        finally
        {
            if (!inserted && !wasEjected) SetEjectState!(false);
        }
    }
}

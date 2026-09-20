using GWGUI.App.Localization.Extensions;
using GWGUI.MediaEngine.Operations;

namespace GWGUI.App.ViewModels.Conversion;

public sealed record FileMigrationLossRow(MigrationLoss Loss)
{
    public string Severity => LocExtension.Get(Loss.IsBlocking ? "Migration.Blocking" : "Migration.MetadataLoss");
    public string Description => LocExtension.Get($"Migration.Loss.{Loss.Kind}");
    public string Path => Loss.Path;
}

using GWGUI.App.Functions.Localization;
using GWGUI.MediaEngine.Migration;

namespace GWGUI.App.ViewModels.Conversion;

public sealed record FileMigrationTargetOption(FileSystemMigrationTarget Target)
{
    public string Label => FileMigrationTargetLocalizer.GetDisplayName(Target.FormatId);
}

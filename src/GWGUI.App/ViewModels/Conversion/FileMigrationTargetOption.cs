using GWGUI.MediaEngine.Contracts.Migration;
using GWGUI.App.Functions.Localization;

namespace GWGUI.App.ViewModels.Conversion;

public sealed record FileMigrationTargetOption(FileSystemMigrationTarget Target)
{
    public string Label => FileMigrationTargetLocalizer.GetDisplayName(Target.FormatId);
}

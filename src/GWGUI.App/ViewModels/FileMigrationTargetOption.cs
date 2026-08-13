using GWGUI.MediaEngine.Migration;
using GWGUI.App.Localization;

namespace GWGUI.App.ViewModels;

public sealed record FileMigrationTargetOption(FileSystemMigrationTarget Target)
{
    public string Label => FileMigrationTargetLocalization.GetDisplayName(Target.FormatId);
}

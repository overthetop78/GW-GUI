using GWGUI.Emulation.Epoch.Emulators.PD777.Constants;

namespace GWGUI.Emulation.Epoch.Emulators.PD777.Dictionaries;

internal static class PD777OptionCatalog
{
    internal static IReadOnlyList<CoreOption> All { get; } =
    [
        new(OptionConstants.OptionAnnounceCourseSwitch, "Input > Announce Course Switch", "Show the new setting of the Course Select Switch when changing it with up/down buttons.", "input",
            "enabled", "enabled", [new("disabled", "disabled", ResourceKey: SettingsChoiceConstants.Disabled), new("enabled", "enabled", ResourceKey: SettingsChoiceConstants.Enabled)],
            ResourceKey: SettingsTextConstants.AnnounceCourseSwitch, ExplanationResourceKey: SettingsTextConstants.AnnounceCourseSwitchHelp),
    ];
}

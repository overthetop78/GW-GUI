using GWGUI.Emulation.Snk.Emulators.Mame2015.Constants;

namespace GWGUI.Emulation.Snk.Emulators.Mame2015.Dictionaries;

internal static class OptionCatalog
{
    internal static IReadOnlyList<CoreOption> Options { get; } = Array.AsReadOnly(new CoreOption[] { new(
        RuntimeOptionConstants.ReadConfig,
        "Read configuration",
        "",
        EmulationMachineTab.General.ToString(),
        "disabled",
        "disabled",
        [new("disabled", "disabled"), new("enabled", "enabled")],
        IsVisible: false,
        RequiresRestart: false
    ), new(
        RuntimeOptionConstants.AutoSave,
        "Auto save/load states",
        "",
        EmulationMachineTab.General.ToString(),
        "disabled",
        "disabled",
        [new("disabled", "disabled"), new("enabled", "enabled")],
        IsVisible: true,
        RequiresRestart: false,
        ResourceKey: SettingsTextConstants.SettingAutoSaveLoadStates
    ), new(
        RuntimeOptionConstants.MouseMode,
        "XY device",
        "",
        EmulationMachineTab.Controllers.ToString(),
        "none",
        "none",
        [new("none", "none"), new("lightgun", "lightgun"), new("mouse", "mouse")],
        IsVisible: false,
        RequiresRestart: true
    ), new(RuntimeOptionConstants.Throttle, "Enable throttle", "", EmulationMachineTab.General.ToString(), "disabled", "disabled", [new("disabled", "disabled"), new("enabled", "enabled")], IsVisible: false, RequiresRestart: false), new(
        RuntimeOptionConstants.CheatsEnable,
        "Enable cheats",
        "",
        EmulationMachineTab.General.ToString(),
        "disabled",
        "disabled",
        [new("disabled", "disabled"), new("enabled", "enabled")],
        IsVisible: true,
        RequiresRestart: false,
        ResourceKey: SettingsTextConstants.SettingEnableCheats
    ), new(
        RuntimeOptionConstants.HideNagscreen,
        "Hide nag screen",
        "",
        EmulationMachineTab.General.ToString(),
        "disabled",
        "disabled",
        [new("disabled", "disabled"), new("enabled", "enabled")],
        IsVisible: true,
        RequiresRestart: false,
        ResourceKey: SettingsTextConstants.SettingHideNagScreen
    ), new(
        RuntimeOptionConstants.HideInfoscreen,
        "Hide gameinfo screen",
        "",
        EmulationMachineTab.General.ToString(),
        "disabled",
        "disabled",
        [new("disabled", "disabled"), new("enabled", "enabled")],
        IsVisible: true,
        RequiresRestart: false,
        ResourceKey: SettingsTextConstants.SettingHideGameInfoScreen
    ), new(
        RuntimeOptionConstants.HideWarnings,
        "Hide warnings screen",
        "",
        EmulationMachineTab.General.ToString(),
        "disabled",
        "disabled",
        [new("disabled", "disabled"), new("enabled", "enabled")],
        IsVisible: true,
        RequiresRestart: false,
        ResourceKey: SettingsTextConstants.SettingHideWarningsScreen
    ), new(
        RuntimeOptionConstants.AlternateRenderer,
        "Alternate render method",
        "",
        EmulationMachineTab.Video.ToString(),
        "disabled",
        "disabled",
        [new("disabled", "disabled"), new("enabled", "enabled")],
        IsVisible: true,
        RequiresRestart: false,
        ResourceKey: SettingsTextConstants.SettingAlternateRenderMethod
    ), new(RuntimeOptionConstants.BootToOsd, "Boot to OSD", "", EmulationMachineTab.General.ToString(), "disabled", "disabled", [new("disabled", "disabled"), new("enabled", "enabled")], IsVisible: false, RequiresRestart: false), new(
        RuntimeOptionConstants.BootFromCli,
        "Boot from CLI",
        "",
        EmulationMachineTab.General.ToString(),
        "disabled",
        "disabled",
        [new("disabled", "disabled"), new("enabled", "enabled")],
        IsVisible: false,
        RequiresRestart: false
    ), });
}

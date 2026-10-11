namespace GWGUI.Emulation.Snk.Emulators.Mame2003Plus.Constants;

internal static class RuntimeOptionConstants
{
    internal const string FourWayEmulation = "mame2003-plus_four_way_emulation";
    internal const string XyDevice = "mame2003-plus_xy_device";
    internal const string CrosshairEnabled = "mame2003-plus_crosshair_enabled";
    internal const string CrosshairAppearance = "mame2003-plus_crosshair_appearance";
    internal const string SkipDisclaimer = "mame2003-plus_skip_disclaimer";
    internal const string SkipWarnings = "mame2003-plus_skip_warnings";
    internal const string DisplaySetup = "mame2003-plus_display_setup";
    internal const string Brightness = "mame2003-plus_brightness";
    internal const string Gamma = "mame2003-plus_gamma";
    internal const string DisplayArtwork = "mame2003-plus_display_artwork";
    internal const string ArtResolution = "mame2003-plus_art_resolution";
    internal const string ArtOverlayOpacity = "mame2003-plus_art_overlay_opacity";
    internal const string NeogeoBios = "mame2003-plus_neogeo_bios";
    internal const string StvBios = "mame2003-plus_stv_bios";
    internal const string UseSamples = "mame2003-plus_use_samples";
    internal const string UseAltSound = "mame2003-plus_use_alt_sound";
    internal const string Dialsharexy = "mame2003-plus_dialsharexy";
    internal const string DialSwapXy = "mame2003-plus_dial_swap_xy";
    internal const string TateMode = "mame2003-plus_tate_mode";
    internal const string VectorResolution = "mame2003-plus_vector_resolution";
    internal const string VectorAntialias = "mame2003-plus_vector_antialias";
    internal const string VectorBeamWidth = "mame2003-plus_vector_beam_width";
    internal const string VectorTranslucency = "mame2003-plus_vector_translucency";
    internal const string VectorFlicker = "mame2003-plus_vector_flicker";
    internal const string VectorIntensity = "mame2003-plus_vector_intensity";
    internal const string NvramBootstraps = "mame2003-plus_nvram_bootstraps";
    internal const string SampleRate = "mame2003-plus_sample_rate";
    internal const string InputInterface = "mame2003-plus_input_interface";
    internal const string MameRemapping = "mame2003-plus_mame_remapping";
    internal const string Frameskip = "mame2003-plus_frameskip";
    internal const string CoreSysSubfolder = "mame2003-plus_core_sys_subfolder";
    internal const string CoreSaveSubfolder = "mame2003-plus_core_save_subfolder";
    internal const string AutosaveHiscore = "mame2003-plus_autosave_hiscore";
    internal const string CheatInputPorts = "mame2003-plus_cheat_input_ports";
    internal const string DigitalJoyCentering = "mame2003-plus_digital_joy_centering";
    internal const string CpuClockScale = "mame2003-plus_cpu_clock_scale";
    internal const string OverrideAdStick = "mame2003-plus_override_ad_stick";
    internal const string InputToggle = "mame2003-plus_input_toggle";
    internal const string Enabled = "enabled";
    internal const string Disabled = "disabled";
    internal const string Retropad = "retropad";
    internal const string SystemArchive = "neogeo.zip";
    internal const bool NeedsFullPath = true;
    internal static readonly IReadOnlyList<string> CartridgeExtensions = Array.AsReadOnly(new[] { ".zip" });
}

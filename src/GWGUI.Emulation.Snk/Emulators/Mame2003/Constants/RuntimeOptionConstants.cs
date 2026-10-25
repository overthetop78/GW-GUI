namespace GWGUI.Emulation.Snk.Emulators.Mame2003.Constants;

internal static class RuntimeOptionConstants
{
    internal const string FourWayEmulation = "mame2003_four_way_emulation";
    internal const string MouseDevice = "mame2003_mouse_device";
    internal const string CrosshairEnabled = "mame2003_crosshair_enabled";
    internal const string SkipDisclaimer = "mame2003_skip_disclaimer";
    internal const string SkipWarnings = "mame2003_skip_warnings";
    internal const string DisplaySetup = "mame2003_display_setup";
    internal const string Brightness = "mame2003_brightness";
    internal const string Gamma = "mame2003_gamma";
    internal const string DisplayArtwork = "mame2003_display_artwork";
    internal const string ArtResolution = "mame2003_art_resolution";
    internal const string NeogeoBios = "mame2003_neogeo_bios";
    internal const string StvBios = "mame2003_stv_bios";
    internal const string UseSamples = "mame2003_use_samples";
    internal const string UseAltSound = "mame2003_use_alt_sound";
    internal const string Dialsharexy = "mame2003_dialsharexy";
    internal const string DualJoysticks = "mame2003_dual_joysticks";
    internal const string RstickToBtns = "mame2003_rstick_to_btns";
    internal const string TateMode = "mame2003_tate_mode";
    internal const string VectorResolution = "mame2003_vector_resolution";
    internal const string VectorAntialias = "mame2003_vector_antialias";
    internal const string VectorBeamWidth = "mame2003_vector_beam_width";
    internal const string VectorTranslucency = "mame2003_vector_translucency";
    internal const string VectorFlicker = "mame2003_vector_flicker";
    internal const string VectorIntensity = "mame2003_vector_intensity";
    internal const string NvramBootstraps = "mame2003_nvram_bootstraps";
    internal const string SampleRate = "mame2003_sample_rate";
    internal const string DcsSpeedhack = "mame2003_dcs_speedhack";
    internal const string QsoundOutputFilter = "mame2003_qsound_output_filter";
    internal const string InputInterface = "mame2003_input_interface";
    internal const string MameRemapping = "mame2003_mame_remapping";
    internal const string Frameskip = "mame2003_frameskip";
    internal const string CoreSysSubfolder = "mame2003_core_sys_subfolder";
    internal const string CoreSaveSubfolder = "mame2003_core_save_subfolder";
    internal const string AutosaveHiscore = "mame2003_autosave_hiscore";
    internal const string CheatInputPorts = "mame2003_cheat_input_ports";
    internal const string MachineTiming = "mame2003_machine_timing";
    internal const string CpuClockScale = "mame2003_cpu_clock_scale";
    internal const string Enabled = "enabled";
    internal const string Disabled = "disabled";
    internal const string Retropad = "retropad";
    internal const string SystemArchive = "neogeo.zip";
    internal const bool NeedsFullPath = true;
    internal static readonly IReadOnlyList<string> CartridgeExtensions = Array.AsReadOnly(new[] { ".zip" });
}

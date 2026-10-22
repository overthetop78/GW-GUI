namespace GWGUI.Emulation.Constants;

/// <summary>Noms invariants des exports du contrat natif, partagés par les adaptateurs indépendants.</summary>
public static class ExternalCoreExportNames
{
    public const string ApiVersion = "retro_api_version";
    public const string SetEnvironment = "retro_set_environment";
    public const string SetVideoRefresh = "retro_set_video_refresh";
    public const string SetAudioSample = "retro_set_audio_sample";
    public const string SetAudioSampleBatch = "retro_set_audio_sample_batch";
    public const string SetInputPoll = "retro_set_input_poll";
    public const string SetInputState = "retro_set_input_state";
    public const string Initialize = "retro_init";
    public const string Deinitialize = "retro_deinit";
    public const string GetSystemInfo = "retro_get_system_info";
    public const string GetSystemAvInfo = "retro_get_system_av_info";
    public const string SetControllerPortDevice = "retro_set_controller_port_device";
    public const string Reset = "retro_reset";
    public const string Run = "retro_run";
    public const string LoadGame = "retro_load_game";
    public const string LoadGameSpecial = "retro_load_game_special";
    public const string UnloadGame = "retro_unload_game";
    public const string GetRegion = "retro_get_region";
    public const string GetMemoryData = "retro_get_memory_data";
    public const string GetMemorySize = "retro_get_memory_size";
    public const string GetSerializedSize = "retro_serialize_size";
    public const string Serialize = "retro_serialize";
    public const string Unserialize = "retro_unserialize";
}

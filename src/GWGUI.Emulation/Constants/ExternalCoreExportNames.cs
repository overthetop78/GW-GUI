namespace GWGUI.Emulation.Constants;

internal static class ExternalCoreExportNames
{
    internal const string ApiVersion = "retro_api_version";
    internal const string SetEnvironment = "retro_set_environment";
    internal const string SetVideoRefresh = "retro_set_video_refresh";
    internal const string SetAudioSample = "retro_set_audio_sample";
    internal const string SetAudioSampleBatch = "retro_set_audio_sample_batch";
    internal const string SetInputPoll = "retro_set_input_poll";
    internal const string SetInputState = "retro_set_input_state";
    internal const string Initialize = "retro_init";
    internal const string Deinitialize = "retro_deinit";
    internal const string GetSystemInfo = "retro_get_system_info";
    internal const string GetSystemAvInfo = "retro_get_system_av_info";
    internal const string SetControllerPortDevice = "retro_set_controller_port_device";
    internal const string Reset = "retro_reset";
    internal const string Run = "retro_run";
    internal const string LoadGame = "retro_load_game";
    internal const string UnloadGame = "retro_unload_game";
    internal const string GetRegion = "retro_get_region";
    internal const string GetMemoryData = "retro_get_memory_data";
    internal const string GetMemorySize = "retro_get_memory_size";
    internal const string GetSerializedSize = "retro_serialize_size";
    internal const string Serialize = "retro_serialize";
    internal const string Unserialize = "retro_unserialize";
}

namespace GWGUI.Emulation.Contracts;

public sealed record EmulationRuntimeServices(
    string SessionsDirectory,
    string StatesDirectory,
    string ConvertedMediaDirectory,
    string HostExecutablePath,
    Func<string?, int, IAudioOutput> CreateAudioOutput);

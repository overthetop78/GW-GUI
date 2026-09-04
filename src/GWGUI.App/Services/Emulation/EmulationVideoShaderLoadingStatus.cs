namespace GWGUI.App.Services.Emulation;

internal sealed record EmulationVideoShaderLoadingChangedEventArgs(
    string ModuleId, Guid ConfigurationId, bool IsLoading);

internal static class EmulationVideoShaderLoadingStatus
{
    private static readonly Lock Gate = new();
    private static readonly HashSet<(string ModuleId, Guid ConfigurationId)> Loading = [];

    internal static event EventHandler<EmulationVideoShaderLoadingChangedEventArgs>? Changed;

    internal static bool IsLoading(string moduleId, Guid configurationId)
    {
        lock (Gate) return Loading.Contains((moduleId, configurationId));
    }

    internal static void Set(string moduleId, Guid configurationId, bool isLoading)
    {
        bool changed;
        lock (Gate)
        {
            changed = isLoading
                ? Loading.Add((moduleId, configurationId))
                : Loading.Remove((moduleId, configurationId));
        }
        if (changed)
            Changed?.Invoke(null, new EmulationVideoShaderLoadingChangedEventArgs(
                moduleId, configurationId, isLoading));
    }
}

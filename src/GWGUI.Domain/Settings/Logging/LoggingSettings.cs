namespace GWGUI.Domain.Settings.Logging;

public sealed class OperationLogSettings
{
    public bool Enabled { get; set; } = true;
    public int MaximumKilobytes { get; set; } = 1024;
    public bool KeepArchives { get; set; }
    public Dictionary<string, ActionLogSettings> Actions { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public ActionLogSettings ForAction(string action) => Actions.TryGetValue(action, out var settings)
        ? settings
        : new ActionLogSettings { Enabled = Enabled, MaximumKilobytes = MaximumKilobytes, KeepArchives = KeepArchives };

    public ActionLogSettings GetOrCreate(string action)
    {
        if (!Actions.TryGetValue(action, out var settings))
            Actions[action] = settings = ForAction(action);
        return settings;
    }
}

public sealed class ActionLogSettings
{
    public bool Enabled { get; set; } = true;
    public int MaximumKilobytes { get; set; } = 1024;
    public bool KeepArchives { get; set; }
}

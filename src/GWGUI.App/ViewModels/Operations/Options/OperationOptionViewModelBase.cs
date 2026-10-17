using EnabledOption = global::GWGUI.MediaEngine.Contracts.Options.EnabledOption;
using System.ComponentModel;

namespace GWGUI.App.ViewModels.Operations.Options;

public abstract class OperationOptionViewModelBase(string argument) : INotifyPropertyChanged
{
    private bool _enabled;
    public string Argument { get; } = argument;
    public bool Enabled { get => _enabled; set { if (_enabled == value) return; _enabled = value; PropertyChanged?.Invoke(this, new(nameof(Enabled))); } }
    public abstract EnabledOption ToEnabledOption();
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void Changed(string propertyName) => PropertyChanged?.Invoke(this, new(propertyName));

    internal static HashSet<string> CaptureEnabledOptions(IEnumerable<OperationOptionViewModelBase> options) => options.Where(x => x.Enabled).Select(x => x.Argument.TrimStart('-')).ToHashSet();

    internal static Dictionary<string, string> CaptureValues(IEnumerable<OperationOptionViewModelBase> options, string expertArguments)
    {
        var result = options.OfType<ValueOptionViewModel>().ToDictionary(x => x.Argument.TrimStart('-'), x => x.Value);
        result["expert"] = expertArguments; return result;
    }

    internal static string ApplyOptions(IEnumerable<OperationOptionViewModelBase> options, IReadOnlySet<string> enabled, IReadOnlyDictionary<string, string> values)
    {
        foreach (var option in options)
        {
            var key = option.Argument.TrimStart('-'); option.Enabled = enabled.Contains(key);
            if (option is ValueOptionViewModel valued && values.TryGetValue(key, out var value)) valued.Value = value;
        }
        return values.GetValueOrDefault("expert", "");
    }
}

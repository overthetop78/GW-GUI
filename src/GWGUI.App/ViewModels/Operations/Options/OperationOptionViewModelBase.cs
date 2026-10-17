using GWGUI.App.ViewModels;
using EnabledOption = global::GWGUI.MediaEngine.Contracts.Options.EnabledOption;

namespace GWGUI.App.ViewModels.Operations.Options;

public abstract class OperationOptionViewModelBase(string argument) : ObservableObject
{
    private bool _enabled;
    public string Argument { get; } = argument;
    public bool Enabled { get => _enabled; set => Set(ref _enabled, value); }
    public abstract EnabledOption ToEnabledOption();
    protected void Changed(string propertyName) => OnPropertyChanged(propertyName);

    internal static HashSet<string> CaptureEnabledOptions(IEnumerable<OperationOptionViewModelBase> options) => options.Where(option => option.Enabled).Select(option => option.Argument.TrimStart('-')).ToHashSet();

    internal static Dictionary<string, string> CaptureValues(IEnumerable<OperationOptionViewModelBase> options, string expertArguments)
    {
        var values = options.OfType<ValueOptionViewModel>().ToDictionary(option => option.Argument.TrimStart('-'), option => option.Value);
        values["expert"] = expertArguments;
        return values;
    }

    internal static string ApplyOptions(IEnumerable<OperationOptionViewModelBase> options, IReadOnlySet<string> enabled, IReadOnlyDictionary<string, string> values)
    {
        foreach (var option in options)
        {
            var key = option.Argument.TrimStart('-');
            option.Enabled = enabled.Contains(key);
            if (option is ValueOptionViewModel valueOption && values.TryGetValue(key, out var value)) valueOption.Value = value;
        }
        return values.GetValueOrDefault("expert", "");
    }

    internal static IReadOnlyList<EnabledOption> BuildOptions(IEnumerable<OperationOptionViewModelBase> options,
        OperationOptionViewModelBase? excluded = null) =>
        options.Where(option => option.Enabled && option != excluded).Select(option => option.ToEnabledOption()).ToArray();
}

using GWGUI.Domain.Commands.Options;
using GWGUI.Domain.Conversion;
using GWGUI.Domain.Formats;
using GWGUI.App.ViewModels.Operations.Options;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GWGUI.App.ViewModels.Conversion;

public sealed class ConversionOperationViewModel : INotifyPropertyChanged
{
    private string _sourcePath = "";
    private string _outputName = "";
    private bool _addTags;
    private string _expertArguments = "";
    private HashSet<string> _selectedFormats = [];
    private Dictionary<string, HashSet<string>> _explicitExtensions = [];

    public string SourcePath { get => _sourcePath; set => Set(ref _sourcePath, value); }
    public string OutputName { get => _outputName; set => Set(ref _outputName, value); }
    public bool AddTags { get => _addTags; set => Set(ref _addTags, value); }
    public string ExpertArguments { get => _expertArguments; set => Set(ref _expertArguments, value); }
    public ValueOptionViewModel Tracks { get; } = new("--tracks", "c=0-79:h=0-1");
    public ValueOptionViewModel OutputTracks { get; } = new("--out-tracks", "c=0-79:h=0-1");
    public ValueOptionViewModel AdjustSpeed { get; } = new("--adjust-speed", "300rpm");
    public ValueOptionViewModel Pll { get; } = new("--pll", "period=5:phase=60");
    public FlagOptionViewModel HardSectors { get; } = new("--hard-sectors");
    public FlagOptionViewModel Reverse { get; } = new("--reverse");
    public ValueOptionViewModel DiskDefs { get; } = new("--diskdefs", "");
    public IReadOnlySet<string> SelectedFormats => _selectedFormats;
    public IReadOnlyDictionary<string, HashSet<string>> ExplicitExtensions => _explicitExtensions;

    public IReadOnlyList<EnabledOption> BuildOptions() => AllOptions().Where(x => x.Enabled).Select(x => x.ToEnabledOption()).ToArray();

    public void SetFormat(string formatId, bool selected, IEnumerable<string>? extensions)
    {
        if (selected) _selectedFormats.Add(formatId); else _selectedFormats.Remove(formatId);
        var values = extensions?.Where(x => !string.IsNullOrWhiteSpace(x)).ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];
        if (values.Count == 0) _explicitExtensions.Remove(formatId); else _explicitExtensions[formatId] = values;
    }

    public IEnumerable<ConversionSelection> BuildSelections(IEnumerable<DiskFormat> formats) => formats
        .Where(format => _selectedFormats.Contains(format.Id))
        .Select(format => new ConversionSelection(format.Id, _explicitExtensions.GetValueOrDefault(format.Id) ?? []));

    public void ApplyProfile(IReadOnlySet<string> enabled, IReadOnlyDictionary<string, string> values)
    {
        ApplyOptions(enabled, values);
        AddTags = enabled.Contains("tags");
        _selectedFormats = enabled.Where(x => x.StartsWith("format:", StringComparison.Ordinal)).Select(x => x[7..]).ToHashSet();
        _explicitExtensions = values.Where(x => x.Key.StartsWith("extensions:", StringComparison.Ordinal))
            .ToDictionary(x => x.Key[11..], x => x.Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToHashSet(StringComparer.OrdinalIgnoreCase));
    }

    public void ApplySettings(bool addTags, IReadOnlySet<string> selectedFormats, IReadOnlyDictionary<string, HashSet<string>> extensions, IReadOnlySet<string> enabled, IReadOnlyDictionary<string, string> values)
    {
        AddTags = addTags; _selectedFormats = selectedFormats.ToHashSet();
        _explicitExtensions = extensions.ToDictionary(x => x.Key, x => x.Value.ToHashSet(StringComparer.OrdinalIgnoreCase));
        ApplyOptions(enabled, values);
    }

    public HashSet<string> CaptureProfileEnabled()
    {
        var result = CaptureEnabledOptions();
        if (AddTags) result.Add("tags");
        foreach (var format in _selectedFormats) result.Add("format:" + format);
        return result;
    }

    public Dictionary<string, string> CaptureProfileValues()
    {
        var result = CaptureValues();
        foreach (var pair in _explicitExtensions.Where(x => x.Value.Count > 0)) result["extensions:" + pair.Key] = string.Join(',', pair.Value);
        return result;
    }

    public HashSet<string> CaptureEnabledOptions() => AllOptions().Where(x => x.Enabled).Select(x => x.Argument.TrimStart('-')).ToHashSet();
    public Dictionary<string, string> CaptureValues()
    {
        var result = AllOptions().OfType<ValueOptionViewModel>().ToDictionary(x => x.Argument.TrimStart('-'), x => x.Value);
        result["expert"] = ExpertArguments; return result;
    }

    private void ApplyOptions(IReadOnlySet<string> enabled, IReadOnlyDictionary<string, string> values)
    {
        foreach (var option in AllOptions())
        {
            var key = option.Argument.TrimStart('-'); option.Enabled = enabled.Contains(key);
            if (option is ValueOptionViewModel valued && values.TryGetValue(key, out var value)) valued.Value = value;
        }
        ExpertArguments = values.GetValueOrDefault("expert", "");
    }

    private IEnumerable<OperationOptionViewModelBase> AllOptions() => [Tracks, OutputTracks, AdjustSpeed, Pll, HardSectors, Reverse, DiskDefs];
    public event PropertyChangedEventHandler? PropertyChanged;
    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null) { if (EqualityComparer<T>.Default.Equals(field, value)) return; field = value; PropertyChanged?.Invoke(this, new(name)); }
}

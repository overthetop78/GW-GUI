using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using GWGUI.Domain.Naming;
using GWGUI.Domain.Read;

namespace GWGUI.App.ViewModels;

public sealed class ReadOperationViewModel : INotifyPropertyChanged
{
    private string _fileName = "";
    private string _folder = "";
    private bool _autoNumber;
    private int _sequenceKindIndex;
    private int _sequenceWidthIndex;
    private string _sequenceValue = "1";
    private string _expertArguments = "";

    public ValueOptionViewModel Revs { get; } = new("--revs", "5");
    public ValueOptionViewModel Retries { get; } = new("--retries", "5");
    public ValueOptionViewModel Tracks { get; } = new("--tracks", "c=0-79:h=0-1");
    public ValueOptionViewModel SeekRetries { get; } = new("--seek-retries", "0");
    public ValueOptionViewModel FakeIndex { get; } = new("--fake-index", "300rpm");
    public FlagOptionViewModel HardSectors { get; } = new("--hard-sectors");
    public ValueOptionViewModel AdjustSpeed { get; } = new("--adjust-speed", "300rpm");
    public ValueOptionViewModel Pll { get; } = new("--pll", "period=5:phase=60");
    public FlagOptionViewModel Reverse { get; } = new("--reverse");
    public ValueOptionViewModel Densel { get; } = new("--densel", "H");
    public FlagOptionViewModel Tg43 { get; } = new("--gen-tg43");
    public ValueOptionViewModel DiskDefs { get; } = new("--diskdefs", "");

    public string FileName { get => _fileName; set => Set(ref _fileName, value); }
    public string Folder { get => _folder; set => Set(ref _folder, value); }
    public bool AutoNumber { get => _autoNumber; set => Set(ref _autoNumber, value); }
    public int SequenceKindIndex { get => _sequenceKindIndex; set => Set(ref _sequenceKindIndex, value); }
    public int SequenceWidthIndex { get => _sequenceWidthIndex; set => Set(ref _sequenceWidthIndex, value); }
    public string SequenceValue { get => _sequenceValue; set => Set(ref _sequenceValue, value); }
    public string ExpertArguments { get => _expertArguments; set => Set(ref _expertArguments, value); }

    public SequenceKind SequenceKind => SequenceKindIndex == 1 ? SequenceKind.Alphabetic : SequenceKind.Numeric;

    public string BuildTarget(string extension, string exampleName)
    {
        var name = string.IsNullOrWhiteSpace(FileName) ? exampleName : FileName.Trim();
        if (AutoNumber && SequenceFormatter.TryParse(SequenceValue, SequenceKind, out var sequence))
            name += " " + SequenceFormatter.Format(sequence, SequenceKind, SequenceWidthIndex + 1);
        return Path.Combine(Folder, name + extension);
    }

    public bool TryAdvanceSequence()
    {
        if (!AutoNumber || !SequenceFormatter.TryParse(SequenceValue, SequenceKind, out var value)) return false;
        SequenceValue = SequenceKind == SequenceKind.Numeric
            ? (value + 1).ToString()
            : SequenceFormatter.Format(value + 1, SequenceKind, 1);
        return true;
    }

    public IReadOnlyList<EnabledOption> BuildOptions() =>
        AllOptions()
            .Where(option => option.Enabled)
            .Select(option => option.ToEnabledOption())
            .ToArray();

    public void ResetOptionalSettings()
    {
        foreach (var option in AllOptions())
            option.Enabled = false;
        ExpertArguments = "";
    }

    public void EnableFakeIndex() { FakeIndex.Enabled = true; HardSectors.Enabled = false; }
    public void EnableHardSectors() { HardSectors.Enabled = true; FakeIndex.Enabled = false; }
    public void EnableDensel() { Densel.Enabled = true; Tg43.Enabled = false; }
    public void EnableTg43() { Tg43.Enabled = true; Densel.Enabled = false; }

    public void ApplyOptions(IReadOnlySet<string> enabled, IReadOnlyDictionary<string, string> values)
    {
        foreach (var option in AllOptions())
        {
            var key = option.Argument.TrimStart('-');
            option.Enabled = enabled.Contains(key);
            if (option is ValueOptionViewModel valueOption && values.TryGetValue(key, out var value)) valueOption.Value = value;
        }
        ExpertArguments = values.GetValueOrDefault("expert", "");
    }

    public HashSet<string> CaptureEnabledOptions() => AllOptions().Where(option => option.Enabled).Select(option => option.Argument.TrimStart('-')).ToHashSet();

    public Dictionary<string, string> CaptureValues()
    {
        var values = AllOptions().OfType<ValueOptionViewModel>().ToDictionary(option => option.Argument.TrimStart('-'), option => option.Value);
        values["expert"] = ExpertArguments;
        return values;
    }

    private IEnumerable<OperationOptionViewModelBase> AllOptions() => [Revs, Retries, Tracks, SeekRetries, FakeIndex, HardSectors, AdjustSpeed, Pll, Reverse, Densel, Tg43, DiskDefs];

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        if (propertyName == nameof(SequenceKindIndex)) PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SequenceKind)));
    }
}

using System.ComponentModel;
using System.Runtime.CompilerServices;
using GWGUI.Domain.Read;

namespace GWGUI.App.ViewModels;

public sealed class WriteOperationViewModel : INotifyPropertyChanged
{
    private string _sourcePath = "";
    private string _expertArguments = "";

    public string SourcePath { get => _sourcePath; set => Set(ref _sourcePath, value); }
    public string ExpertArguments { get => _expertArguments; set => Set(ref _expertArguments, value); }
    public FlagOptionViewModel InternalWriter { get; } = new("--internal-writer");
    public FlagOptionViewModel NoVerify { get; } = new("--no-verify");
    public FlagOptionViewModel EraseEmpty { get; } = new("--erase-empty");
    public ValueOptionViewModel Retries { get; } = new("--retries", "3");
    public ValueOptionViewModel Tracks { get; } = new("--tracks", "c=0-79:h=0-1");
    public FlagOptionViewModel PreErase { get; } = new("--pre-erase");
    public ValueOptionViewModel FakeIndex { get; } = new("--fake-index", "300rpm");
    public FlagOptionViewModel HardSectors { get; } = new("--hard-sectors");
    public ValueOptionViewModel Precomp { get; } = new("--precomp", "type=mfm:40=125");
    public FlagOptionViewModel Reverse { get; } = new("--reverse");
    public ValueOptionViewModel Densel { get; } = new("--densel", "H");
    public FlagOptionViewModel Tg43 { get; } = new("--gen-tg43");
    public ValueOptionViewModel DiskDefs { get; } = new("--diskdefs", "");

    public bool DisableVerification => NoVerify.Enabled;
    public IReadOnlyList<EnabledOption> BuildOptions() => AllOptions().Where(x => x.Enabled && x != NoVerify && x != InternalWriter).Select(x => x.ToEnabledOption()).ToArray();
    public void EnableFakeIndex() { FakeIndex.Enabled = true; HardSectors.Enabled = false; }
    public void EnableHardSectors() { HardSectors.Enabled = true; FakeIndex.Enabled = false; }
    public void EnableDensel() { Densel.Enabled = true; Tg43.Enabled = false; }
    public void EnableTg43() { Tg43.Enabled = true; Densel.Enabled = false; }

    public void ApplyOptions(IReadOnlySet<string> enabled, IReadOnlyDictionary<string, string> values)
    {
        foreach (var option in AllOptions())
        {
            var key = option.Argument.TrimStart('-'); option.Enabled = enabled.Contains(key);
            if (option is ValueOptionViewModel valued && values.TryGetValue(key, out var value)) valued.Value = value;
        }
        ExpertArguments = values.GetValueOrDefault("expert", "");
    }

    public HashSet<string> CaptureEnabledOptions() => AllOptions().Where(x => x.Enabled).Select(x => x.Argument.TrimStart('-')).ToHashSet();
    public Dictionary<string, string> CaptureValues()
    {
        var result = AllOptions().OfType<ValueOptionViewModel>().ToDictionary(x => x.Argument.TrimStart('-'), x => x.Value);
        result["expert"] = ExpertArguments; return result;
    }

    private IEnumerable<OperationOptionViewModelBase> AllOptions() => [InternalWriter, NoVerify, EraseEmpty, Retries, Tracks, PreErase, FakeIndex, HardSectors, Precomp, Reverse, Densel, Tg43, DiskDefs];
    public event PropertyChangedEventHandler? PropertyChanged;
    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null) { if (EqualityComparer<T>.Default.Equals(field, value)) return; field = value; PropertyChanged?.Invoke(this, new(name)); }
}

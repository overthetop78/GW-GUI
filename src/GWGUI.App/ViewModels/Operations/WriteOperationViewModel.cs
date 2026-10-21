using GWGUI.App.ViewModels;
using EnabledOption = GWGUI.MediaEngine.Contracts.Options.EnabledOption;
using GWGUI.App.ViewModels.Operations.Options;

namespace GWGUI.App.ViewModels.Operations;

public sealed class WriteOperationViewModel : ObservableObject
{
    private string _sourcePath = "";
    private string _expertArguments = "";

    public string SourcePath { get => _sourcePath; set => Set(ref _sourcePath, value); }
    public string ExpertArguments { get => _expertArguments; set => Set(ref _expertArguments, value); }
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
    public IReadOnlyList<EnabledOption> BuildOptions() => OperationOptionViewModelBase.BuildOptions(AllOptions(), NoVerify);
    public void EnableFakeIndex() { FakeIndex.Enabled = true; HardSectors.Enabled = false; }
    public void EnableHardSectors() { HardSectors.Enabled = true; FakeIndex.Enabled = false; }
    public void EnableDensel() { Densel.Enabled = true; Tg43.Enabled = false; }
    public void EnableTg43() { Tg43.Enabled = true; Densel.Enabled = false; }

    public void ApplyOptions(IReadOnlySet<string> enabled, IReadOnlyDictionary<string, string> values)
        => ExpertArguments = OperationOptionViewModelBase.ApplyOptions(AllOptions(), enabled, values);

    public HashSet<string> CaptureEnabledOptions() => OperationOptionViewModelBase.CaptureEnabledOptions(AllOptions());
    public Dictionary<string, string> CaptureValues()
        => OperationOptionViewModelBase.CaptureValues(AllOptions(), ExpertArguments);

    private IEnumerable<OperationOptionViewModelBase> AllOptions() => [NoVerify, EraseEmpty, Retries, Tracks, PreErase, FakeIndex, HardSectors, Precomp, Reverse, Densel, Tg43, DiskDefs];
}

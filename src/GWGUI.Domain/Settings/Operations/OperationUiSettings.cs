using GWGUI.Domain.Formats;
using GWGUI.Domain.Naming;
namespace GWGUI.Domain.Settings.Operations;

public sealed class ReadUiSettings
{
    public bool UseKnownFormat { get; set; }
    public string? FormatId { get; set; }
    public string? ImageExtension { get; set; }
    public bool AutoNumber { get; set; }
    public string SequenceKind { get; set; } = "Numeric";
    public int SequenceWidth { get; set; } = 1;
    public long NextSequence { get; set; } = 1;
    public Dictionary<string, string> OptionValues { get; set; } = new();
    public HashSet<string> EnabledOptions { get; set; } = [];
}

public sealed class AdvancedUiSettings
{
    public Dictionary<string, string> OptionValues { get; set; } = new();
    public HashSet<string> EnabledOptions { get; set; } = [];
}

public sealed class ConversionUiSettings
{
    public bool AddTags { get; set; }
    public string TagPattern { get; set; } = "[{FAMILY}-{FORMAT}] ";
    public List<string> RecentCustomTagPatterns { get; set; } = [];
    public HashSet<string> SelectedFormats { get; set; } = [];
    public Dictionary<string, HashSet<string>> ExplicitExtensions { get; set; } = new();
    public Dictionary<string, string> OptionValues { get; set; } = new();
    public HashSet<string> EnabledOptions { get; set; } = [];
}

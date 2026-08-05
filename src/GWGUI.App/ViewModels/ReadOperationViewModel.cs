using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using GWGUI.Domain.Naming;

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

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        if (propertyName == nameof(SequenceKindIndex)) PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SequenceKind)));
    }
}

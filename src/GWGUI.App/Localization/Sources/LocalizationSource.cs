using GWGUI.App.Localization.Extensions;
using System.ComponentModel;

namespace GWGUI.App.Localization.Sources;

public sealed class LocalizationSource : INotifyPropertyChanged
{
    public static LocalizationSource Instance { get; } = new();
    public string this[string key] => LocExtension.Get(key);
    public event PropertyChangedEventHandler? PropertyChanged;
    public void Refresh() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
}

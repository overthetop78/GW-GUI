using GWGUI.App.Localization.Extensions;
using System.ComponentModel;
using System.Globalization;

namespace GWGUI.App.Localization.Sources;

public sealed class LocalizationSource : INotifyPropertyChanged
{
    public static LocalizationSource Instance { get; } = new();
    public CultureInfo Culture { get; private set; } = CultureInfo.CurrentCulture;
    public CultureInfo UiCulture { get; private set; } = CultureInfo.CurrentUICulture;
    public int Version { get; private set; }
    public event PropertyChangedEventHandler? PropertyChanged;
    public void SetCultures(CultureInfo culture, CultureInfo uiCulture, bool refresh = true)
    {
        Culture = culture;
        UiCulture = uiCulture;
        if (refresh) Refresh();
    }

    public void Refresh()
    {
        Version++;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Version)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
    }
}

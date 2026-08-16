using System.Windows.Controls;

namespace GWGUI.App.Controls;

public sealed class EmulationSection : UserControl
{
    private readonly AmigaEmulationSection _amiga = new();
    private readonly AtariEmulationSection _atari = new();

    public EmulationSection()
    {
        Content = new TabControl
        {
            Items =
            {
                Tab(AtariEmulationConstants.AmigaTitle, _amiga),
                Tab(AtariEmulationConstants.AtariTitle, _atari)
            }
        };
    }

    public void Configure(GWGUI.Domain.Settings.AppSettings settings)
    {
        _amiga.Configure(settings);
        _atari.Configure(settings);
    }
    public async Task StopAllAsync()
    {
        await _amiga.StopAllAsync();
        await _atari.StopAllAsync();
    }

    private static TabItem Tab(string title, object content)
    {
        var item = new TabItem
        {
            Header = title, Content = content,
            Padding = new System.Windows.Thickness(18, 9, 18, 9)
        };
        item.SetResourceReference(StyleProperty, AtariEmulationConstants.MainTabItemStyleResource);
        return item;
    }
}

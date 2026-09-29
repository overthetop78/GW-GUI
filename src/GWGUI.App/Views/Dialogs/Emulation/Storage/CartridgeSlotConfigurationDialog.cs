using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Constants.Localization;
using GWGUI.App.Functions.Views.Emulation.Storage;
using GWGUI.App.Localization.Extensions;
using System.Windows;
using System.Windows.Controls;
using GWGUI.Emulation;

namespace GWGUI.App.Views.Dialogs.Emulation.Storage;

public sealed class CartridgeSlotConfigurationDialog : Window
{
    private readonly CheckBox _option = new();

    public CartridgeSlotSettings Settings => new(_option.IsChecked == true);

    public CartridgeSlotConfigurationDialog(string identifier, string machineName,
        CartridgeSlotSettings settings, string optionLabel, string? optionHint)
    {
        Title = $"{LocExtension.Get(EmulationResourceKeys.StorageDeviceConfigure)} {identifier}";
        Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive);
        ShowInTaskbar = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Width = 720;
        SizeToContent = SizeToContent.Height;
        ResizeMode = ResizeMode.NoResize;
        _option.Content = optionLabel;
        _option.IsChecked = settings.OptionEnabled;
        _option.Margin = new Thickness(0, 4, 0, 4);

        var body = new StackPanel();
        if (!string.IsNullOrWhiteSpace(optionLabel)) body.Children.Add(_option);
        if (!string.IsNullOrWhiteSpace(optionHint)) body.Children.Add(StorageDialogUi.Info(optionHint));
        Content = StorageDialogUi.DialogLayout(
            StorageDialogUi.DialogHeader(IconGlyphs.Controller, Title,
                $"{LocExtension.Get(EmulationResourceKeys.CartridgeDevice)} · {machineName}"),
            StorageDialogUi.Card(LocExtension.Get(EmulationResourceKeys.CartridgeDevice), body),
            StorageDialogUi.Footer(this, LocExtension.Get("Common.Save")));
    }
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using GWGUI.App.Localization;
using GWGUI.Emulation.Atari;

namespace GWGUI.App.Controls;

internal static class AtariFirmwarePresentation
{
    internal static DataTemplate CreateTemplate()
    {
        var row = new FrameworkElementFactory(typeof(StackPanel));
        row.SetValue(FrameworkElement.MarginProperty, new Thickness(4, 6, 4, 6));
        row.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        var name = new FrameworkElementFactory(typeof(TextBlock));
        name.SetBinding(TextBlock.TextProperty, new Binding(nameof(AtariScannedFirmware.Path))
        {
            Converter = new FileNameValueConverter()
        });
        name.SetValue(TextBlock.FontWeightProperty, FontWeights.SemiBold);
        name.SetValue(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis);
        name.SetValue(FrameworkElement.WidthProperty, 260d);
        name.SetValue(FrameworkElement.MarginProperty, new Thickness(4, 0, 8, 0));
        row.AppendChild(name);
        var version = new FrameworkElementFactory(typeof(TextBlock));
        version.SetBinding(TextBlock.TextProperty, new Binding("Definition.Version")
        {
            TargetNullValue = LocExtension.Get("Common.Unknown")
        });
        version.SetValue(FrameworkElement.WidthProperty, 120d);
        version.SetValue(FrameworkElement.MarginProperty, new Thickness(8, 0, 8, 0));
        row.AppendChild(version);
        var compatibility = new FrameworkElementFactory(typeof(TextBlock));
        compatibility.SetBinding(TextBlock.TextProperty, new Binding(nameof(AtariScannedFirmware.Compatibility))
        {
            Converter = new CompatibilityValueConverter()
        });
        compatibility.SetValue(FrameworkElement.WidthProperty, 155d);
        compatibility.SetValue(FrameworkElement.MarginProperty, new Thickness(8, 0, 4, 0));
        compatibility.SetValue(TextBlock.FontWeightProperty, FontWeights.SemiBold);
        row.AppendChild(compatibility);
        return new DataTemplate { VisualTree = row };
    }

    private sealed class FileNameValueConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) =>
            value is string path ? System.IO.Path.GetFileName(path) : string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) =>
            Binding.DoNothing;
    }

    private sealed class CompatibilityValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture) => value switch
        {
            AtariFirmwareCompatibility.Compatible =>
                LocExtension.Get(AtariGeneralSettingsConstants.CompatibleResource),
            AtariFirmwareCompatibility.PartiallyCompatible =>
                LocExtension.Get(AtariGeneralSettingsConstants.PartiallyCompatibleResource),
            _ => LocExtension.Get("Common.Unknown")
        };

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture) => Binding.DoNothing;
    }
}

using GWGUI.Domain.Conversion;
using GWGUI.Domain.Formats;
using GWGUI.App.Contracts.ViewModels.Conversion;
using GWGUI.App.Enums.ViewModels.Conversion;
using GWGUI.App.Localization.Extensions;
using System.Windows;
using System.Windows.Controls;

namespace GWGUI.App.Views.Controls.Conversion;

public partial class ConversionFormatControl : UserControl
{
    private bool _settingState;
    public static readonly DependencyProperty PresentationProperty = DependencyProperty.Register(
        nameof(Presentation), typeof(ConversionFormatPresentation), typeof(ConversionFormatControl),
        new PropertyMetadata(null, PresentationChanged));

    public ConversionFormatPresentation? Presentation
    {
        get => (ConversionFormatPresentation?)GetValue(PresentationProperty);
        set => SetValue(PresentationProperty, value);
    }
    public DiskFormat Format => Presentation?.Format ?? throw new InvalidOperationException("No conversion format is bound.");
    public event EventHandler? ValueChanged;
    public bool IsSelected => FormatCheck.IsChecked == true;
    public IReadOnlySet<string> ExplicitExtensions => ExtensionsPanel.Children.OfType<CheckBox>().Where(x => x.IsChecked == true).Select(x => (string)x.Tag).ToHashSet(StringComparer.OrdinalIgnoreCase);

    public ConversionFormatControl() => InitializeComponent();

    public ConversionFormatControl(DiskFormat format) : this()
    {
        Presentation = new(format, true, false, new HashSet<string>(StringComparer.OrdinalIgnoreCase), format.IsCommon ? ConversionFormatGroup.Common : ConversionFormatGroup.Rare, false);
    }

    private static void PresentationChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        if (sender is ConversionFormatControl control && args.NewValue is ConversionFormatPresentation presentation)
            control.Render(presentation);
    }

    private void Render(ConversionFormatPresentation presentation)
    {
        var format = presentation.Format;
        FormatCheck.Content = presentation.IsReconstructedFlux ? $"{format.DisplayName} — {LocExtension.Get("Conversion.ReconstructedFlux")}" : format.DisplayName;
        FormatCheck.ToolTip = LocExtension.Get("Conversion.DefaultExtensionTip", format.Extensions.First(x => x.IsDefault).Extension.ToUpperInvariant());
        ToolTip = presentation.IsCompatible ? null : LocExtension.Get("Conversion.Incompatible", format.DisplayName);
        ExtensionsPanel.Children.Clear();
        foreach (var extension in format.Extensions)
        {
            var check = new CheckBox { Content = extension.Extension.TrimStart('.').ToUpperInvariant(), Tag = extension.Extension, Margin = new Thickness(12, 0, 0, 0), ToolTip = extension.DisplayName };
            check.Checked += SelectionChanged; check.Unchecked += SelectionChanged; ExtensionsPanel.Children.Add(check);
        }
        SetState(presentation.IsSelected, presentation.ExplicitExtensions);
    }

    public ConversionSelection ToSelection() => new(Format.Id, ExplicitExtensions);
    public void SetState(bool selected, IEnumerable<string>? explicitExtensions)
    {
        _settingState = true;
        try
        {
            FormatCheck.IsChecked = selected;
            var wanted = explicitExtensions?.ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];
            foreach (var check in ExtensionsPanel.Children.OfType<CheckBox>()) check.IsChecked = wanted.Contains((string)check.Tag);
        }
        finally { _settingState = false; }
    }
    private void SelectionChanged(object sender, RoutedEventArgs e)
    {
        if (!_settingState) ValueChanged?.Invoke(this, EventArgs.Empty);
    }
}

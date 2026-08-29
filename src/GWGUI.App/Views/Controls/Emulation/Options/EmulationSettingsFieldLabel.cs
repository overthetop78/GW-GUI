using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Constants.Emulation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace GWGUI.App.Views.Controls.Emulation.Options;

internal sealed class EmulationSettingsFieldLabel : TextBlock
{
    private readonly Border? _helpIcon;
    private readonly FrameworkElement? _associatedControl;
    private readonly FrameworkElement? _postIt;
    private AdornerLayer? _adornerLayer;
    private FieldHelpAdorner? _adorner;
    private bool _inputHandlerAttached;

    internal EmulationSettingsFieldLabel(
        string label,
        string? explanation = null,
        string? detailedExplanation = null,
        FrameworkElement? associatedControl = null)
    {
        Text = label;
        VerticalAlignment = VerticalAlignment.Center;
        TextWrapping = TextWrapping.Wrap;

        if (string.IsNullOrWhiteSpace(explanation) ||
            string.IsNullOrWhiteSpace(detailedExplanation))
            return;

        Text = string.Empty;
        Inlines.Add(new Run(label));
        Inlines.Add(new Run(" "));
        _helpIcon = new Border
        {
            Background = Brushes.Transparent,
            Padding = new Thickness(2),
            Cursor = Cursors.Hand,
            VerticalAlignment = VerticalAlignment.Center,
            Child = new TextBlock
            {
                Text = ControlVisualConstants.InformationGlyph,
                FontFamily = ControlVisualConstants.IconFont,
                VerticalAlignment = VerticalAlignment.Center
            },
            ToolTip = new ToolTip
            {
                Content = new TextBlock
                {
                    Text = explanation,
                    TextWrapping = TextWrapping.NoWrap
                }
            }
        };
        ToolTipService.SetInitialShowDelay(_helpIcon, 0);
        ToolTipService.SetBetweenShowDelay(_helpIcon, 0);
        Inlines.Add(new InlineUIContainer(_helpIcon)
        {
            BaselineAlignment = BaselineAlignment.Center
        });

        var postItText = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            FontFamily = SystemFonts.MessageFontFamily
        };
        postItText.Inlines.Add(new Run(label) { FontWeight = FontWeights.SemiBold });
        postItText.Inlines.Add(new LineBreak());
        postItText.Inlines.Add(new Run(detailedExplanation));
        postItText.SetResourceReference(ForegroundProperty,
            EmulationSettingsFieldHelpConstants.TextBrushResource);

        _postIt = new Border
        {
            Child = new ScrollViewer
            {
                Content = postItText,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            },
            Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1),
                GradientStops =
                {
                    new GradientStop(Color.FromRgb(242, 220, 100), 0),
                    new GradientStop(Color.FromRgb(255, 245, 157), 0.12),
                    new GradientStop(Color.FromRgb(255, 245, 157), 1)
                }
            },
            BorderBrush = new SolidColorBrush(Color.FromRgb(224, 200, 76)),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(EmulationSettingsFieldHelpConstants.ContentPadding),
            Width = EmulationSettingsFieldHelpConstants.MaximumWidth,
            Height = EmulationSettingsFieldHelpConstants.MaximumHeight,
            RenderTransform = new RotateTransform(-0.6),
            RenderTransformOrigin = new Point(0.5, 0.5),
            Effect = new DropShadowEffect
            {
                Color = Color.FromRgb(128, 107, 54),
                BlurRadius = 9,
                ShadowDepth = 3,
                Opacity = 0.22
            }
        };
        _associatedControl = associatedControl ?? _helpIcon;
        _helpIcon.PreviewMouseLeftButtonDown += TogglePostIt;
        Unloaded += OnUnloaded;
    }

    private void TogglePostIt(object sender, MouseButtonEventArgs args)
    {
        if (_adorner is not null)
        {
            ClosePostIt();
            args.Handled = true;
            return;
        }
        if (_postIt is null || _associatedControl is null || _helpIcon is null) return;

        var window = Window.GetWindow(this);
        if (window?.Content is not FrameworkElement root) return;
        var layer = AdornerLayer.GetAdornerLayer(root) ?? AdornerLayer.GetAdornerLayer(_associatedControl);
        if (layer is null) return;

        _adornerLayer = layer;
        _adorner = new FieldHelpAdorner(root, _postIt, _helpIcon, _associatedControl);
        layer.Add(_adorner);
        AttachInputHandler();
        args.Handled = true;
    }

    private void AttachInputHandler()
    {
        if (_inputHandlerAttached) return;
        InputManager.Current.PreProcessInput += CloseOnNextInput;
        _inputHandlerAttached = true;
    }

    private void DetachInputHandler()
    {
        if (!_inputHandlerAttached) return;
        InputManager.Current.PreProcessInput -= CloseOnNextInput;
        _inputHandlerAttached = false;
    }

    private void CloseOnNextInput(object sender, PreProcessInputEventArgs args)
    {
        if (args.StagingItem.Input is MouseButtonEventArgs mouse && mouse.ButtonState == MouseButtonState.Pressed)
        {
            if (_helpIcon?.IsMouseOver == true) return;
            ClosePostIt();
            return;
        }

        if (args.StagingItem.Input is KeyEventArgs key && key.IsDown)
            ClosePostIt();
    }

    private void ClosePostIt()
    {
        if (_adorner is not null)
        {
            var adorner = _adorner;
            _adornerLayer?.Remove(adorner);
            adorner.Detach();
        }
        _adorner = null;
        _adornerLayer = null;
        DetachInputHandler();
    }

    private void OnUnloaded(object sender, RoutedEventArgs args)
    {
        ClosePostIt();
        if (_helpIcon is not null)
            _helpIcon.PreviewMouseLeftButtonDown -= TogglePostIt;
        Unloaded -= OnUnloaded;
    }

    private sealed class FieldHelpAdorner : Adorner
    {
        private readonly VisualCollection _visuals;
        private readonly FrameworkElement _content;
        private readonly FrameworkElement _horizontalTarget;
        private readonly FrameworkElement _verticalTarget;

        internal FieldHelpAdorner(
            FrameworkElement adornedElement,
            FrameworkElement content,
            FrameworkElement horizontalTarget,
            FrameworkElement verticalTarget)
            : base(adornedElement)
        {
            _content = content;
            _horizontalTarget = horizontalTarget;
            _verticalTarget = verticalTarget;
            _visuals = new VisualCollection(this) { content };
            ClipToBounds = true;
        }

        protected override int VisualChildrenCount => _visuals.Count;

        protected override Visual GetVisualChild(int index) => _visuals[index];

        internal void Detach() => _visuals.Remove(_content);

        protected override Size MeasureOverride(Size constraint)
        {
            _content.Measure(constraint);
            return constraint;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var desired = _content.DesiredSize;
            var icon = _horizontalTarget.TranslatePoint(new Point(), AdornedElement);
            var fieldTop = _verticalTarget.TranslatePoint(new Point(), AdornedElement);
            var fieldBottom = _verticalTarget.TranslatePoint(
                new Point(0, _verticalTarget.ActualHeight), AdornedElement);
            var spacing = EmulationSettingsFieldHelpConstants.IconPopupSpacing;
            var x = Math.Clamp(icon.X, 0, Math.Max(0, finalSize.Width - desired.Width));
            var below = fieldBottom.Y + spacing;
            var above = fieldTop.Y - desired.Height - spacing;
            var y = below + desired.Height <= finalSize.Height ? below : above;
            y = Math.Clamp(y, 0, Math.Max(0, finalSize.Height - desired.Height));
            _content.Arrange(new Rect(new Point(x, y), desired));
            return finalSize;
        }
    }
}

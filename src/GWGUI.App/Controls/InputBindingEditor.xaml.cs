using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using GWGUI.App.Localization;
using GWGUI.App.Services;
using GWGUI.App.ViewModels;
using GWGUI.Emulation;

namespace GWGUI.App.Controls;

public partial class InputBindingEditor : UserControl
{
    private readonly ObservableCollection<InputBindingRow> _rows = [];
    private readonly HashSet<Key> _capturePressed = [];
    private readonly List<Key> _captureOrder = [];
    private InputBindingRow? _captureRow;
    private Button? _captureButton;
    private object? _captureButtonContent;
    private double _captureButtonHeight;
    private ModifierKeys _captureModifiers;
    private InputCaptureSources _captureSources = InputCaptureSources.Keyboard;
    private bool _prefixKeyboardSource;
    private readonly DispatcherTimer _controllerCaptureTimer;
    private IReadOnlyList<EmulationControllerState> _controllerBaseline = [];
    private IReadOnlySet<string> _reservedBindings = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private HwndSource? _windowSource;

    public InputBindingEditor()
    {
        InitializeComponent();
        BindingsList.ItemsSource = _rows;
        SearchBox.ToolTip = LocExtension.Get("Emulation.Input.Binding.Search");
        AddHandler(PreviewKeyDownEvent, new KeyEventHandler(CaptureKeyDown), true);
        AddHandler(PreviewKeyUpEvent, new KeyEventHandler(CaptureKeyUp), true);
        AddHandler(PreviewMouseDownEvent, new MouseButtonEventHandler(CaptureMouseDown), true);
        AddHandler(PreviewMouseWheelEvent, new MouseWheelEventHandler(CaptureMouseWheel), true);
        _controllerCaptureTimer = new DispatcherTimer(ControlTechnicalConstants.ControllerCapturePollingInterval, DispatcherPriority.Input,
            CaptureControllerInput, Dispatcher);
        _controllerCaptureTimer.Stop();
        Loaded += (_, _) => AttachWindowHook();
        Unloaded += (_, _) => DetachWindowHook();
    }

    public bool HasErrors => _rows.Any(row => row.State is InputBindingState.Conflict or InputBindingState.Reserved);
    public IReadOnlyList<InputBindingRow> Rows => _rows;
    public event EventHandler? BindingsChanged;
    public event EventHandler<ControllerCapturedEventArgs>? ControllerCaptured;

    public void ConfigurePresentation(string firstColumnHeader, string searchPlaceholder)
    {
        TargetHeader.Text = firstColumnHeader;
        SearchPlaceholder.Text = searchPlaceholder;
    }

    public void ConfigureCaptureSources(InputCaptureSources sources, bool prefixKeyboardSource = false)
    {
        _captureSources = sources;
        _prefixKeyboardSource = prefixKeyboardSource;
        ValidateBindings();
    }

    public void SetRows(IEnumerable<InputBindingDefinition> definitions, IReadOnlyDictionary<string, string>? values)
    {
        _rows.Clear();
        foreach (var definition in definitions)
            _rows.Add(new InputBindingRow(definition.Id,
                definition.InvariantDisplayValue ?? LocExtension.Get(definition.DisplayResourceKey),
                values?.GetValueOrDefault(definition.Id) ?? definition.DefaultBinding, definition.DefaultBinding));
        ValidateBindings();
    }

    public void SetReservedBindings(IEnumerable<string> bindings)
    {
        _reservedBindings = bindings.Where(value => !string.IsNullOrWhiteSpace(value))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        ValidateBindings();
    }
}

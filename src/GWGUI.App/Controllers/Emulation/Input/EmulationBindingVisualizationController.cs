using GWGUI.App.Constants.Input.Bindings;
using GWGUI.App.Contracts.Input;
using GWGUI.App.Functions.Input.Bindings;
using GWGUI.App.Functions.Input.Keyboard;
using GWGUI.App.Services.Input.GameInput;
using GWGUI.App.Views.Controls.Emulation.Input;
using GWGUI.App.Views.Controls.Options;
using GWGUI.Emulation.Functions;
using System.Windows.Input;
using System.Windows.Threading;

namespace GWGUI.App.Controllers.Emulation.Input;

internal sealed class EmulationBindingVisualizationController
{
    private const string MouseLeft = "Left";
    private const string MouseRight = "Right";
    private const string MouseMiddle = "Middle";
    private const string MouseXButton1 = "XButton1";
    private const string MouseXButton2 = "XButton2";
    private static readonly TimeSpan PollingInterval = TimeSpan.FromMilliseconds(33);

    private readonly InputBindingEditor _bindings;
    private readonly ControllerVisualizer _visualizer;
    private readonly DispatcherTimer _timer;

    internal EmulationBindingVisualizationController(
        InputBindingEditor bindings,
        ControllerVisualizer visualizer)
    {
        _bindings = bindings;
        _visualizer = visualizer;
        _timer = new DispatcherTimer(
            PollingInterval, DispatcherPriority.Input, Update, bindings.Dispatcher);
        _timer.Stop();
    }

    internal void Start()
    {
        Update(this, EventArgs.Empty);
        _timer.Start();
    }

    internal void Stop()
    {
        _timer.Stop();
        _visualizer.VisualState = new ControllerVisualState();
    }

    private void Update(object? sender, EventArgs args)
    {
        var controllers = GameInputControllerReader.ReadAll();
        var values = _bindings.Rows.ToDictionary(
            row => row.Id,
            row => BindingValue(row.Binding, controllers),
            StringComparer.Ordinal);
        _visualizer.VisualState = new ControllerVisualState
        {
            EmulatedCommandValues = values
        };
    }

    private static float BindingValue(
        string binding,
        IReadOnlyList<EmulationControllerState> controllers)
    {
        if (InputBindingSyntax.TryRemovePrefix(
                binding, InputBindingSyntaxConstants.ControllerPrefix, out _))
        {
            var deviceId = EmulationInputMappingFunctions.ParseControllerDeviceId(binding);
            var controller = EmulationInputMappingFunctions.ResolveController(
                deviceId, controllers, fallbackIndex: -1);
            return EmulationInputMappingFunctions.ControllerSourceValue(binding, controller);
        }

        if (InputBindingSyntax.TryRemovePrefix(
                binding, InputBindingSyntaxConstants.MousePrefix, out var mouseSource))
            return MouseValue(mouseSource);

        if (InputBindingSyntax.TryRemovePrefix(
                binding, InputBindingSyntaxConstants.KeyboardPrefix, out var keyboardSource))
            return KeyboardValue(keyboardSource);

        return KeyboardValue(binding);
    }

    private static float KeyboardValue(string binding)
    {
        if (!KeyboardChordFunctions.TryParse(binding, out var chord)) return 0f;
        if ((Keyboard.Modifiers & chord.Modifiers) != chord.Modifiers) return 0f;
        return chord.Keys.All(Keyboard.IsKeyDown) ? 1f : 0f;
    }

    private static float MouseValue(string source) => source switch
    {
        MouseLeft => Mouse.LeftButton == MouseButtonState.Pressed ? 1f : 0f,
        MouseRight => Mouse.RightButton == MouseButtonState.Pressed ? 1f : 0f,
        MouseMiddle => Mouse.MiddleButton == MouseButtonState.Pressed ? 1f : 0f,
        MouseXButton1 => Mouse.XButton1 == MouseButtonState.Pressed ? 1f : 0f,
        MouseXButton2 => Mouse.XButton2 == MouseButtonState.Pressed ? 1f : 0f,
        _ => 0f
    };
}

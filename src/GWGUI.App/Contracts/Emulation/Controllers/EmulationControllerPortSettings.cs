using GWGUI.App.Views.Controls.Emulation.Input;
using GWGUI.App.Views.Controls.Options;
using System.Windows.Controls;



namespace GWGUI.App.Contracts.Emulation.Controllers;

internal sealed record EmulationControllerPortSettings(
    int Number, ComboBox Type, ComboBox Visual,
    ControllerVisualizer Visualizer, InputBindingEditor Bindings);

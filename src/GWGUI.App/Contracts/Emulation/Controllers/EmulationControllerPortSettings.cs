using GWGUI.App.Views.Controls.Emulation.Input;
using System.Windows.Controls;



namespace GWGUI.App.Contracts.Emulation.Controllers;

internal sealed record EmulationControllerPortSettings(
    int Number, ComboBox Type, ComboBox Device, InputBindingEditor Bindings);

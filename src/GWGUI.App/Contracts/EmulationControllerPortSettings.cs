using System.Windows.Controls;

namespace GWGUI.App.Controls;

internal sealed record EmulationControllerPortSettings(
    int Number, ComboBox Type, ComboBox Device, InputBindingEditor Bindings);

using GWGUI.App.Contracts.Emulation.Controllers;
using GWGUI.App.Contracts.Services.Input;
using GWGUI.App.Enums.Input;
using System.Windows.Controls;


namespace GWGUI.App.Views.Controls.Emulation.Input;

internal sealed class EmulationControllerPortEditor
{
    internal EmulationControllerPortEditor(int number, InputCaptureSources captureSources,
        bool prefixKeyboardSource, string actionLabel, string searchLabel)
    {
        Number = number;
        Type = new ComboBox();
        Device = new ComboBox { DisplayMemberPath = nameof(GameControllerDevice.Name) };
        Bindings = new InputBindingEditor();
        Bindings.ConfigurePresentation(actionLabel, searchLabel);
        Bindings.ConfigureCaptureSources(captureSources, prefixKeyboardSource);
        Bindings.ControllerCaptured += (_, args) =>
        {
            var device = Device.Items.Cast<GameControllerDevice>()
                .FirstOrDefault(item => item.Id == $"xinput:{args.Port}")
                ?? Device.Items.Cast<GameControllerDevice>()
                    .FirstOrDefault(item => item.Id == $"gamepad:{args.Port}");
            if (device is not null) Device.SelectedItem = device;
        };
    }

    internal int Number { get; }
    internal ComboBox Type { get; }
    internal ComboBox Device { get; }
    internal InputBindingEditor Bindings { get; }
    internal int DeadZonePercent { get; set; }
    internal EmulationControllerPortSettings Settings => new(Number, Type, Device, Bindings);
}

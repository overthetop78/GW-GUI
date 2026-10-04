using GWGUI.Emulation.Enums;

namespace GWGUI.App.Views.Controls.Options.ControllerVisualization;

internal static partial class ControllerArtworkCatalog
{
    private static ProfileDefinition CreateNecCoreGrafxIiTurboPad() => new(ControllerArtworkFileNames.NecCoregrafxIiTurbopad, NecCoreGrafxTurboPadZones);
}

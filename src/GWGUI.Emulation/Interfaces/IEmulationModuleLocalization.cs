using System.Globalization;

namespace GWGUI.Emulation.Interfaces;

public interface IEmulationModuleLocalization
{
    bool TryGetString(string key, CultureInfo culture, out string value);
}

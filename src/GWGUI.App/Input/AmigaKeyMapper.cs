using System.Windows.Input;
using GWGUI.Emulation;

namespace GWGUI.App.Input;

public static class AmigaKeyMapper
{
    public static bool TryMap(Key key, out EmulationKey result) => EmulationKeyMapper.TryMap(key, out result);
}

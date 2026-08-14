using System.Windows.Input;
using GWGUI.App.Controls;
using GWGUI.Emulation;

namespace GWGUI.Tests;

public sealed class AmigaKeyboardMappingTests
{
    [Theory]
    [InlineData(Key.A, EmulationKey.A)]
    [InlineData(Key.D9, EmulationKey.D9)]
    [InlineData(Key.F10, EmulationKey.F10)]
    [InlineData(Key.CapsLock, EmulationKey.CapsLock)]
    [InlineData(Key.LWin, EmulationKey.LeftAmiga)]
    [InlineData(Key.Oem3, EmulationKey.Backquote)]
    [InlineData(Key.NumPad7, EmulationKey.Numpad7)]
    [InlineData(Key.Decimal, EmulationKey.NumpadPeriod)]
    [InlineData(Key.Divide, EmulationKey.NumpadDivide)]
    [InlineData(Key.Multiply, EmulationKey.NumpadMultiply)]
    [InlineData(Key.Subtract, EmulationKey.NumpadMinus)]
    [InlineData(Key.Add, EmulationKey.NumpadPlus)]
    public void TryMapKey_CoversTheAmigaKeyboard(Key input, EmulationKey expected)
    {
        Assert.True(AmigaMachineView.TryMapKey(input, out var mapped));
        Assert.Equal(expected, mapped);
    }
}

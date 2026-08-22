using GWGUI.App.Functions.Input.Keyboard;
using GWGUI.App.Functions.Rendering.Emulation;
using GWGUI.Emulation;
using System.Windows.Input;

namespace GWGUI.Tests;

public sealed class AmigaKeyboardMappingTests
{
    [Theory]
    [InlineData(1600, 900, 1200, 900)]
    [InlineData(800, 900, 800, 600)]
    [InlineData(1024, 768, 1024, 768)]
    public void EmulatorScreen_IsAlwaysFittedToFourThree(double availableWidth, double availableHeight,
        double expectedWidth, double expectedHeight)
    {
        var size = EmulationVideoLayoutFunctions.FitFourThree(availableWidth, availableHeight);
        Assert.Equal(expectedWidth, size.Width);
        Assert.Equal(expectedHeight, size.Height);
        Assert.Equal(4d / 3d, size.Width / size.Height, 10);
    }

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
        Assert.True(EmulationKeyMapper.TryMap(input, out var mapped));
        Assert.Equal(expected, mapped);
    }
}

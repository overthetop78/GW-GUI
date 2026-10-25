using GWGUI.Emulation;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Contracts;

namespace GWGUI.Tests.Emulation;

public sealed class CoreButtonIndexContractTests
{
    [Theory]
    [InlineData(EmulationControllerCommandIds.B, 0)]
    [InlineData(EmulationControllerCommandIds.Y, 1)]
    [InlineData(EmulationControllerCommandIds.Select, 2)]
    [InlineData(EmulationControllerCommandIds.Start, 3)]
    [InlineData(EmulationControllerCommandIds.Up, 4)]
    [InlineData(EmulationControllerCommandIds.Down, 5)]
    [InlineData(EmulationControllerCommandIds.Left, 6)]
    [InlineData(EmulationControllerCommandIds.Right, 7)]
    [InlineData(EmulationControllerCommandIds.A, 8)]
    [InlineData(EmulationControllerCommandIds.X, 9)]
    [InlineData(EmulationControllerCommandIds.L, 10)]
    [InlineData(EmulationControllerCommandIds.R, 11)]
    [InlineData(EmulationControllerCommandIds.L2, 12)]
    [InlineData(EmulationControllerCommandIds.R2, 13)]
    [InlineData(EmulationControllerCommandIds.L3, 14)]
    [InlineData(EmulationControllerCommandIds.R3, 15)]
    public void StandardIndicesMatchTheNativeBitMask(string command, int expected)
    {
        var processing = EmulationSdk.InputProcessing;
        var index = processing.ControllerButtonIndex(command);
        Assert.Equal(expected, index);
        Assert.Equal(expected, processing.ControllerButtonIndex(command.ToLowerInvariant()));
        var snapshot = EmulationInputSnapshot.Empty with { Controllers = [EmulationControllerState.Empty with { Buttons = 1u << index }] };
        Assert.Equal((short)1, processing.ReadNativeInput(snapshot, 0, 1, 0, (uint)expected));
        Assert.Equal((short)0, processing.ReadNativeInput(snapshot, 0, 1, 0, (uint)((expected + 1) % 16)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("unknown-command")]
    [InlineData(" A ")]
    public void UnknownCommandsAreNotNormalizedIntoAnExistingButton(string command) => Assert.Equal(-1, EmulationSdk.InputProcessing.ControllerButtonIndex(command));
}

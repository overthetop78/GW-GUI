using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Input;
using GWGUI.App.Input;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;
using GWGUI.Emulation.Atari.Cores;
using GWGUI.Emulation.Common;

namespace GWGUI.Tests;

public sealed class AtariKeyboardMappingTests
{
    [Fact]
    public void Mapping_CoversEveryStandardEmulationKey()
    {
        var excluded = new HashSet<EmulationKey>
        {
            EmulationKey.Unknown, EmulationKey.AtariOption, EmulationKey.AtariSelect, EmulationKey.AtariStart
        };
        Assert.All(Enum.GetValues<EmulationKey>().Except(excluded),
            key => Assert.True(AtariKeyboardState.Mappings.ContainsKey(key), key.ToString()));
        Assert.DoesNotContain(EmulationKey.Unknown, AtariKeyboardState.Mappings.Keys);
    }

    [Fact]
    public void State_PublishesEveryMappedKeyAndIgnoresUnknownKeys()
    {
        foreach (var mapping in AtariKeyboardState.Mappings)
        {
            var events = new List<(bool Down, uint Code)>();
            ExternalCoreApi.KeyboardEvent callback = (down, code, _, _) => events.Add((down, code));
            var state = new AtariKeyboardState();
            state.Publish(Keys(mapping.Key), callback);
            state.Publish(Keys(), callback);
            Assert.Equal([(true, mapping.Value), (false, mapping.Value)], events);
        }

        var ignored = new List<uint>();
        ExternalCoreApi.KeyboardEvent ignoredCallback = (_, code, _, _) => ignored.Add(code);
        new AtariKeyboardState().Publish(Keys(EmulationKey.Unknown, EmulationKey.AtariOption,
            EmulationKey.AtariSelect, EmulationKey.AtariStart), ignoredCallback);
        Assert.Empty(ignored);
    }

    [Theory]
    [InlineData(Key.A, EmulationKey.A)]
    [InlineData(Key.F10, EmulationKey.F10)]
    [InlineData(Key.OemQuestion, EmulationKey.Slash)]
    [InlineData(Key.NumPad8, EmulationKey.Numpad8)]
    public void ApplicationAdapter_RemainsOutsideTheEngine(Key source, EmulationKey expected)
    {
        Assert.True(EmulationKeyMapper.TryMap(source, out var actual));
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Character_UsesShiftAndCapsLockWithoutChangingPhysicalMapping()
    {
        Assert.Equal((uint)'a', AtariKeyboardFunctions.Character((uint)'a', Keys()));
        Assert.Equal((uint)'A', AtariKeyboardFunctions.Character((uint)'a', Keys(EmulationKey.LeftShift)));
        Assert.Equal((uint)'A', AtariKeyboardFunctions.Character((uint)'a', Keys(EmulationKey.CapsLock)));
        Assert.Equal((uint)'a', AtariKeyboardFunctions.Character((uint)'a',
            Keys(EmulationKey.LeftShift, EmulationKey.CapsLock)));
        Assert.Equal((uint)'!', AtariKeyboardFunctions.Character((uint)'1', Keys(EmulationKey.RightShift)));
        Assert.Equal(AtariKeyboardConstants.NoCharacter,
            AtariKeyboardFunctions.Character(AtariKeyboardConstants.Help, Keys()));
    }

    [Fact]
    public void ConsoleKeys_UseTheAtari800Controls()
    {
        var snapshot = Snapshot(EmulationKey.AtariOption, EmulationKey.AtariSelect,
            EmulationKey.AtariStart, EmulationKey.Help);
        Assert.Equal(AtariInputConstants.ActiveState, State(snapshot, AtariInputConstants.JoypadLeftShoulderId));
        Assert.Equal(AtariInputConstants.ActiveState, State(snapshot, AtariInputConstants.JoypadSelectId));
        Assert.Equal(AtariInputConstants.ActiveState, State(snapshot, AtariInputConstants.JoypadStartId));
        Assert.Equal(AtariInputConstants.ActiveState, State(snapshot, AtariInputConstants.JoypadRightTriggerId));
    }

    [Fact]
    public void Callback_PublishesModifiersUnicodeDownUpAndFocusLossRelease()
    {
        var events = new List<(bool Down, uint Code, uint Character, ushort Modifiers)>();
        ExternalCoreApi.KeyboardEvent callback = (down, code, character, modifiers) =>
            events.Add((down, code, character, modifiers));
        using var callbacks = CreateCallbacks();
        var native = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.KeyboardCallback>());
        try
        {
            Marshal.StructureToPtr(new ExternalCoreApi.KeyboardCallback
            {
                Callback = Marshal.GetFunctionPointerForDelegate(callback)
            }, native, false);
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.SetKeyboardCallback, native));
            callbacks.Input = Snapshot(EmulationKey.LeftShift, EmulationKey.A);
            callbacks.InputPoll();
            Assert.Collection(events,
                item => Assert.Equal((true, AtariKeyboardConstants.LeftShift,
                    AtariKeyboardConstants.NoCharacter, AtariKeyboardConstants.ShiftModifier), item),
                item => Assert.Equal((true, (uint)'a', (uint)'A', AtariKeyboardConstants.ShiftModifier), item));

            events.Clear();
            callbacks.Input = EmulationInputSnapshot.Empty;
            callbacks.InputPoll();
            Assert.Collection(events,
                item => Assert.Equal((false, (uint)'a', AtariKeyboardConstants.NoCharacter, (ushort)0), item),
                item => Assert.Equal((false, AtariKeyboardConstants.LeftShift,
                    AtariKeyboardConstants.NoCharacter, (ushort)0), item));
            GC.KeepAlive(callback);
        }
        finally
        {
            Marshal.FreeHGlobal(native);
        }
    }

    private static short State(EmulationInputSnapshot snapshot, uint id) => AtariInputFunctions.State(snapshot,
        AtariInputConstants.LeftAnalogIndex, AtariInputConstants.JoypadDevice,
        AtariInputConstants.LeftAnalogIndex, id);

    private static HashSet<EmulationKey> Keys(params EmulationKey[] keys) => [.. keys];

    private static EmulationInputSnapshot Snapshot(params EmulationKey[] keys) => new(Keys(keys),
        EmulationInputSnapshot.Empty.Pointer, EmulationInputSnapshot.Empty.Controllers);

    private static AtariExternalHostCallbacks CreateCallbacks()
    {
        var root = Path.Combine(Path.GetTempPath(), $"gwgui-atari-keyboard-{Guid.NewGuid():N}");
        return new AtariExternalHostCallbacks(Path.Combine(root, "system"), Path.Combine(root, "content"),
            Path.Combine(root, "save"), Path.Combine(root, "assets"), new Dictionary<string, string>());
    }
}

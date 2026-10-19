using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Interop;

namespace GWGUI.Tests.Emulation;

public sealed class KeyboardTransitionTests
{
    private static readonly IReadOnlyDictionary<EmulationKey, uint> Map = new Dictionary<EmulationKey, uint>
    {
        [EmulationKey.A] = 'a', [EmulationKey.B] = 'b', [EmulationKey.C] = 'c',
        [EmulationKey.LeftShift] = 304, [EmulationKey.LeftControl] = 306, [EmulationKey.LeftAmiga] = 311
    };

    [Fact]
    public void ReleasesPrecedePressesAndModifiersKeepTheirOriginalPriority()
    {
        var events = new List<(bool Down, uint Code, uint Character, ushort Modifiers)>();
        var state = new State { Previous = new HashSet<EmulationKey> { EmulationKey.LeftShift, EmulationKey.A } };
        state.Event = (down, code, character, modifiers) => events.Add((down, code, character, modifiers));
        Publish(state, new HashSet<EmulationKey> { EmulationKey.B, EmulationKey.LeftControl });
        Assert.Equal(new[]
        {
            (false, (uint)'a', 0u, (ushort)2), (false, 304u, 0u, (ushort)2),
            (true, 306u, 0u, (ushort)2), (true, (uint)'b', (uint)'b', (ushort)2)
        }, events);
    }

    [Fact]
    public void UnknownKeysAreTrackedInAnIndependentSnapshotWithoutEvents()
    {
        var events = 0;
        var state = new State { Event = (_, _, _, _) => events++ };
        var input = new HashSet<EmulationKey> { EmulationKey.F24 };
        Publish(state, input);
        Assert.Equal(0, events);
        Assert.NotSame(input, state.Previous);
        input.Clear();
        Assert.Contains(EmulationKey.F24, state.Previous);
    }

    [Fact]
    public void CallbackFailureDoesNotCommitTheNextSnapshot()
    {
        var error = new InvalidOperationException("callback failure");
        var state = new State
        {
            Previous = new HashSet<EmulationKey> { EmulationKey.A },
            Event = (_, _, _, _) => throw error
        };
        var previous = state.Previous;
        Assert.Same(error, Assert.Throws<InvalidOperationException>(() => Publish(state, new HashSet<EmulationKey>())));
        Assert.Same(previous, state.Previous);
    }

    [Fact]
    public void ReentrantStateAndCallbackChangesAreVisibleToTheRemainingTransitions()
    {
        var events = 0;
        var state = new State { Previous = new HashSet<EmulationKey> { EmulationKey.A } };
        state.Event = (_, _, _, _) =>
        {
            events++;
            state.Previous = new HashSet<EmulationKey> { EmulationKey.B };
        };
        Publish(state, new HashSet<EmulationKey> { EmulationKey.B });
        Assert.Equal(1, events);

        var receivers = new List<string>();
        state.Previous = new HashSet<EmulationKey> { EmulationKey.A, EmulationKey.C };
        state.Event = (_, _, _, _) =>
        {
            receivers.Add("first");
            state.Event = (_, _, _, _) => receivers.Add("replacement");
        };
        Publish(state, new HashSet<EmulationKey>());
        Assert.Equal(new[] { "first", "replacement" }, receivers);
    }

    [Fact]
    public void CharacterAndModifierPoliciesRemainSuppliedByTheMachine()
    {
        var events = new List<(uint Code, uint Character)>();
        var state = new State { Event = (_, code, character, _) => events.Add((code, character)) };
        var keys = new HashSet<EmulationKey> { EmulationKey.A, EmulationKey.LeftAmiga };
        ExternalCoreKeyboardFunctions.PublishTransitions(keys, Map, 0, ref state.Previous, ref state.Event,
            static key => key == EmulationKey.LeftAmiga, static (code, _) => code + 1000);
        Assert.Equal(new[] { (311u, 1311u), ((uint)'a', (uint)'a' + 1000) }, events);
    }

    [Fact]
    public void RightHandModifiersUseTheSameBitsAndCapsLockAddsNoBit()
    {
        var keys = new HashSet<EmulationKey>
        {
            EmulationKey.RightShift, EmulationKey.RightControl, EmulationKey.RightAlt, EmulationKey.CapsLock
        };
        Assert.Equal((ushort)7, ExternalCoreKeyboardFunctions.Modifiers(keys));
    }

    private static void Publish(State state, IReadOnlySet<EmulationKey> keys) =>
        ExternalCoreKeyboardFunctions.PublishTransitions(keys, Map, ExternalCoreKeyboardFunctions.Modifiers(keys),
            ref state.Previous, ref state.Event, IsModifier, ExternalCoreKeyboardFunctions.Character);

    private static bool IsModifier(EmulationKey key) => key is EmulationKey.LeftShift or EmulationKey.LeftControl;

    private sealed class State
    {
        internal IReadOnlySet<EmulationKey> Previous = new HashSet<EmulationKey>();
        internal ExternalCoreApi.KeyboardEvent? Event;
    }
}

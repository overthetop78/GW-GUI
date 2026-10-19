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

    [Fact]
    public void NativeCodePoliciesOrderEachPhaseWithoutCommittingOwnerState()
    {
        var previous = new HashSet<uint> { 8, 1 };
        var original = previous;
        var current = new HashSet<uint> { 2, 7 };
        var events = new List<(bool Down, uint Code, uint Character, ushort Modifiers)>();
        ExternalCoreKeyboardFunctions.PublishCodeTransitions(current, ref previous,
            (down, code, character, modifiers) => events.Add((down, code, character, modifiers)), 3,
            codes => codes.OrderBy(code => code == 8),
            codes => codes.OrderByDescending(code => code == 7), code => code + 100);
        Assert.Equal(new[]
        {
            (false, 1u, 0u, (ushort)3), (false, 8u, 0u, (ushort)3),
            (true, 7u, 107u, (ushort)3), (true, 2u, 102u, (ushort)3)
        }, events);
        Assert.Same(original, previous);
    }

    [Fact]
    public void AliasedNativeCodesProduceNoDuplicateTransitions()
    {
        var previous = new HashSet<uint> { 13 };
        var current = new HashSet<uint>(new uint[] { 13, 13 });
        var events = 0;
        ExternalCoreKeyboardFunctions.PublishCodeTransitions(current, ref previous,
            (_, _, _, _) => events++, 0, codes => codes, codes => codes, _ => 0);
        Assert.Equal(0, events);
    }

    [Fact]
    public void NativeCodePressesReadTheReentrantPreviousSet()
    {
        IReadOnlySet<uint> previous = new HashSet<uint> { 1 };
        IReadOnlySet<uint> current = new HashSet<uint> { 2 };
        var events = 0;
        ExternalCoreKeyboardFunctions.PublishCodeTransitions(current, ref previous,
            (_, _, _, _) => { events++; previous = new HashSet<uint> { 2 }; },
            0, codes => codes, codes => codes, _ => 0);
        Assert.Equal(1, events);
    }

    [Fact]
    public void NativeCodeCallbackFailureLeavesTheCommitToTheOwner()
    {
        var previous = new HashSet<uint> { 1 };
        var original = previous;
        var error = new InvalidOperationException("native callback failure");
        var released = new List<uint>();
        Assert.Same(error, Assert.Throws<InvalidOperationException>(() =>
            ExternalCoreKeyboardFunctions.PublishCodeTransitions(new HashSet<uint> { 2 }, ref previous,
                (down, code, _, _) => { if (down) throw error; released.Add(code); },
                0, codes => codes, codes => codes, _ => 0)));
        Assert.Equal(new uint[] { 1 }, released);
        Assert.Same(original, previous);
    }

    [Fact]
    public void NativeCodeCharactersReadTheLiveMachinePolicy()
    {
        var previous = new HashSet<uint>();
        var composed = false;
        var characters = new List<(uint Code, uint Character)>();
        ExternalCoreKeyboardFunctions.PublishCodeTransitions(new HashSet<uint> { 4, 3 }, ref previous,
            (_, code, character, _) => { characters.Add((code, character)); composed = true; },
            0, codes => codes, codes => codes.Order(), code => composed ? 0 : code);
        Assert.Equal(new[] { (3u, 3u), (4u, 0u) }, characters);
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

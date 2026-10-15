using System.IO;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;

internal static class ControllerFeedbackProtocol
{
    private const string InvalidCount = "Invalid controller feedback count.";
    private const string InvalidState = "Invalid controller feedback state.";
    private const int MaximumEffects = ControllerPortConstants.MaximumControllerPortCount * 2;

    internal static void Write(BinaryWriter writer, IReadOnlyList<ControllerFeedbackState> states)
    {
        Validate(states);
        writer.Write(states.Count);
        foreach (var state in states)
        {
            writer.Write(state.Port);
            writer.Write(state.CommandId == EmulationControllerFeedbackCommands.Strong);
            writer.Write(state.Strength);
        }
    }

    internal static IReadOnlyList<ControllerFeedbackState> Read(BinaryReader reader)
    {
        var count = reader.ReadInt32();
        if (count is < 0 or > MaximumEffects) throw new InvalidDataException(InvalidCount);
        var states = new ControllerFeedbackState[count];
        for (var index = 0; index < count; index++)
            states[index] = new(reader.ReadInt32(), reader.ReadBoolean() ? EmulationControllerFeedbackCommands.Strong : EmulationControllerFeedbackCommands.Weak, reader.ReadSingle());
        Validate(states);
        return states;
    }

    private static void Validate(IReadOnlyList<ControllerFeedbackState> states)
    {
        if (states.Count > MaximumEffects) throw new InvalidDataException(InvalidCount);
        var seen = new HashSet<(int, string)>();
        foreach (var state in states)
        {
            if (state.Port is < 0 or >= ControllerPortConstants.MaximumControllerPortCount
                || state.CommandId is not (EmulationControllerFeedbackCommands.Strong or EmulationControllerFeedbackCommands.Weak)
                || !float.IsFinite(state.Strength) || state.Strength is < 0 or > 1
                || !seen.Add((state.Port, state.CommandId)))
                throw new InvalidDataException(InvalidState);
        }
    }
}

namespace GWGUI.Emulation.Atari.Common.Machines.AtariST.Functions;

internal static class StModelFunctions
{
    internal static IReadOnlyList<T> Values<T>(params T[] values) => Array.AsReadOnly(values);

    internal static IReadOnlyList<T> EnumValues<T>() where T : struct, Enum =>
        Array.AsReadOnly(Enum.GetValues<T>());

    internal static IReadOnlyList<int> InclusiveRange(int minimum, int maximum, int step)
    {
        var values = new List<int>();
        for (var value = minimum; value <= maximum; value += step) values.Add(value);
        return values.AsReadOnly();
    }

    internal static IReadOnlyDictionary<MachineModel, StModelDefinition> Index(
        IReadOnlyList<StModelDefinition> definitions)
    {
        var result = definitions.ToDictionary(definition => definition.Model);
        if (result.Count != definitions.Count)
            throw new InvalidOperationException(ErrorMessages.DuplicateStModelDefinition);
        return result;
    }
}

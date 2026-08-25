namespace GWGUI.Emulation.Atari.Functions;

internal static class AtariStModelFunctions
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

    internal static IReadOnlyDictionary<AtariMachineModel, AtariStModelDefinition> Index(
        IReadOnlyList<AtariStModelDefinition> definitions)
    {
        var result = definitions.ToDictionary(definition => definition.Model);
        if (result.Count != definitions.Count)
            throw new InvalidOperationException(AtariErrorMessages.DuplicateStModelDefinition);
        return result;
    }
}

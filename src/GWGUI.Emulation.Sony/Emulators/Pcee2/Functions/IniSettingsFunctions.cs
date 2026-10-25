namespace GWGUI.Emulation.Sony.Emulators.Pcee2.Functions;

internal static class IniSettingsFunctions
{
    internal static void Set(List<string> lines, string section, string key, string value)
    {
        var start = lines.FindIndex(line => line.Trim().Equals("[" + section + "]", StringComparison.OrdinalIgnoreCase));
        if (start < 0)
        {
            lines.Add("[" + section + "]");
            start = lines.Count - 1;
        }
        var end = lines.FindIndex(start + 1, line => line.TrimStart().StartsWith('['));
        if (end < 0) end = lines.Count;
        for (var index = start + 1; index < end; index++)
        {
            var separator = lines[index].IndexOf('=');
            if (separator < 0 || !lines[index][..separator].Trim().Equals(key, StringComparison.OrdinalIgnoreCase)) continue;
            lines[index] = key + " = " + value;
            return;
        }
        lines.Insert(end, key + " = " + value);
    }
}

using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace GWGUI.Emulation.Sony.Emulators.Play.Functions;

internal static class MemoryCardFunctions
{
    internal static string DirectoryFor(int index)
    {
        var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Play Data Files");
        var fallback = Path.Combine(root, "vfs", "mc" + index);
        var configuration = Path.Combine(root, "config.xml");
        if (!File.Exists(configuration)) return fallback;
        try
        {
            var document = XDocument.Load(configuration);
            var selected = document.Descendants("Preference").FirstOrDefault(entry =>
                (string?)entry.Attribute("Name") == "ps2.mc" + index + ".directory.v2")
                ?.Attribute("Value")?.Value;
            return !string.IsNullOrWhiteSpace(selected) && Directory.Exists(selected) ? selected : fallback;
        }
        catch (XmlException) { return fallback; }
        catch (IOException) { return fallback; }
        catch (UnauthorizedAccessException) { return fallback; }
    }
}

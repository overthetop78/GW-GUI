using System.Text;

namespace GWGUI.Emulation.Amiga;

internal static class AmigaConfigurationValidationFunctions
{
    private const string EncryptedKickstartHeader = "AMIROMTYPE1";

    internal static void ValidateForSave(AmigaMachineConfiguration configuration)
    {
        ValidateFile(configuration.KickstartPath, true);
        ValidateFile(configuration.ExtendedRomPath, false);

        var requiresRomKey = IsEncryptedKickstart(configuration.KickstartPath);
        ValidateFile(configuration.RomKeyPath, requiresRomKey);

        ValidateFile(configuration.InitialDiskPath, false);
        foreach (var floppy in configuration.Floppies ?? []) ValidateFile(floppy.Path, true);
        foreach (var media in configuration.Media ?? []) ValidateFile(media.Path, true);
    }

    internal static bool IsEncryptedKickstart(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return false;
        try
        {
            Span<byte> header = stackalloc byte[EncryptedKickstartHeader.Length];
            using var stream = File.OpenRead(path);
            return stream.Read(header) == header.Length
                && Encoding.ASCII.GetString(header) == EncryptedKickstartHeader;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static void ValidateFile(string? path, bool required)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            if (required) throw new FileNotFoundException(null, path);
            return;
        }

        if (!File.Exists(path)) throw new FileNotFoundException(null, path);
    }
}

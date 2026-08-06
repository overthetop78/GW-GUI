namespace GWGUI.Domain.Settings;

public static class SettingsMigrator
{
    public const int CurrentVersion = 7;

    public static AppSettings Migrate(AppSettings settings)
    {
        if (settings.SchemaVersion <= 0) settings.SchemaVersion = 1;
        if (settings.SchemaVersion > CurrentVersion)
            throw new NotSupportedException($"Settings schema {settings.SchemaVersion} is newer than supported schema {CurrentVersion}.");

        Normalize(settings);
        while (settings.SchemaVersion < CurrentVersion)
        {
            switch (settings.SchemaVersion)
            {
                case 1: MigrateV1ToV2(settings); break;
                case 2: MigrateV2ToV3(settings); break;
                case 3: MigrateV3ToV4(settings); break;
                case 4: MigrateV4ToV5(settings); break;
                case 5: MigrateV5ToV6(settings); break;
                case 6: MigrateV6ToV7(settings); break;
                default: throw new NotSupportedException($"No migration exists for settings schema {settings.SchemaVersion}.");
            }
        }
        Normalize(settings);
        return settings;
    }

    private static void MigrateV1ToV2(AppSettings settings)
    {
        settings.Read.FormatId = CorrectFormatId(settings.Read.FormatId);
        settings.Conversion.SelectedFormats = settings.Conversion.SelectedFormats.Select(CorrectFormatId).Where(id => id is not null).Cast<string>().ToHashSet(StringComparer.OrdinalIgnoreCase);
        settings.Conversion.ExplicitExtensions = RenameKeys(settings.Conversion.ExplicitExtensions, CorrectFormatId);
        foreach (var profile in settings.Profiles)
        {
            profile.EnabledOptions = profile.EnabledOptions.Select(option => option.StartsWith("format:", StringComparison.Ordinal) ? "format:" + CorrectFormatId(option[7..]) : option).ToHashSet();
            profile.Values = RenameKeys(profile.Values, key => key.StartsWith("extensions:", StringComparison.Ordinal) ? "extensions:" + CorrectFormatId(key[11..]) : key);
        }
        settings.SchemaVersion = 2;
    }

    private static void MigrateV2ToV3(AppSettings settings)
    {
        settings.Write ??= new AdvancedUiSettings();
        settings.Conversion.OptionValues ??= [];
        settings.Conversion.EnabledOptions ??= [];
        settings.SchemaVersion = 3;
    }

    private static void MigrateV3ToV4(AppSettings settings)
    {
        settings.Conversion.TagPattern = " [{tag}]";
        settings.SchemaVersion = 4;
    }

    private static void MigrateV4ToV5(AppSettings settings)
    {
        // ImageExtension is additive. Existing installations retain the
        // default extension of their selected format until the next save.
        settings.SchemaVersion = 5;
    }

    private static void MigrateV5ToV6(AppSettings settings)
    {
        // USB identity aliases are additive and are populated by the next
        // hardware scan. Existing controller IDs and drive links stay intact.
        settings.SchemaVersion = 6;
    }

    private static void MigrateV6ToV7(AppSettings settings)
    {
        settings.UnconfiguredControllers = [];
        settings.SchemaVersion = 7;
    }

    private static void Normalize(AppSettings settings)
    {
        settings.Language = settings.Language?.Trim() ?? "";
        settings.DefaultImagesFolder ??= Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        settings.Window ??= new WindowPlacementSettings();
        settings.Controllers ??= [];
        settings.UnconfiguredControllers ??= [];
        settings.Drives ??= [];
        settings.Read ??= new ReadUiSettings();
        settings.Write ??= new AdvancedUiSettings();
        settings.Profiles ??= [];
        settings.Conversion ??= new ConversionUiSettings();
        settings.Read.OptionValues ??= [];
        settings.Read.EnabledOptions ??= [];
        settings.Write.OptionValues ??= [];
        settings.Write.EnabledOptions ??= [];
        settings.Conversion.SelectedFormats ??= [];
        settings.Conversion.ExplicitExtensions ??= [];
        settings.Conversion.OptionValues ??= [];
        settings.Conversion.EnabledOptions ??= [];
        settings.Conversion.RecentCustomTagPatterns ??= [];
        settings.Conversion.RecentCustomTagPatterns = settings.Conversion.RecentCustomTagPatterns
            .Where(pattern => !string.IsNullOrWhiteSpace(pattern)).Distinct(StringComparer.OrdinalIgnoreCase).Take(5).ToList();
        settings.Conversion.TagPattern = string.IsNullOrWhiteSpace(settings.Conversion.TagPattern)
            ? "[{FAMILY}-{FORMAT}] "
            : settings.Conversion.TagPattern.Replace("{tag}", "{FAMILY}-{FORMAT}", StringComparison.OrdinalIgnoreCase);
        if (string.Equals(settings.Conversion.TagPattern, " [{FAMILY}-{FORMAT}]", StringComparison.OrdinalIgnoreCase))
            settings.Conversion.TagPattern = "[{FAMILY}-{FORMAT}] ";
        foreach (var profile in settings.Profiles) { profile.Values ??= []; profile.EnabledOptions ??= []; }
    }

    private static string? CorrectFormatId(string? id) => id == "amiga.amigadoshd" ? "amiga.amigados_hd" : id;

    private static Dictionary<string, TValue> RenameKeys<TValue>(Dictionary<string, TValue> source, Func<string, string?> rename) =>
        source.Select(pair => (Key: rename(pair.Key), pair.Value)).Where(pair => pair.Key is not null).ToDictionary(pair => pair.Key!, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
}

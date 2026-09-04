namespace GWGUI.App.Services.Input.GameInput;

internal static class GameInputDeviceModelCatalog
{
    private static readonly IReadOnlyDictionary<uint, KnownDevice> KnownDevices =
        new Dictionary<uint, KnownDevice>
        {
            [Key(0x045E, 0x028E)] = new("Xbox 360 Controller for Windows", ControllerVisualModel.Xbox360, false),
            [Key(0x045E, 0x0B12)] = new("Xbox Series X Controller", ControllerVisualModel.XboxSeries, false),
            [Key(0x10F5, 0x7122)] = new("Xbox Rematch Core Wired Controller- Black", ControllerVisualModel.XboxRematchCore, true),
            [Key(0x0810, 0xE501)] = new("SEGA Mega Drive 6 boutons", ControllerVisualModel.MegaDrive6, true),
            [Key(0x0079, 0x0006)] = new("Manette Nintendo 64", ControllerVisualModel.Nintendo64, true),
            [Key(0x054C, 0x05C4)] = new("DUALSHOCK 4 Wireless Controller", ControllerVisualModel.PlayStation4, true),
            [Key(0x054C, 0x09CC)] = new("DUALSHOCK 4 Wireless Controller", ControllerVisualModel.PlayStation4, true),
            [Key(0x054C, 0x0CE6)] = new("DualSense Wireless Controller", ControllerVisualModel.PlayStation5, true)
        };

    private static readonly IReadOnlySet<uint> AmbiguousDevices = new HashSet<uint>
    {
        Key(0x081F, 0xE401)
    };

    internal static string ResolveProductName(
        ushort vendorId,
        ushort productId,
        string? windowsName,
        string? databaseName,
        string? gameInputName)
    {
        var key = Key(vendorId, productId);
        if (AmbiguousDevices.Contains(key))
            return $"Manette rétro USB ({vendorId:X4}:{productId:X4})";
        if (KnownDevices.TryGetValue(key, out var known)) return known.ProductName;
        foreach (var candidate in new[] { windowsName, databaseName, gameInputName })
            if (!string.IsNullOrWhiteSpace(candidate) &&
                !WindowsDeviceNameResolver.IsTransportOrGenericName(candidate))
                return candidate.Trim();
        return $"Controller {vendorId:X4}:{productId:X4}";
    }

    internal static (ControllerVisualModel Model, bool Exact) ResolveVisualModel(
        ushort vendorId,
        ushort productId,
        string productName,
        GameInputKind supportedInput)
    {
        var key = Key(vendorId, productId);
        if (AmbiguousDevices.Contains(key))
            return (ControllerVisualModel.GenericGamepad, false);
        if (KnownDevices.TryGetValue(key, out var known))
            return (known.VisualModel, known.ExactVisualModelMatch);

        var normalized = productName.ToLowerInvariant();
        if (normalized.Contains("dualsense") || normalized.Contains("playstation 5") || normalized.Contains("ps5"))
            return (ControllerVisualModel.PlayStation5, true);
        if (normalized.Contains("dualshock 4") || normalized.Contains("playstation 4") || normalized.Contains("ps4"))
            return (ControllerVisualModel.PlayStation4, true);
        if (normalized.Contains("xbox series"))
            return (ControllerVisualModel.XboxSeries, true);
        if (normalized.Contains("xbox 360"))
            return (ControllerVisualModel.Xbox360, false);
        if (normalized.Contains("xbox"))
            return (ControllerVisualModel.XboxOne, false);
        if (normalized.Contains("mega drive") || normalized.Contains("megadrive") || normalized.Contains("genesis"))
            return (ControllerVisualModel.MegaDrive6, true);
        if (normalized.Contains("nintendo 64") || normalized.Contains("n64"))
            return (ControllerVisualModel.Nintendo64, false);
        if (normalized.Contains("super nintendo") || normalized.Contains("snes"))
            return (ControllerVisualModel.SuperNintendo, false);
        if (normalized.Contains("master system"))
            return (ControllerVisualModel.MasterSystem, true);
        if (normalized.Contains("dreamcast"))
            return (ControllerVisualModel.Dreamcast, true);
        if (normalized.Contains("saturn"))
            return (ControllerVisualModel.Saturn, true);
        if (normalized.Contains("playstation 2") || normalized.Contains("ps2"))
            return (ControllerVisualModel.PlayStation2, true);
        if (normalized.Contains("playstation") || normalized.Contains("ps1"))
            return (ControllerVisualModel.PlayStation1, true);
        if ((supportedInput & GameInputKind.RacingWheel) != 0)
            return (ControllerVisualModel.RacingWheel, false);
        if ((supportedInput & GameInputKind.FlightStick) != 0)
            return (ControllerVisualModel.FlightStick, false);
        if ((supportedInput & GameInputKind.ArcadeStick) != 0)
            return (ControllerVisualModel.ArcadeStick, false);
        return (ControllerVisualModel.GenericGamepad, false);
    }

    internal static IReadOnlyList<ControllerVisualModel> AllVisualModels { get; } =
        Enum.GetValues<ControllerVisualModel>();

    private static uint Key(ushort vendorId, ushort productId) => ((uint)vendorId << 16) | productId;
    private sealed record KnownDevice(string ProductName, ControllerVisualModel VisualModel, bool ExactVisualModelMatch);
}

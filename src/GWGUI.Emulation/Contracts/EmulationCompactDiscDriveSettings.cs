using GWGUI.Emulation.Constants;

namespace GWGUI.Emulation.Contracts;

public sealed record EmulationCompactDiscDriveSettings(
    string Speed = EmulationCompactDiscDriveSettingsConstants.DefaultSpeed,
    bool CacheImage = false,
    bool IgnoreErrors = false);

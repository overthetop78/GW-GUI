namespace GWGUI.Emulation.Contracts;

/// <summary>Identité d’un port pour la sauvegarde : périphérique, manette physique, zone morte et associations.</summary>
/// <remarks>Le module fournit les associations déjà ordonnées pour conserver son format d’empreinte existant.</remarks>
public sealed record EmulationStateControllerFingerprint<TPeripheral>(
    int Port,
    TPeripheral Peripheral,
    string? DeviceId,
    int DeadZonePercent,
    IReadOnlyList<KeyValuePair<string, string>> Mappings);

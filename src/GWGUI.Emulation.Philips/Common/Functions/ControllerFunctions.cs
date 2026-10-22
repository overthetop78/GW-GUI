using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation;
namespace GWGUI.Emulation.Philips.Common.Functions;

internal static class ControllerFunctions
{
    // Dead zones are applied to physical sources by the mapper before it encodes native axes and carrier bits.
    internal static EmulationInputSnapshot FreezeMappedInput(EmulationInputSnapshot input) => EmulationSdk.InputProcessing.Freeze(input);
    /// <summary>Lit une commande native de souris à partir du pointeur de la frame.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    internal static short NativeMouseState(EmulationPointerState pointer, uint port, uint index, uint id) => EmulationSdk.InputProcessing.NativeMouseState(pointer, port, index, id);

    /// <summary>Lit un bouton ou le masque natif de la manette, avec la capacité fournie par l’adaptation.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    internal static short NativeJoypadState(uint buttons, uint id, uint maximumButtonCount) => EmulationSdk.InputProcessing.NativeJoypadState(buttons, id, maximumButtonCount);

    /// <summary>Lit un bouton ou le masque natif de la manette, avec la capacité fournie par l’adaptation.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    internal static short NativeJoypadState(EmulationControllerState controller, uint id, uint maximumButtonCount) => EmulationSdk.InputProcessing.NativeJoypadState(controller, id, maximumButtonCount);

    /// <summary>Lit la composante native d’un stick ou d’une gâchette.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    internal static short NativeAnalogState(EmulationControllerState controller, uint index, uint id) => EmulationSdk.InputProcessing.NativeAnalogState(controller, index, id);

    /// <summary>Additionne deux déplacements relatifs en saturant aux limites du type Int32.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    internal static int SaturatingAdd(int left, int right) => EmulationSdk.InputProcessing.SaturatingAdd(left, right);

    /// <summary>Projette les touches ou les sources physiques vers les touches émulées.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    internal static IReadOnlySet<EmulationKey> MapKeyboard(IReadOnlySet<EmulationKey> keys, IReadOnlyDictionary<string, EmulationKey>? mappings) => EmulationSdk.InputProcessing.MapKeyboard(keys, mappings);

    /// <summary>Projette les touches ou les sources physiques vers les touches émulées.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    internal static IReadOnlySet<EmulationKey> MapKeyboard(EmulationInputSnapshot snapshot, IReadOnlyDictionary<string, string>? mappings) => EmulationSdk.InputProcessing.MapKeyboard(snapshot, mappings);

    /// <summary>Résout un contrôleur physique par son identité ou son indice de repli.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    internal static EmulationControllerState ResolveController(string? deviceId, IReadOnlyList<EmulationControllerState> controllers, int fallbackIndex) => EmulationSdk.InputProcessing.ResolveController(deviceId, controllers, fallbackIndex);

    /// <summary>Résout le contrôleur désigné par une expression en conservant la politique de repli.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    internal static EmulationControllerState ResolveSourceController(string source, IReadOnlyList<EmulationControllerState> controllers, EmulationControllerState fallback) => EmulationSdk.InputProcessing.ResolveSourceController(source, controllers, fallback);

    /// <summary>Résout le contrôleur désigné par une expression en conservant la politique de repli.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    internal static EmulationControllerState ResolveSourceController(string source, IReadOnlyList<EmulationControllerState> controllers, EmulationControllerState fallback, bool fallbackWhenMissing) => EmulationSdk.InputProcessing.ResolveSourceController(source, controllers, fallback, fallbackWhenMissing);

    /// <summary>Applique les zones mortes aux axes et aux gâchettes d’un contrôleur.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    internal static EmulationControllerState ApplyDeadZones(EmulationControllerState controller, int percent) => EmulationSdk.InputProcessing.ApplyDeadZones(controller, percent);

    /// <summary>Lit la valeur d’une commande physique de contrôleur.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    internal static float ControllerSourceValue(string source, EmulationControllerState controller) => EmulationSdk.InputProcessing.ControllerSourceValue(source, controller);

    /// <summary>Évalue l’appui d’une commande de contrôleur selon les règles existantes.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    internal static bool IsControllerSourcePressed(string source, EmulationControllerState controller) => EmulationSdk.InputProcessing.IsControllerSourcePressed(source, controller);

    /// <summary>Évalue l’appui d’une commande de contrôleur selon les règles existantes.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    internal static bool IsControllerSourcePressed(string source, EmulationControllerState controller, float value) => EmulationSdk.InputProcessing.IsControllerSourcePressed(source, controller, value);

    /// <summary>Évalue la pression d’une expression de clavier, souris ou contrôleur.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    internal static float InputSourcePressure(string source, EmulationControllerState controller, EmulationInputSnapshot snapshot) => EmulationSdk.InputProcessing.InputSourcePressure(source, controller, snapshot);

    /// <summary>Évalue la pression d’une expression de clavier, souris ou contrôleur.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    internal static float InputSourcePressure(string source, EmulationControllerState controller, IReadOnlyList<EmulationControllerState> physical, IReadOnlySet<EmulationKey> keys, IReadOnlyDictionary<string, bool> mouse) => EmulationSdk.InputProcessing.InputSourcePressure(source, controller, physical, keys, mouse);

    /// <summary>Évalue l’appui d’une expression de source à partir de l’instantané physique.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    internal static bool IsMappedInputPressed(string mapping, EmulationControllerState controller, EmulationInputSnapshot snapshot) => EmulationSdk.InputProcessing.IsMappedInputPressed(mapping, controller, snapshot);

    /// <summary>Convertit une amplitude normalisée vers un axe signé sur seize bits.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    internal static short NormalizedAxis(float value) => EmulationSdk.InputProcessing.NormalizedAxis(value);

    /// <summary>Calcule l’amplitude associée après application de la zone morte en pourcentage.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    internal static float MappedInputMagnitude(string mapping, EmulationControllerState controller, EmulationInputSnapshot snapshot, int deadZone) => EmulationSdk.InputProcessing.MappedInputMagnitude(mapping, controller, snapshot, deadZone);

    /// <summary>Lit une entrée native générique par le contrat commun de traitement des entrées.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    internal static short ReadNativeInput(EmulationInputSnapshot snapshot, uint port, uint device, uint index, uint id) => EmulationSdk.InputProcessing.ReadNativeInput(snapshot, port, device, index, id);
}

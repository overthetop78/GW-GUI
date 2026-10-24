using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation;
using GWGUI.Emulation.Constants;

namespace GWGUI.Emulation.Example.Common.Functions;

/// <summary>Adaptation des périphériques et accès aux contrats communs de traitement des entrées.</summary>
/// <remarks>La projection des associations, les zones mortes et les ports désactivés sont traités dans
/// InputSnapshotFunctions. Les services retournés ici sont empruntés ; les stockages créés appartiennent
/// à leur session. Les lectures natives conservent les unités des commandes déjà projetées.</remarks>
internal static class ControllerFunctions
{
    /// <summary>Service public de consultation des descriptions et de traitement des entrées, emprunté au SDK.</summary>
    /// <remarks>Le service ne possède aucune machine ni aucun périphérique et ne conserve pas les descriptions reçues.</remarks>
    internal static IEmulationInputProcessing InputProcessing => EmulationSdk.InputProcessing;

    /// <summary>Lit le nombre de ports matériels avant les extensions propres au cœur sélectionné.</summary>
    /// <param name="configuration">Configuration empruntée dont le modèle détermine les ports de base.</param>
    /// <returns>Nombre matériel utilisé sans adaptateur ou comme comportement par défaut de son contrat.</returns>
    internal static int ModelPortCount(MachineConfiguration configuration) => ConfigurationFunctions.ControllerPortCount(configuration.Model);

    /// <summary>Résout les ports de session par le contrat Common du cœur sélectionné.</summary>
    /// <param name="configuration">Configuration empruntée, y compris les options de multitap ou d’extension.</param>
    /// <returns>Nombre annoncé par l’adaptateur ; nombre configuré, extension comprise, sans cœur sélectionné.</returns>
    /// <remarks>Le contrat reçoit la configuration entière. Cette consultation ne crée ni cœur ni ressource native.</remarks>
    internal static int PortCount(MachineConfiguration configuration) => new Engine().SelectedAdapter(configuration)?.GetControllerPortCount(configuration) ?? ConfigurationFunctions.ControllerPortCount(configuration);

    /// <summary>Service public de description, validation et projection des retours, emprunté au SDK sans ressource physique possédée.</summary>
    internal static IEmulationControllerFeedbackProcessing Feedback => EmulationSdk.ControllerFeedback;

    /// <summary>Valide la syntaxe commune des associations clavier, souris et contrôleur.</summary>
    /// <param name="binding">Association empruntée ; une valeur nulle, vide ou blanche est acceptée.</param>
    /// <returns>True si chaque composante respecte la syntaxe commune, sans modifier les libellés ni les correspondances du module.</returns>
    internal static bool IsValidBinding(string? binding) => EmulationSdk.InputProcessing.IsValidBinding(binding);

    /// <summary>Crée un accumulateur indépendant pour conserver les entrées reçues entre deux lectures du cœur.</summary>
    /// <remarks>Ce relais Common demande l’instance au contrat SDK. Les collections restent empruntées et doivent être stables ; Update additionne les mouvements avec saturation, Consume remet seulement les déplacements et molettes à zéro, et Reset efface toutes les entrées. Le service ne possède aucune ressource native.</remarks>
    /// <returns>Contrat appartenant au consommateur ; chaque appel fournit un état indépendant.</returns>
    internal static IEmulationInputAccumulator CreateInputAccumulator() => EmulationSdk.InputProcessing.CreateInputAccumulator();

    /// <summary>Crée un stockage indépendant qui copie les entrées reçues et publie un instantané stable par frame.</summary>
    /// <returns>Contrat possédé par le consommateur, avec des collections copiées et sans ressource native.</returns>
    /// <remarks>Update copie les collections et additionne les quatre composantes relatives avec saturation et échange atomique. Poll les consomme une seule fois et conserve le dernier état absolu ; Polled reste stable jusqu’au Poll suivant. Chaque stockage appartient à son appelant et reste indépendant des autres sessions.</remarks>
    internal static IEmulationInputFrames CreateInputFrames() => EmulationSdk.InputProcessing.CreateInputFrames();

    /// <summary>Associe chaque catégorie générique à la classe native qu’interrogera le cœur.</summary>
    /// <param name="peripheral">Catégorie du port, déjà contrôlée dans la configuration.</param>
    /// <returns>Classe native commune aux trois modes fictifs, indépendante du matériel physique.</returns>
    /// <remarks>Les joysticks analogiques, volants et contrôleurs rotatifs exposent les sticks et les pressions.
    /// Le pistolet et le stylet conservent leurs classes de pointage ; le clavier et la souris utilisent leurs
    /// lecteurs dédiés. Les autres catégories échangent des boutons, et Automatic reprend la manette générique.</remarks>
    internal static uint ResolveControllerDevice(PeripheralCategory peripheral) => peripheral switch
    {
        PeripheralCategory.None => ExternalCoreApiConstants.NoDevice,
        PeripheralCategory.Mouse => ExternalCoreApiConstants.MouseDevice,
        PeripheralCategory.Keyboard => ExternalCoreApiConstants.KeyboardDevice,
        PeripheralCategory.LightGun => ExternalCoreApiConstants.LightGunDevice,
        PeripheralCategory.Pointer => ExternalCoreApiConstants.PointerDevice,
        PeripheralCategory.AnalogJoystick or PeripheralCategory.DrivingController or PeripheralCategory.Paddle => ExternalCoreApiConstants.AnalogDevice,
        PeripheralCategory.Automatic or PeripheralCategory.RetroPad or PeripheralCategory.Joystick or PeripheralCategory.NumericKeypad or PeripheralCategory.DanceMat => ExternalCoreApiConstants.JoypadDevice,
        _ => throw new ArgumentOutOfRangeException(nameof(peripheral))
    };

    /// <summary>Fournit la table clavier du contrat natif par l’interface commune du SDK.</summary>
    /// <returns>Correspondances dont les identités ne dépendent d’aucune marque de machine.</returns>
    internal static IReadOnlyDictionary<EmulationKey, uint> CreateNativeKeyMap() => EmulationSdk.InputProcessing.CreateKeyMap();

    /// <summary>Lit l’instantané stable de la frame pour un callback d’entrée natif.</summary>
    /// <param name="snapshot">Entrées déjà projetées par l’adaptateur du module.</param>
    /// <param name="port">Indice du port émulé.</param>
    /// <param name="device">Classe native de périphérique demandée par le cœur.</param>
    /// <param name="index">Indice de composante de cette classe.</param>
    /// <param name="id">Identifiant de la commande native à interroger.</param>
    /// <returns>Valeur calculée par les sélecteurs existants du SDK.</returns>
    internal static short ReadNativeInput(EmulationInputSnapshot snapshot, uint port, uint device, uint index, uint id) => EmulationSdk.InputProcessing.ReadNativeInput(snapshot, port, device, index, id);

    /// <summary>Applique le changement de périphérique d’un port sans perdre ses associations physiques.</summary>
    /// <param name="configuration">Configuration d’origine à conserver immuable.</param>
    /// <param name="port">Indice du port émulé, à partir de zéro.</param>
    /// <param name="peripheral">Catégorie générique à connecter sur ce port.</param>
    /// <returns>Configuration validée dont seul le périphérique du port demandé est remplacé.</returns>
    internal static MachineConfiguration WithControllerType(MachineConfiguration configuration, int port, PeripheralCategory peripheral)
    {
        var controllers = (configuration.Input.Controllers ?? []).Where(binding => binding.Port != port).ToList();
        var previous = configuration.Input.Controllers?.FirstOrDefault(binding => binding.Port == port);
        controllers.Add(previous is null ? new ControllerBinding(port, peripheral) : previous with { Peripheral = peripheral });
        var updated = configuration with { Input = configuration.Input with { Controllers = controllers.OrderBy(binding => binding.Port).ToArray() } };
        ConfigurationFunctions.Validate(updated);
        return updated;
    }

    /// <summary>Lit une commande native de souris à partir du pointeur de la frame.</summary>
    /// <param name="pointer">Déplacements relatifs, molettes et boutons empruntés de la souris.</param>
    /// <param name="port">Indice natif du port, à partir de zéro.</param>
    /// <param name="index">Sélecteur natif du stick ou de la catégorie de composante.</param>
    /// <param name="id">Identifiant natif de la composante demandée.</param>
    /// <returns>Valeur native de la composante ; zéro lorsque la commande est inactive ou non prise en charge.</returns>
    internal static short NativeMouseState(EmulationPointerState pointer, uint port, uint index, uint id) => EmulationSdk.InputProcessing.NativeMouseState(pointer, port, index, id);

    /// <summary>Lit un bouton ou le masque natif de la manette, avec la capacité fournie par l’adaptation.</summary>
    /// <param name="buttons">Masque des boutons, avec un bit par indice natif.</param>
    /// <param name="id">Identifiant natif de la composante demandée.</param>
    /// <param name="maximumButtonCount">Nombre de bits de boutons admis par cette adaptation.</param>
    /// <returns>Valeur native de la composante ; zéro lorsque la commande est inactive ou non prise en charge.</returns>
    internal static short NativeJoypadState(uint buttons, uint id, uint maximumButtonCount) => EmulationSdk.InputProcessing.NativeJoypadState(buttons, id, maximumButtonCount);

    /// <summary>Lit un bouton ou le masque natif de la manette, avec la capacité fournie par l’adaptation.</summary>
    /// <param name="controller">État physique du contrôleur, conservé inchangé.</param>
    /// <param name="id">Identifiant natif de la composante demandée.</param>
    /// <param name="maximumButtonCount">Nombre de bits de boutons admis par cette adaptation.</param>
    /// <returns>Valeur native de la composante ; zéro lorsque la commande est inactive ou non prise en charge.</returns>
    internal static short NativeJoypadState(EmulationControllerState controller, uint id, uint maximumButtonCount) => EmulationSdk.InputProcessing.NativeJoypadState(controller, id, maximumButtonCount);

    /// <summary>Lit la composante native d’un stick ou d’une gâchette.</summary>
    /// <param name="controller">État physique du contrôleur, conservé inchangé.</param>
    /// <param name="index">Sélecteur natif du stick ou de la catégorie de composante.</param>
    /// <param name="id">Identifiant natif de la composante demandée.</param>
    /// <returns>Valeur native de la composante ; zéro lorsque la commande est inactive ou non prise en charge.</returns>
    internal static short NativeAnalogState(EmulationControllerState controller, uint index, uint id) => EmulationSdk.InputProcessing.NativeAnalogState(controller, index, id);

    /// <summary>Additionne deux déplacements relatifs en saturant aux limites du type Int32.</summary>
    /// <param name="left">Premier déplacement relatif entier à additionner.</param>
    /// <param name="right">Second déplacement relatif entier à additionner.</param>
    /// <returns>Somme bornée aux limites de System.Int32, sans débordement arithmétique.</returns>
    internal static int SaturatingAdd(int left, int right) => EmulationSdk.InputProcessing.SaturatingAdd(left, right);

    /// <summary>Projette les touches ou les sources physiques vers les touches émulées.</summary>
    /// <param name="keys">Touches physiques pressées pendant la frame, sans transfert de propriété.</param>
    /// <param name="mappings">Associations des sources clavier vers les touches émulées.</param>
    /// <returns>Nouvel ensemble stable des touches émulées actuellement pressées.</returns>
    internal static IReadOnlySet<EmulationKey> MapKeyboard(IReadOnlySet<EmulationKey> keys, IReadOnlyDictionary<string, EmulationKey>? mappings) => EmulationSdk.InputProcessing.MapKeyboard(keys, mappings);

    /// <summary>Projette les touches ou les sources physiques vers les touches émulées.</summary>
    /// <param name="snapshot">Instantané physique emprunté pendant le calcul.</param>
    /// <param name="mappings">Associations des touches émulées vers les expressions de sources physiques.</param>
    /// <returns>Nouvel ensemble stable des touches émulées actuellement pressées.</returns>
    internal static IReadOnlySet<EmulationKey> MapKeyboard(EmulationInputSnapshot snapshot, IReadOnlyDictionary<string, string>? mappings) => EmulationSdk.InputProcessing.MapKeyboard(snapshot, mappings);

    /// <summary>Résout un contrôleur physique par son identité ou son indice de repli.</summary>
    /// <param name="deviceId">Identité physique recherchée, ou null pour employer l’indice de repli.</param>
    /// <param name="controllers">Collection empruntée des états des contrôleurs physiques.</param>
    /// <param name="fallbackIndex">Indice utilisé en l’absence d’identité physique explicite.</param>
    /// <returns>État sélectionné, état de repli ou état vide selon la politique demandée.</returns>
    internal static EmulationControllerState ResolveController(string? deviceId, IReadOnlyList<EmulationControllerState> controllers, int fallbackIndex) => EmulationSdk.InputProcessing.ResolveController(deviceId, controllers, fallbackIndex);

    /// <summary>Résout le contrôleur désigné par une expression en conservant la politique de repli.</summary>
    /// <param name="source">Expression de la source physique à interpréter.</param>
    /// <param name="controllers">Collection empruntée des états des contrôleurs physiques.</param>
    /// <param name="fallback">État de repli fourni par l’adaptation.</param>
    /// <returns>État sélectionné, état de repli ou état vide selon la politique demandée.</returns>
    internal static EmulationControllerState ResolveSourceController(string source, IReadOnlyList<EmulationControllerState> controllers, EmulationControllerState fallback) => EmulationSdk.InputProcessing.ResolveSourceController(source, controllers, fallback);

    /// <summary>Résout le contrôleur désigné par une expression en conservant la politique de repli.</summary>
    /// <param name="source">Expression de la source physique à interpréter.</param>
    /// <param name="controllers">Collection empruntée des états des contrôleurs physiques.</param>
    /// <param name="fallback">État de repli fourni par l’adaptation.</param>
    /// <param name="fallbackWhenMissing">Autorise le repli lorsqu’une identité explicite est absente des sources disponibles.</param>
    /// <returns>État sélectionné, état de repli ou état vide selon la politique demandée.</returns>
    internal static EmulationControllerState ResolveSourceController(string source, IReadOnlyList<EmulationControllerState> controllers, EmulationControllerState fallback, bool fallbackWhenMissing) => EmulationSdk.InputProcessing.ResolveSourceController(source, controllers, fallback, fallbackWhenMissing);

    /// <summary>Applique les zones mortes aux axes et aux gâchettes d’un contrôleur.</summary>
    /// <param name="controller">État physique du contrôleur, conservé inchangé.</param>
    /// <param name="percent">Zone morte exprimée en pourcentage de la course.</param>
    /// <returns>Valeur calculée par le traitement commun, sans modification des entrées reçues.</returns>
    internal static EmulationControllerState ApplyDeadZones(EmulationControllerState controller, int percent) => EmulationSdk.InputProcessing.ApplyDeadZones(controller, percent);

    /// <summary>Lit la valeur d’une commande physique de contrôleur.</summary>
    /// <param name="source">Expression de la source physique à interpréter.</param>
    /// <param name="controller">État physique du contrôleur, conservé inchangé.</param>
    /// <returns>Valeur calculée par le traitement commun, sans modification des entrées reçues.</returns>
    internal static float ControllerSourceValue(string source, EmulationControllerState controller) => EmulationSdk.InputProcessing.ControllerSourceValue(source, controller);

    /// <summary>Évalue l’appui d’une commande de contrôleur selon les règles existantes.</summary>
    /// <param name="source">Expression de la source physique à interpréter.</param>
    /// <param name="controller">État physique du contrôleur, conservé inchangé.</param>
    /// <returns>True lorsque la source est considérée appuyée selon les règles et seuils du SDK.</returns>
    internal static bool IsControllerSourcePressed(string source, EmulationControllerState controller) => EmulationSdk.InputProcessing.IsControllerSourcePressed(source, controller);

    /// <summary>Évalue l’appui d’une commande de contrôleur selon les règles existantes.</summary>
    /// <param name="source">Expression de la source physique à interpréter.</param>
    /// <param name="controller">État physique du contrôleur, conservé inchangé.</param>
    /// <param name="value">Amplitude normalisée, ou valeur déjà lue de la source selon la surcharge.</param>
    /// <returns>True lorsque la source est considérée appuyée selon les règles et seuils du SDK.</returns>
    internal static bool IsControllerSourcePressed(string source, EmulationControllerState controller, float value) => EmulationSdk.InputProcessing.IsControllerSourcePressed(source, controller, value);

    /// <summary>Évalue la pression d’une expression de clavier, souris ou contrôleur.</summary>
    /// <param name="source">Expression de la source physique à interpréter.</param>
    /// <param name="controller">État physique du contrôleur, conservé inchangé.</param>
    /// <param name="snapshot">Instantané physique emprunté pendant le calcul.</param>
    /// <returns>Valeur calculée par le traitement commun, sans modification des entrées reçues.</returns>
    internal static float InputSourcePressure(string source, EmulationControllerState controller, EmulationInputSnapshot snapshot) => EmulationSdk.InputProcessing.InputSourcePressure(source, controller, snapshot);

    /// <summary>Évalue la pression d’une expression de clavier, souris ou contrôleur.</summary>
    /// <param name="source">Expression de la source physique à interpréter.</param>
    /// <param name="controller">État physique du contrôleur, conservé inchangé.</param>
    /// <param name="physical">Collection empruntée des autres contrôleurs physiques.</param>
    /// <param name="keys">Touches physiques pressées pendant la frame, sans transfert de propriété.</param>
    /// <param name="mouse">États empruntés des commandes physiques de souris.</param>
    /// <returns>Valeur calculée par le traitement commun, sans modification des entrées reçues.</returns>
    internal static float InputSourcePressure(string source, EmulationControllerState controller, IReadOnlyList<EmulationControllerState> physical, IReadOnlySet<EmulationKey> keys, IReadOnlyDictionary<string, bool> mouse) => EmulationSdk.InputProcessing.InputSourcePressure(source, controller, physical, keys, mouse);

    /// <summary>Évalue l’appui d’une expression de source à partir de l’instantané physique.</summary>
    /// <param name="mapping">Expression de la source associée à une commande émulée.</param>
    /// <param name="controller">État physique du contrôleur, conservé inchangé.</param>
    /// <param name="snapshot">Instantané physique emprunté pendant le calcul.</param>
    /// <returns>True lorsque la source est considérée appuyée selon les règles et seuils du SDK.</returns>
    internal static bool IsMappedInputPressed(string mapping, EmulationControllerState controller, EmulationInputSnapshot snapshot) => EmulationSdk.InputProcessing.IsMappedInputPressed(mapping, controller, snapshot);

    /// <summary>Convertit une amplitude normalisée vers un axe signé sur seize bits.</summary>
    /// <param name="value">Amplitude normalisée, ou valeur déjà lue de la source selon la surcharge.</param>
    /// <returns>Axe signé sur seize bits après bornage de l’amplitude normalisée.</returns>
    internal static short NormalizedAxis(float value) => EmulationSdk.InputProcessing.NormalizedAxis(value);

    /// <summary>Calcule l’amplitude associée après application de la zone morte en pourcentage.</summary>
    /// <param name="mapping">Expression de la source associée à une commande émulée.</param>
    /// <param name="controller">État physique du contrôleur, conservé inchangé.</param>
    /// <param name="snapshot">Instantané physique emprunté pendant le calcul.</param>
    /// <param name="deadZone">Zone morte des associations, exprimée en pourcentage.</param>
    /// <returns>Valeur calculée par le traitement commun, sans modification des entrées reçues.</returns>
    internal static float MappedInputMagnitude(string mapping, EmulationControllerState controller, EmulationInputSnapshot snapshot, int deadZone) => EmulationSdk.InputProcessing.MappedInputMagnitude(mapping, controller, snapshot, deadZone);

    /// <summary>Calcule le masque natif des touches Majuscule, Contrôle et Alt.</summary>
    /// <param name="keys">Touches appuyées empruntées pendant le calcul ; le service ne conserve pas cet ensemble.</param>
    /// <returns>Masque natif des modificateurs ; les touches droites et gauches utilisent les mêmes bits.</returns>
    internal static ushort Modifiers(IReadOnlySet<EmulationKey> keys) => EmulationSdk.InputProcessing.Modifiers(keys);

    /// <summary>Calcule le caractère associé à un code natif selon Majuscule et le verrouillage des majuscules.</summary>
    /// <param name="code">Code clavier natif, distinct de la valeur numérique de l’énumération EmulationKey.</param>
    /// <param name="keys">Touches appuyées empruntées pour déterminer les modificateurs.</param>
    /// <returns>Caractère natif, ou zéro lorsque le code ne produit pas de caractère imprimable.</returns>
    internal static uint Character(uint code, IReadOnlySet<EmulationKey> keys) => EmulationSdk.InputProcessing.Character(code, keys);

    /// <summary>Complète une table de correspondances avec une plage continue de touches et de codes.</summary>
    /// <param name="map">Table mutable appartenant à l’appelant ; les entrées de la plage sont écrites sur place.</param>
    /// <param name="firstKey">Première touche de la plage dans l’énumération.</param>
    /// <param name="firstCode">Premier code natif, incrémenté pour chaque touche.</param>
    /// <param name="count">Nombre de touches à écrire ; une valeur non positive ne produit aucune écriture.</param>
    internal static void AddRange(IDictionary<EmulationKey, uint> map, EmulationKey firstKey, uint firstCode, int count) => EmulationSdk.InputProcessing.AddRange(map, firstKey, firstCode, count);

    /// <summary>Publie les relâchements avant les pressions, suivant les politiques fournies par l’adaptateur.</summary>
    /// <param name="keys">Touches courantes empruntées pour cette publication.</param>
    /// <param name="keyMap">Correspondances empruntées ; les touches absentes de cette table ne produisent pas d’événement.</param>
    /// <param name="modifiers">Masque natif transmis tel quel à chaque événement.</param>
    /// <param name="previous">Référence de l’état précédent du propriétaire, remplacée par une copie des touches après succès.</param>
    /// <param name="keyboardEvent">Référence du callback enregistré par le propriétaire ; les remplacements réentrants restent visibles.</param>
    /// <param name="isModifier">Politique de priorité : modificateurs relâchés après les touches et pressés avant elles.</param>
    /// <param name="characterFor">Politique de caractère appliquée aux pressions ; les relâchements transmettent zéro.</param>
    /// <remarks>Le propriétaire vérifie la présence du callback avant publication. Une exception interrompt la publication
    /// et empêche sa copie finale de l’état. Les changements réentrants de previous et keyboardEvent restent visibles ;
    /// aucune ressource ni référence de session n’est conservée par le service.</remarks>
    internal static void PublishTransitions(IReadOnlySet<EmulationKey> keys, IReadOnlyDictionary<EmulationKey, uint> keyMap, ushort modifiers,
        ref IReadOnlySet<EmulationKey> previous, ref ExternalCoreApi.KeyboardEvent? keyboardEvent,
        Func<EmulationKey, bool> isModifier, Func<uint, IReadOnlySet<EmulationKey>, uint> characterFor)
        => EmulationSdk.InputProcessing.PublishTransitions(keys, keyMap, modifiers, ref previous, ref keyboardEvent, isModifier, characterFor);

    /// <summary>Publie les transitions de codes natifs sans remplacer les ensembles détenus par l’appelant.</summary>
    /// <typeparam name="TSet">Type d’ensemble de codes choisi par le propriétaire pour son état de session.</typeparam>
    /// <param name="current">Codes courants empruntés ; aucun ensemble de remplacement n’est créé.</param>
    /// <param name="previous">Référence de l’ensemble précédent, permettant de voir une réaffectation réentrante entre les deux phases.</param>
    /// <param name="keyboardEvent">Callback emprunté pour cette publication ; le service ne le mémorise pas.</param>
    /// <param name="modifiers">Masque natif transmis sans transformation.</param>
    /// <param name="orderReleased">Ordre des codes relâchés ; cette phase précède les pressions.</param>
    /// <param name="orderPressed">Ordre des codes pressés, évalué après les relâchements.</param>
    /// <param name="characterFor">Caractère calculé pour chaque pression ; les relâchements transmettent zéro.</param>
    /// <remarks>Le propriétaire valide le callback et conserve la responsabilité de mémoriser current après succès.
    /// Les exceptions des politiques et du callback sont propagées ; aucune fermeture ni destruction du callback emprunté n’a lieu.</remarks>
    internal static void PublishCodeTransitions<TSet>(TSet current, ref TSet previous, ExternalCoreApi.KeyboardEvent keyboardEvent, ushort modifiers,
        Func<IEnumerable<uint>, IEnumerable<uint>> orderReleased, Func<IEnumerable<uint>, IEnumerable<uint>> orderPressed, Func<uint, uint> characterFor) where TSet : IReadOnlySet<uint>
        => EmulationSdk.InputProcessing.PublishCodeTransitions(current, ref previous, keyboardEvent, modifiers, orderReleased, orderPressed, characterFor);

    /// <summary>Convertit une catégorie publique vers le type de périphérique attendu par les règles existantes du module.</summary>
    /// <param name="peripheral">Catégorie générique fournie par le contrat public des entrées.</param>
    /// <param name="configuration">Configuration de session empruntée, utilisée lorsque la correspondance dépend du modèle ou du choix courant.</param>
    /// <param name="port">Indice du port ; la commande conserve le contrôle des ports et des choix réellement autorisés.</param>
    /// <returns>Catégorie interne attendue par le cœur, sans créer ni modifier de périphérique.</returns>
    /// <remarks>Le tapis de danse et le stylet conservent leur catégorie jusqu’à la sélection de leur classe native.
    /// Cette opération conserve les autres correspondances. La sélection native et la publication
    /// des associations restent dans la commande du propriétaire de la machine, après acceptation du cœur.</remarks>
    internal static PeripheralCategory FromCommon(EmulationPeripheralCategory peripheral, MachineConfiguration configuration, int port)
    {
        if (port < 0 || port >= PortCount(configuration)) throw new ArgumentOutOfRangeException(nameof(port));
        if (!Enum.IsDefined(peripheral)) throw new ArgumentOutOfRangeException(nameof(peripheral));
        return peripheral switch
        {
            EmulationPeripheralCategory.None => PeripheralCategory.None,
            EmulationPeripheralCategory.Automatic => PeripheralCategory.Automatic,
            EmulationPeripheralCategory.Keyboard => PeripheralCategory.Keyboard,
            EmulationPeripheralCategory.Mouse => PeripheralCategory.Mouse,
            EmulationPeripheralCategory.Joystick => PeripheralCategory.Joystick,
            EmulationPeripheralCategory.AnalogJoystick => PeripheralCategory.AnalogJoystick,
            EmulationPeripheralCategory.Paddle => PeripheralCategory.Paddle,
            EmulationPeripheralCategory.LightGun => PeripheralCategory.LightGun,
            EmulationPeripheralCategory.NumericKeypad => PeripheralCategory.NumericKeypad,
            EmulationPeripheralCategory.DrivingController => PeripheralCategory.DrivingController,
            EmulationPeripheralCategory.DanceMat => PeripheralCategory.DanceMat,
            EmulationPeripheralCategory.Pointer => PeripheralCategory.Pointer,
            _ => PeripheralCategory.RetroPad
        };

    }
}

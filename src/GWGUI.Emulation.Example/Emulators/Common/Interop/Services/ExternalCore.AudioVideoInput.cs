using CommonControllerFunctions = GWGUI.Emulation.Example.Common.Functions.ControllerFunctions;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Example.Common.Functions;
using GWGUI.Emulation.Example.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Example.Emulators.Common.Interop.Dictionaries;
using SimulationConstants = GWGUI.Emulation.Example.Emulators.Common.Interop.Constants.CoreConstants;

namespace GWGUI.Emulation.Example.Emulators.Common.Interop.Services;

/// <summary>Production d’une frame déterministe et adaptation des trois chemins d’appel du simulateur.</summary>
internal sealed partial class ExternalCore
{
    /// <summary>Callbacks empruntés à l’hôte ; leur durée de vie est bornée par celle du cœur.</summary>
    private ExternalCoreApi.EnvironmentCallback? _environment;
    /// <summary>Récepteur vidéo du chemin Libretro.</summary>
    private ExternalCoreApi.VideoCallback? _videoCallback;
    /// <summary>Récepteur audio échantillon par échantillon, conservé selon le contrat ABI.</summary>
    private ExternalCoreApi.AudioSampleCallback? _sampleCallback;
    /// <summary>Récepteur audio par lots du chemin Libretro.</summary>
    private ExternalCoreApi.AudioBatchCallback? _batchCallback;
    /// <summary>Demande de rafraîchissement des entrées avant calcul.</summary>
    private ExternalCoreApi.InputPollCallback? _pollCallback;
    /// <summary>Lecteur d’entrées natif fourni par l’adaptation Common.</summary>
    private ExternalCoreApi.InputStateCallback? _inputCallback;
    /// <summary>Instantané des entrées déjà figé par la machine propriétaire.</summary>
    private EmulationInputSnapshot _input = EmulationInputSnapshot.Empty;
    /// <summary>Types de périphériques natifs raccordés aux ports, indexés à partir de zéro.</summary>
    private readonly Dictionary<int, uint> _ports = [];
    /// <summary>Codes clavier uniques fournis par Common, conservés pour interroger toutes les touches du clavier simulé.</summary>
    private static readonly IReadOnlyList<uint> KeyboardCodes = Array.AsReadOnly(CommonControllerFunctions.CreateNativeKeyMap().Values.Distinct().ToArray());
    /// <summary>Image et son bruts du dernier calcul, lus après l’export Run dans le mode externe.</summary>
    private byte[] _pixels = [];
    /// <summary>Lot PCM stéréo brut du dernier calcul.</summary>
    private short[] _samples = [];

    /// <summary>Expose la dernière image publiée par le cœur simulé.</summary>
    /// <remarks><para>La valeur est absente avant la première publication. Le consommateur emprunte l&apos;image et respecte la durée de validité de ses données ; il ne détruit pas le cœur qui la fournit.</para></remarks>
    public VideoFrame? LatestVideoFrame { get; private set; }
    /// <summary>Expose le dernier bloc audio publié par le cœur simulé.</summary>
    /// <remarks><para>La valeur est absente tant qu&apos;aucun bloc n&apos;a été produit. Les données servent à la restitution de la session et ne possèdent pas de périphérique de sortie.</para></remarks>
    public AudioChunk? LatestAudioChunk { get; private set; }
    /// <summary>Expose un instantané des demandes de retour vers les périphériques.</summary>
    /// <remarks><para>Le stockage de retours appartient au cœur. Sans stockage actif, la liste est vide ; le consommateur ne pilote que les sorties autorisées par Common.</para><para>Instantané commun aux trois modes ; sa lecture n’active aucun moteur physique.</para></remarks>
    public IReadOnlyList<ControllerFeedbackState> ControllerFeedback => _feedback?.Snapshot() ?? [];
    /// <summary>Retire un lot produit ; une fin de vidage normale ne compte pas comme manque audio.</summary>
    /// <param name="chunk">Bloc audio retiré du tampon si disponible ; null lorsqu&apos;aucun bloc n&apos;a été extrait.</param>
    /// <returns>True si un bloc a été retiré du tampon et fourni au paramètre de sortie.</returns>
    public bool TryDequeueAudio(out AudioChunk? chunk) => _audio.TryDequeueNative(out chunk);
    /// <summary>Remplace les entrées de la frame ; le contrat Common garantit leur instantané immuable.</summary>
    /// <param name="snapshot">Instantané d&apos;entrées emprunté ; ses collections restent stables pendant leur consommation.</param>
    public void SetInput(EmulationInputSnapshot snapshot) { EnsureReady(); _input = snapshot; }
    /// <summary>Raccorde un périphérique au port après vérification de la capacité du profil courant.</summary>
    /// <param name="port">Index technique du port, à partir de zéro, vérifié selon les capacités de la configuration.</param>
    /// <param name="peripheral">Catégorie de périphérique à décrire ou à appliquer au port.</param>
    public void SetControllerPortDevice(int port, PeripheralCategory peripheral)
    {
        EnsureReady();
        if (port < 0 || port >= ConfigurationFunctions.ControllerPortCount(_configuration!)) throw new ArgumentOutOfRangeException(nameof(port));
        _feedback?.ResetPort(port);
        var device = CommonControllerFunctions.ResolveControllerDevice(peripheral);
        if (Emulator == Emulator.Internal) _ports[port] = device;
        else _exports.SetControllerPortDevice((uint)port, device);
    }
    /// <summary>Relie la lecture ABI au même interpréteur de boutons, axes, souris et clavier que Common.</summary>
    /// <param name="port">Index du port demandé par le protocole natif du cœur.</param>
    /// <param name="device">Identifiant du type de périphérique selon le protocole natif d&apos;entrées.</param>
    /// <param name="index">Index de la composante du périphérique, interprété selon son type dans le protocole natif.</param>
    /// <param name="id">Identifiant de la commande d&apos;entrée dans le protocole natif du périphérique.</param>
    /// <returns>Valeur native signée sur seize bits pour la commande désignée ; son unité dépend du type de commande.</returns>
    private short ControllerInput(uint port, uint device, uint index, uint id) => CommonControllerFunctions.ReadNativeInput(_input, port, device, index, id);

    /// <summary>Exécute une frame par le chemin annoncé par l’adaptateur.</summary>
    /// <remarks>Les chemins interne et externe lisent le résultat du calcul ; Libretro reçoit ce même
    /// résultat pendant les callbacks. Une frame est publiée une seule fois, quel que soit le chemin.</remarks>
    public void RunFrame()
    {
        EnsureReady();
        if (ShutdownRequested) return;
        if (Emulator == Emulator.Internal) CalculateFrame();
        else _exports.Run();
        if (Emulator != Emulator.Libretro) PublishOutputs();
    }

    /// <summary>Calcule un motif mobile et un son facultatif ; aucune instruction d’une machine réelle n’est exécutée.</summary>
    /// <remarks>Les commandes des périphériques sélectionnés influent sur le motif pour rendre leur raccordement
    /// observable. La RAM change uniquement en mémoire ; aucun média utilisateur n’est modifié.</remarks>
    private void CalculateFrame()
    {
        _pollCallback?.Invoke();
        var inputValue = ReadFrameInput();
        UpdateControllerFeedback();
        _ticks = checked(_ticks + 1);
        _sequence = checked(_sequence + 1);
        _memory[0] = unchecked((byte)(_ticks + inputValue));
        var (width, height) = OptionCatalog.Resolution(_configuration!);
        var format = OptionCatalog.PixelFormat(_configuration!);
        var bytesPerPixel = format == EmulationPixelFormat.Xrgb8888 ? sizeof(uint) : sizeof(ushort);
        var length = checked(width * height * bytesPerPixel);
        if (_pixels.Length != length) _pixels = new byte[length];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var red = (byte)(x + _memory[0]);
            var green = (byte)(y + _ticks);
            var blue = (byte)(x ^ y);
            var offset = (y * width + x) * bytesPerPixel;
            if (format == EmulationPixelFormat.Xrgb8888) BinaryPrimitives.WriteUInt32LittleEndian(_pixels.AsSpan(offset), (uint)((red << 16) | (green << 8) | blue));
            else
            {
                var pixel = format == EmulationPixelFormat.Rgb565 ? ((red >> 3) << 11) | ((green >> 2) << 5) | (blue >> 3) : ((red >> 3) << 10) | ((green >> 3) << 5) | (blue >> 3);
                BinaryPrimitives.WriteUInt16LittleEndian(_pixels.AsSpan(offset), (ushort)pixel);
            }
        }
        var frames = SampleRate / SimulationConstants.FramesPerSecond;
        if (_samples.Length != frames * AudioConstants.StereoChannelCount) _samples = new short[frames * AudioConstants.StereoChannelCount];
        var tone = _configuration!.AudioEnabled && _configuration.Options.GetValueOrDefault(OptionConstants.AudioTone) == OptionConstants.Enabled;
        for (var frame = 0; frame < frames; frame++)
        {
            var sample = tone ? (short)(Math.Sin(2 * Math.PI * SimulationConstants.ToneFrequency * ((_ticks - 1) * frames + frame) / SampleRate) * SimulationConstants.ToneAmplitude) : (short)0;
            _samples[frame * 2] = sample;
            _samples[frame * 2 + 1] = sample;
        }
        if (Emulator == Emulator.Libretro) PublishNative(width, height, width * bytesPerPixel);
    }

    /// <summary>Produit les deux effets de contrôle en utilisant le stockage de callbacks du Common.</summary>
    /// <remarks>La gâchette gauche commande l’effet fort, la droite l’effet faible. Cette règle rend
    /// le mock observable sans jeu réel : chaque frame remplace les deux intensités, y compris par zéro.
    /// Les modes interne, Libretro et externe passent ici par le même calcul. Un port désactivé ou clavier
    /// ne produit aucun effet ; les associations physiques restent appliquées par Machine.Feedback.</remarks>
    private void UpdateControllerFeedback()
    {
        if (_feedback is null || _configuration is null) return;
        for (var port = 0; port < ConfigurationFunctions.ControllerPortCount(_configuration!); port++)
        {
            var peripheral = _configuration.Input.Controllers?.FirstOrDefault(binding => binding.Port == port)?.Peripheral ?? PeripheralCategory.RetroPad;
            var device = _ports.GetValueOrDefault(port, CommonControllerFunctions.ResolveControllerDevice(peripheral));
            if (device == CommonControllerFunctions.ResolveControllerDevice(PeripheralCategory.None) || device == CommonControllerFunctions.ResolveControllerDevice(PeripheralCategory.Keyboard))
            {
                _feedback.ResetPort(port);
                continue;
            }
            var state = port < _input.Controllers.Count ? _input.Controllers[port] : EmulationControllerState.Empty;
            _feedback.SetState((uint)port, SimulationConstants.StrongRumbleEffect, FeedbackStrength(state.LeftTrigger));
            _feedback.SetState((uint)port, SimulationConstants.WeakRumbleEffect, FeedbackStrength(state.RightTrigger));
        }
    }

    /// <summary>Convertit la course positive d’une gâchette vers l’amplitude non signée du callback.</summary>
    /// <param name="trigger">Valeur signée sur seize bits ; les valeurs négatives sont ramenées à zéro.</param>
    /// <returns>Intensité entre zéro et ushort.MaxValue, sans dépassement lors du produit calculé sur UInt32.</returns>
    private static ushort FeedbackStrength(short trigger) => (ushort)((uint)Math.Clamp(trigger, (short)0, short.MaxValue) * ushort.MaxValue / short.MaxValue);

    /// <summary>Efface les effets de tous les ports lors d’un arrêt, reset ou chargement d’état.</summary>
    /// <remarks>ResetPort ne ferme pas le stockage : une frame ultérieure peut produire de nouveaux effets.
    /// Dispose ferme ce stockage après le détachement des callbacks et refuse alors tout effet tardif.</remarks>
    private void ClearControllerFeedback()
    {
        if (_feedback is null || _configuration is null) return;
        for (var port = 0; port < ConfigurationFunctions.ControllerPortCount(_configuration!); port++) _feedback.ResetPort(port);
    }

    /// <summary>Interroge les classes natives des ports du profil et additionne leurs valeurs pour le motif de contrôle.</summary>
    /// <returns>Contribution des commandes de la frame, sans modifier les entrées empruntées.</returns>
    /// <remarks>Avant une commande de changement de périphérique, le choix provient de la configuration initiale.
    /// Ensuite, le choix enregistré dans les exports prime. Un port désactivé ne contribue plus au calcul.
    /// Les trois modes interrogent les mêmes identifiants ; les deux modes à exports passent par leur callback.
    /// Chaque classe conserve ses unités : déplacements relatifs, positions absolues, pressions et états de boutons.
    /// Leur somme sert seulement à rendre les lectures observables dans le motif fictif ; elle ne convertit pas
    /// les commandes en instructions d’une machine réelle et ne modifie aucune association configurée.</remarks>
    private int ReadFrameInput()
    {
        var configuration = _configuration!;
        var result = 0;
        for (var port = 0; port < ConfigurationFunctions.ControllerPortCount(configuration); port++)
        {
            var peripheral = configuration.Input.Controllers?.FirstOrDefault(binding => binding.Port == port)?.Peripheral ?? PeripheralCategory.RetroPad;
            var device = _ports.TryGetValue(port, out var connected) ? connected : CommonControllerFunctions.ResolveControllerDevice(peripheral);
            short Read(uint index, uint id) => Emulator == Emulator.Internal ? ControllerInput((uint)port, device, index, id) : _inputCallback?.Invoke((uint)port, device, index, id) ?? 0;
            switch (device)
            {
                case ExternalCoreApiConstants.JoypadDevice:
                    for (uint id = 0; id < sizeof(uint) * 8; id++) result += Read(0, id);
                    break;
                case ExternalCoreApiConstants.MouseDevice:
                    for (uint id = ExternalCoreApiConstants.MouseXId; id <= ExternalCoreApiConstants.MouseButton5Id; id++) result += Read(0, id);
                    break;
                case ExternalCoreApiConstants.KeyboardDevice:
                    foreach (var code in KeyboardCodes) result += Read(0, code);
                    break;
                case ExternalCoreApiConstants.LightGunDevice:
                    for (uint id = ExternalCoreApiConstants.LightGunTriggerId; id <= ExternalCoreApiConstants.LightGunReloadId; id++) result += Read(0, id);
                    break;
                case ExternalCoreApiConstants.AnalogDevice:
                    for (uint index = ExternalCoreApiConstants.LeftAnalogIndex; index <= ExternalCoreApiConstants.RightAnalogIndex; index++)
                    for (uint id = ExternalCoreApiConstants.AnalogXId; id <= ExternalCoreApiConstants.AnalogYId; id++) result += Read(index, id);
                    for (uint id = 0; id < sizeof(ushort) * 8; id++) result += Read(ExternalCoreApiConstants.ButtonAnalogIndex, id);
                    break;
                case ExternalCoreApiConstants.PointerDevice:
                    for (uint id = ExternalCoreApiConstants.PointerXId; id <= ExternalCoreApiConstants.PointerCountId; id++) result += Read(0, id);
                    result += Read(0, ExternalCoreApiConstants.PointerOffscreenId);
                    break;
            }
        }
        return result;
    }

    /// <summary>Épingle les seuls tableaux empruntés pendant les callbacks et libère les deux épingles dans finally.</summary>
    /// <param name="width">Largeur de l&apos;image, exprimée en pixels.</param>
    /// <param name="height">Hauteur de l&apos;image, exprimée en pixels.</param>
    /// <param name="pitch">Nombre d&apos;octets d&apos;une ligne de pixels, incluant son éventuel remplissage.</param>
    private void PublishNative(int width, int height, int pitch)
    {
        GCHandle pixels = default, samples = default;
        try
        {
            pixels = GCHandle.Alloc(_pixels, GCHandleType.Pinned);
            samples = GCHandle.Alloc(_samples, GCHandleType.Pinned);
            _videoCallback?.Invoke(pixels.AddrOfPinnedObject(), (uint)width, (uint)height, (nuint)pitch);
            if (_batchCallback is not null) _batchCallback(samples.AddrOfPinnedObject(), (nuint)(_samples.Length / 2));
            else if (_sampleCallback is not null)
                for (var i = 0; i < _samples.Length; i += 2) _sampleCallback(_samples[i], _samples[i + 1]);
        }
        finally
        {
            if (samples.IsAllocated) samples.Free();
            if (pixels.IsAllocated) pixels.Free();
        }
    }

    /// <summary>Copie les résultats gérés dans les tampons de publication sans exposer les tableaux de calcul.</summary>
    private void PublishOutputs()
    {
        var (width, height) = OptionCatalog.Resolution(_configuration!);
        var pixels = _video.Rent(_pixels.Length);
        _pixels.CopyTo(pixels, 0);
        LatestVideoFrame = new(pixels.AsMemory(0, _pixels.Length), width, height, _pixels.Length / height, OptionCatalog.PixelFormat(_configuration!), (float)width / height, _sequence, Timestamp);
        PublishAudio(_samples.ToArray());
    }
    /// <summary>Instant émulé de la frame, exprimé en secondes ; la séquence conserve l’ordre de publication.</summary>
    private TimeSpan Timestamp => TimeSpan.FromSeconds(_ticks / FramesPerSecond);
    /// <summary>Copie immédiatement les pixels natifs avant la fin de validité du pointeur emprunté.</summary>
    /// <param name="data">Adresse des pixels natifs, copiés avant la fin de validité du pointeur emprunté.</param>
    /// <param name="width">Largeur de l&apos;image, exprimée en pixels.</param>
    /// <param name="height">Hauteur de l&apos;image, exprimée en pixels.</param>
    /// <param name="pitch">Nombre d&apos;octets d&apos;une ligne de pixels, incluant son éventuel remplissage.</param>
    private void ReceiveVideo(nint data, uint width, uint height, nuint pitch)
    {
        var pixels = _video.CopyFramePixels(data, height, pitch);
        LatestVideoFrame = new(pixels.AsMemory(0, checked((int)(height * pitch))), (int)width, (int)height, (int)pitch, OptionCatalog.PixelFormat(_configuration!), (float)width / height, _sequence, Timestamp);
    }
    /// <summary>Copie un lot natif par le traitement commun et annonce le nombre de frames consommées.</summary>
    /// <param name="data">Adresse des échantillons PCM signés sur seize bits, entrelacés par canal et copiés pendant le callback.</param>
    /// <param name="frames">Nombre de trames audio ; chaque trame comprend les échantillons de tous ses canaux.</param>
    /// <returns>Nombre de trames audio consommées, identique à la quantité transmise au callback.</returns>
    private nuint ReceiveAudio(nint data, nuint frames) { PublishAudio(AudioFunctions.Processing.CopyBatch(data, frames)); return frames; }
    /// <summary>Publie un lot possédé par la file audio avec les mêmes unités que le contrat SDK.</summary>
    /// <param name="samples">Échantillons PCM signés sur seize bits, entrelacés par canal ; le tableau doit rester stable pendant l&apos;utilisation du bloc publié.</param>
    private void PublishAudio(short[] samples)
    {
        LatestAudioChunk = new(samples, SampleRate, samples.Length / AudioConstants.StereoChannelCount, _sequence, Timestamp);
        _audio.Enqueue(LatestAudioChunk);
    }
    /// <summary>Retire les sorties transitoires après arrêt, reset ou restauration d’état.</summary>
    private void ClearOutputs() { LatestVideoFrame = null; LatestAudioChunk = null; _audio.ClearNative(() => { }); ClearControllerFeedback(); }
}

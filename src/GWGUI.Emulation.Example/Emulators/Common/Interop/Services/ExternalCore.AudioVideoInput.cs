using System.Buffers.Binary;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Interop;
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
    /// <summary>Image et son bruts du dernier calcul, lus après l’export Run dans le mode externe.</summary>
    private byte[] _pixels = [];
    /// <summary>Lot PCM stéréo brut du dernier calcul.</summary>
    private short[] _samples = [];

    /// <inheritdoc/>
    public VideoFrame? LatestVideoFrame { get; private set; }
    /// <inheritdoc/>
    public AudioChunk? LatestAudioChunk { get; private set; }
    /// <inheritdoc/>
    public IReadOnlyList<ControllerFeedbackState> ControllerFeedback => [];
    /// <summary>Retire un lot produit ; une fin de vidage normale ne compte pas comme manque audio.</summary>
    public bool TryDequeueAudio(out AudioChunk? chunk) => _audio.TryDequeueNative(out chunk);
    /// <summary>Remplace les entrées de la frame ; le contrat Common garantit leur instantané immuable.</summary>
    public void SetInput(EmulationInputSnapshot snapshot) { EnsureReady(); _input = snapshot; }
    /// <summary>Raccorde un périphérique au port après vérification de la capacité du profil courant.</summary>
    public void SetControllerPortDevice(int port, PeripheralCategory peripheral)
    {
        EnsureReady();
        if (port < 0 || port >= ConfigurationFunctions.ControllerPortCount(_configuration!.Model)) throw new ArgumentOutOfRangeException(nameof(port));
        var device = peripheral switch { PeripheralCategory.None => 0u, PeripheralCategory.Mouse => 2u, PeripheralCategory.Keyboard => 3u, _ => 1u };
        if (Emulator == Emulator.Internal) _ports[port] = device;
        else _exports.SetControllerPortDevice((uint)port, device);
    }
    /// <summary>Relie la lecture ABI au même interpréteur de boutons, axes, souris et clavier que Common.</summary>
    private short ControllerInput(uint port, uint device, uint index, uint id) => ControllerFunctions.ReadNativeInput(_input, port, device, index, id);

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
    /// <remarks>Le premier bouton du premier port influe sur le motif pour rendre le raccordement des
    /// entrées observable. La RAM change uniquement en mémoire ; aucun média utilisateur n’est modifié.</remarks>
    private void CalculateFrame()
    {
        _pollCallback?.Invoke();
        var pressed = Emulator == Emulator.Internal ? ControllerInput(0, 1, 0, 0) : _inputCallback?.Invoke(0, 1, 0, 0) ?? 0;
        _ticks = checked(_ticks + 1);
        _sequence = checked(_sequence + 1);
        _memory[0] = unchecked((byte)(_ticks + pressed));
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

    /// <summary>Épingle les seuls tableaux empruntés pendant les callbacks et libère les deux épingles dans finally.</summary>
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
    private void ReceiveVideo(nint data, uint width, uint height, nuint pitch)
    {
        var pixels = _video.CopyFramePixels(data, height, pitch);
        LatestVideoFrame = new(pixels.AsMemory(0, checked((int)(height * pitch))), (int)width, (int)height, (int)pitch, OptionCatalog.PixelFormat(_configuration!), (float)width / height, _sequence, Timestamp);
    }
    /// <summary>Copie un lot natif par le traitement commun et annonce le nombre de frames consommées.</summary>
    private nuint ReceiveAudio(nint data, nuint frames) { PublishAudio(AudioFunctions.CopyBatch(data, frames)); return frames; }
    /// <summary>Publie un lot possédé par la file audio avec les mêmes unités que le contrat SDK.</summary>
    private void PublishAudio(short[] samples)
    {
        LatestAudioChunk = new(samples, SampleRate, samples.Length / AudioConstants.StereoChannelCount, _sequence, Timestamp);
        _audio.Enqueue(LatestAudioChunk);
    }
    /// <summary>Retire les sorties transitoires après arrêt, reset ou restauration d’état.</summary>
    private void ClearOutputs() { LatestVideoFrame = null; LatestAudioChunk = null; _audio.ClearNative(() => { }); }
}

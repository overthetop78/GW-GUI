using System.IO;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Example.Emulators.Common.Interop.Dictionaries;
using SimulationConstants = GWGUI.Emulation.Example.Emulators.Common.Interop.Constants.CoreConstants;

namespace GWGUI.Emulation.Example.Emulators.Common.Interop.Services;

/// <summary>Commandes d’état et d’options ; l’enveloppe d’identité et d’intégrité des fichiers appartient à Common.</summary>
internal sealed partial class ExternalCore
{
    /// <summary>Réinitialise le compteur et la RAM à l’image de démarrage, sans réécrire le firmware ni les médias.</summary>
    public void HardReset() { EnsureReady(); _bootMemory.CopyTo(_memory, 0); SoftReset(); }
    /// <summary>Réinitialise la logique tout en conservant la RAM et les médias montés.</summary>
    public void SoftReset() { EnsureReady(); _ticks = 0; ShutdownRequested = false; ClearOutputs(); }

    /// <summary>Produit une charge utile binaire indépendante contenant modèle, compteur logique et RAM.</summary>
    /// <remarks>La séquence de publication n’est pas sauvegardée : elle doit rester monotone après restauration.
    /// Les lecteurs et options sont contrôlés par l’enveloppe Common et la configuration de la machine.</remarks>
    public byte[] SaveState()
    {
        EnsureReady();
        using var stream = new MemoryStream(SimulationConstants.StateHeaderLength + _memory.Length);
        using var writer = new BinaryWriter(stream);
        writer.Write(SimulationConstants.StateSignature);
        writer.Write(SimulationConstants.StateVersion);
        writer.Write((int)_configuration!.Model);
        writer.Write(_ticks);
        writer.Write(_memory.Length);
        writer.Write(_memory);
        writer.Flush();
        return stream.ToArray();
    }

    /// <summary>Valide toute la charge utile avant de modifier la RAM épinglée et le compteur.</summary>
    /// <remarks>Une version, un modèle, une taille ou un compteur invalides sont rejetés. La RAM est copiée
    /// dans le tableau existant afin que les pointeurs fournis par les exports restent valides.</remarks>
    public void LoadState(ReadOnlySpan<byte> state)
    {
        EnsureReady();
        if (state.Length != SimulationConstants.StateHeaderLength + _memory.Length) throw new InvalidDataException(nameof(state));
        using var stream = new MemoryStream(state.ToArray(), writable: false);
        using var reader = new BinaryReader(stream);
        if (reader.ReadInt32() != SimulationConstants.StateSignature || reader.ReadInt32() != SimulationConstants.StateVersion || reader.ReadInt32() != (int)_configuration!.Model) throw new InvalidDataException(nameof(state));
        var ticks = reader.ReadInt64();
        if (ticks < 0 || reader.ReadInt32() != _memory.Length) throw new InvalidDataException(nameof(state));
        state[SimulationConstants.StateHeaderLength..].CopyTo(_memory);
        _ticks = ticks;
        ShutdownRequested = false;
        ClearOutputs();
    }

    /// <summary>Copie l’état dans la mémoire empruntée par l’appelant ABI après vérification de sa taille.</summary>
    private bool SerializeNative(nint data, nuint size)
    {
        var state = SaveState();
        if (data == 0 || size < (nuint)state.Length) return false;
        Marshal.Copy(state, 0, data, state.Length);
        return true;
    }
    /// <summary>Copie puis valide l’état ABI ; un document rejeté laisse la machine inchangée.</summary>
    private bool UnserializeNative(nint data, nuint size)
    {
        if (data == 0 || size != (nuint)(SimulationConstants.StateHeaderLength + _memory.Length)) return false;
        var bytes = new byte[(int)size];
        Marshal.Copy(data, bytes, 0, bytes.Length);
        try { LoadState(bytes); return true; }
        catch (InvalidDataException) { return false; }
    }

    /// <summary>Applique une option reconnue après validation ; les changements de géométrie exigent une recréation.</summary>
    public void SetOption(string key, string value)
    {
        EnsureReady();
        var option = OptionCatalog.All.SingleOrDefault(item => item.Key == key) ?? throw new ArgumentException(nameof(key));
        if (!option.Values.Any(item => item.Value == value)) throw new ArgumentException(nameof(value));
        if (option.RequiresRestart && _configuration!.Options.GetValueOrDefault(key, option.DefaultValue) != value) throw new InvalidOperationException(nameof(option.RequiresRestart));
        var options = new Dictionary<string, string>(_configuration!.Options, StringComparer.Ordinal) { [key] = value };
        _configuration = _configuration with { Options = new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(options) };
    }
}

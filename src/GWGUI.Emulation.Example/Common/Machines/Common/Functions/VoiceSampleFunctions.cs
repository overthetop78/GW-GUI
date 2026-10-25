using System.IO;
using System.IO.Compression;
using System.Buffers.Binary;
using System.Text.RegularExpressions;

namespace GWGUI.Emulation.Example.Common.Machines.Common.Functions;

/// <summary>Validation et préparation de la banque facultative d’échantillons WAV.</summary>
/// <remarks>Le contrôle du format reprend le lecteur de banque existant. Les noms de répertoires ZIP
/// ne sont jamais des destinations : chaque entrée retenue rejoint le seul sous-dossier de la session.</remarks>
internal static class VoiceSampleFunctions
{
    /// <summary>Convention de noms choisie par le module, appliquée sans distinction de casse.</summary>
    private static readonly Regex FileNames = new(VoiceSampleConstants.FileNamePattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>Valide toute la banque avant de retourner ses entrées empruntées.</summary>
    /// <param name="archive">Archive conservée ouverte par l’appelant, propriétaire de ses entrées.</param>
    /// <returns>Entrées WAV dans leur ordre d’origine, utilisables tant que l’archive reste ouverte.</returns>
    /// <exception cref="InvalidDataException">Aucun échantillon, nom inutilisable ou dupliqué, ou format WAV invalide.</exception>
    /// <remarks>Les flux de lecture des en-têtes sont fermés avant le retour. L’archive reçue reste ouverte,
    /// y compris après rejet. Les fichiers annexes ne correspondant pas à la convention sont ignorés.</remarks>
    internal static IReadOnlyList<ZipArchiveEntry> Entries(ZipArchive archive)
    {
        var entries = archive.Entries.Where(entry => FileNames.IsMatch(entry.Name)).ToArray();
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (entries.Length == 0) throw new InvalidDataException();
        foreach (var entry in entries)
            if (entry.Name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || !names.Add(entry.Name) || !ValidWave(entry)) throw new InvalidDataException();
        return entries;
    }

    /// <summary>Contrôle le contenu d’un fichier utilisateur sans le modifier ni conserver son archive ouverte.</summary>
    /// <param name="path">Fichier emprunté, dont l’extension n’impose pas l’identité du contenu.</param>
    /// <returns>True uniquement pour une archive contenant une banque entièrement validée.</returns>
    internal static bool IsCompatible(string path)
    {
        try
        {
            using var archive = ZipFile.OpenRead(path);
            _ = Entries(archive);
            return true;
        }
        catch (Exception error) when (error is InvalidDataException or IOException or UnauthorizedAccessException) { return false; }
    }

    /// <summary>Prépare les copies de la banque dans le dossier système possédé par la session.</summary>
    /// <param name="path">Archive source empruntée, conservée intacte.</param>
    /// <param name="systemDirectory">Répertoire de la session, dont l’appelant conserve le nettoyage final.</param>
    /// <remarks>Toute la banque est validée avant la première copie. Seuls les noms simples sont retenus,
    /// en majuscules comme dans la préparation existante. Chaque flux possédé est fermé par son using,
    /// même en cas d’erreur ; les copies déjà produites appartiennent toujours à la session appelante.</remarks>
    internal static void Prepare(string path, string systemDirectory)
    {
        using var archive = ZipFile.OpenRead(path);
        var entries = Entries(archive);
        var directory = Path.Combine(systemDirectory, VoiceSampleConstants.DirectoryName);
        Directory.CreateDirectory(directory);
        foreach (var entry in entries)
        {
            var destination = Path.Combine(directory, entry.Name.ToUpperInvariant());
            using var source = entry.Open();
            using var output = File.Create(destination);
            source.CopyTo(output);
        }
    }

    /// <summary>Contrôle l’en-tête WAV simple, l’alignement des trames et la présence des octets annoncés.</summary>
    /// <param name="entry">Entrée empruntée ; son flux de lecture est possédé uniquement pendant ce contrôle.</param>
    /// <returns>True pour le PCM entier 8/16 bits ou flottant 32 bits, mono ou stéréo, à fréquence positive.</returns>
    /// <remarks>La fréquence est exprimée en hertz ; les positions et longueurs de la structure sont en octets.
    /// Ce lecteur conserve les variantes de l’implémentation d’origine, sans prétendre décoder tous les WAV.</remarks>
    private static bool ValidWave(ZipArchiveEntry entry)
    {
        if (entry.Length < VoiceSampleConstants.HeaderBytes) return false;
        Span<byte> header = stackalloc byte[VoiceSampleConstants.HeaderBytes];
        using var stream = entry.Open();
        stream.ReadExactly(header);
        var format = BinaryPrimitives.ReadUInt16LittleEndian(header[VoiceSampleConstants.FormatTagOffset..]);
        var bits = BinaryPrimitives.ReadUInt16LittleEndian(header[VoiceSampleConstants.BitsOffset..]);
        var channels = BinaryPrimitives.ReadUInt16LittleEndian(header[VoiceSampleConstants.ChannelsOffset..]);
        var dataSize = BinaryPrimitives.ReadUInt32LittleEndian(header[VoiceSampleConstants.DataSizeOffset..]);
        return header.StartsWith(VoiceSampleConstants.RiffMagic)
            && header[VoiceSampleConstants.WaveOffset..].StartsWith(VoiceSampleConstants.WaveMagic)
            && header[VoiceSampleConstants.FormatOffset..].StartsWith(VoiceSampleConstants.FormatMagic)
            && BinaryPrimitives.ReadUInt32LittleEndian(header[VoiceSampleConstants.FormatSizeOffset..]) == VoiceSampleConstants.PlainFormatBytes
            && header[VoiceSampleConstants.DataOffset..].StartsWith(VoiceSampleConstants.DataMagic)
            && channels is 1 or 2
            && BinaryPrimitives.ReadUInt32LittleEndian(header[VoiceSampleConstants.SampleRateOffset..]) > 0
            && (format == VoiceSampleConstants.IntegerPcm ? bits is VoiceSampleConstants.BytePcmBits or VoiceSampleConstants.WordPcmBits : format == VoiceSampleConstants.FloatPcm && bits == VoiceSampleConstants.FloatPcmBits)
            && dataSize > 0 && dataSize <= entry.Length - VoiceSampleConstants.HeaderBytes
            && dataSize % (channels * (bits / VoiceSampleConstants.BytePcmBits)) == 0;
    }
}

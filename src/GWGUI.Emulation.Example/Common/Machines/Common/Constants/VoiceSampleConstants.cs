namespace GWGUI.Emulation.Example.Common.Machines.Common.Constants;

/// <summary>Structure du fichier WAV et choix de présentation de la banque facultative d’échantillons.</summary>
/// <remarks>Les positions et formats reprennent le lecteur de banque existant. Le mock accepte des noms WAV
/// génériques ; un module peut choisir ici la convention de noms attendue par son cœur.</remarks>
internal static class VoiceSampleConstants
{
    /// <summary>Sous-dossier relatif au répertoire système possédé par la session.</summary>
    internal const string DirectoryName = "voice";
    /// <summary>Expression appliquée au nom seul de chaque entrée, sans ses répertoires ZIP.</summary>
    internal const string FileNamePattern = "^.+\\.WAV$";
    /// <summary>Extension de l’archive proposée comme source de banque.</summary>
    internal const string ArchiveExtension = ".zip";
    /// <summary>Clé du libellé commun traduit par l’application hôte.</summary>
    internal const string LabelResourceKey = "Emulation.Firmware.VoiceSamples";
    /// <summary>Taille en octets de l’en-tête WAV simple attendu par le lecteur existant.</summary>
    internal const int HeaderBytes = 44;
    /// <summary>Position en octets de la signature du conteneur audio.</summary>
    internal const int WaveOffset = 8;
    /// <summary>Position en octets de la signature du bloc de format.</summary>
    internal const int FormatOffset = 12;
    /// <summary>Position en octets de la longueur du bloc de format.</summary>
    internal const int FormatSizeOffset = 16;
    /// <summary>Position en octets de l’identifiant d’encodage PCM.</summary>
    internal const int FormatTagOffset = 20;
    /// <summary>Position en octets du nombre de canaux.</summary>
    internal const int ChannelsOffset = 22;
    /// <summary>Position en octets de la fréquence d’échantillonnage exprimée en hertz.</summary>
    internal const int SampleRateOffset = 24;
    /// <summary>Position en octets de la résolution exprimée en bits par échantillon.</summary>
    internal const int BitsOffset = 34;
    /// <summary>Position en octets de la signature du bloc d’échantillons.</summary>
    internal const int DataOffset = 36;
    /// <summary>Position en octets de la longueur du bloc d’échantillons.</summary>
    internal const int DataSizeOffset = 40;
    /// <summary>Longueur en octets du bloc de format PCM simple.</summary>
    internal const int PlainFormatBytes = 16;
    /// <summary>Identifiant WAV du PCM entier.</summary>
    internal const int IntegerPcm = 1;
    /// <summary>Identifiant WAV du PCM flottant.</summary>
    internal const int FloatPcm = 3;
    /// <summary>Résolution en bits du PCM entier sur un octet.</summary>
    internal const int BytePcmBits = 8;
    /// <summary>Résolution en bits du PCM entier sur deux octets.</summary>
    internal const int WordPcmBits = 16;
    /// <summary>Résolution en bits du PCM flottant sur quatre octets.</summary>
    internal const int FloatPcmBits = 32;
    /// <summary>Signature du conteneur RIFF.</summary>
    internal static readonly byte[] RiffMagic = "RIFF"u8.ToArray();
    /// <summary>Signature du contenu WAVE.</summary>
    internal static readonly byte[] WaveMagic = "WAVE"u8.ToArray();
    /// <summary>Signature du bloc décrivant le format audio.</summary>
    internal static readonly byte[] FormatMagic = "fmt "u8.ToArray();
    /// <summary>Signature du bloc contenant les échantillons.</summary>
    internal static readonly byte[] DataMagic = "data"u8.ToArray();
}

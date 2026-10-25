namespace GWGUI.Emulation.Example.Common.Constants;

/// <summary>Taille du tampon utilisé pour transférer les contenus.</summary>
/// <remarks>La taille est exprimée en octets. Les flux restent sous la responsabilité de leur propriétaire ;
/// cette constante fournit le dimensionnement du tampon aux opérations de transfert du module.</remarks>
internal static class BufferConstants
{
    /// <summary>Taille en octets du tampon utilisé par les transferts de fichiers : 81 920 octets.</summary>
    /// <remarks>Cette taille décrit une copie de flux ; elle est indépendante des capacités des tampons audio ou vidéo.</remarks>
    internal const int FileTransferBufferSize = 81920;
}


namespace GWGUI.MediaEngine.Containers.Scp;

/// <summary>Fournit la validation commune de la signature d'un conteneur SuperCard Pro.</summary>
internal static class ScpSignature
{
    /// <summary>Indique si le contenu est assez long et commence par la signature SCP attendue.</summary>
    /// <param name="data">Contenu à examiner depuis son premier octet.</param>
    /// <returns><see langword="true"/> lorsque la signature SCP complète est présente.</returns>
    public static bool IsPresent(ReadOnlySpan<byte> data) => data.Length >= ScpFormatConstants.SignatureLength && data[..ScpFormatConstants.SignatureLength].SequenceEqual(ScpFormatConstants.FileSignature);
}

using System.Buffers.Binary;

namespace GWGUI.MediaEngine.Containers.Scp;

/// <summary>Valide un descripteur de révolution SCP et décode ses mots de flux.</summary>
internal static class ScpRevolutionReader
{
    /// <summary>Lit une révolution décrite dans l'en-tête d'une piste SCP.</summary>
    /// <param name="data">Octets complets du conteneur SCP.</param>
    /// <param name="trackOffset">Position de la piste depuis le début du conteneur, en octets.</param>
    /// <param name="descriptorOffset">Position du descripteur depuis le début de la piste, en octets.</param>
    /// <param name="trackNumber">Numéro de piste utilisé dans les erreurs de validation.</param>
    /// <param name="revolutionIndex">Index de révolution basé sur zéro.</param>
    /// <returns>Révolution contenant les intervalles de flux décodés en pas temporels SCP.</returns>
    /// <exception cref="InvalidDataException">Les données de flux annoncées sont incomplètes ou hors limites.</exception>
    /// <exception cref="OverflowException">Une position, une taille ou un intervalle de flux dépasse la plage numérique prise en charge.</exception>
    public static ScpRevolution Read(ReadOnlySpan<byte> data, int trackOffset, int descriptorOffset, int trackNumber, int revolutionIndex)
    {
        var trackData = data[trackOffset..];
        var indexTime = BinaryPrimitives.ReadUInt32LittleEndian(trackData.Slice(descriptorOffset + ScpFormatConstants.RevolutionIndexTimeOffset, sizeof(uint)));
        var fluxCount = BinaryPrimitives.ReadUInt32LittleEndian(trackData.Slice(descriptorOffset + ScpFormatConstants.RevolutionFluxCountOffset, sizeof(uint)));
        var relativeOffset = BinaryPrimitives.ReadUInt32LittleEndian(trackData.Slice(descriptorOffset + ScpFormatConstants.RevolutionDataOffset, sizeof(uint)));
        var fluxOffset = checked(trackOffset + (int)relativeOffset);
        var byteCount = checked((int)fluxCount * ScpFormatConstants.FluxIntervalSize);
        ScpDataValidator.Require(data, fluxOffset, byteCount, ScpSection.RevolutionFlux, trackNumber, revolutionIndex + ScpFormatConstants.RevolutionNumberOffset);
        var fluxBytes = data.Slice(fluxOffset, byteCount);
        var intervals = new List<uint>((int)Math.Min(fluxCount, (uint)int.MaxValue));
        uint overflow = 0;
        for (var position = 0; position < fluxBytes.Length; position += ScpFormatConstants.FluxIntervalSize)
        {
            var value = BinaryPrimitives.ReadUInt16BigEndian(fluxBytes.Slice(position, ScpFormatConstants.FluxIntervalSize));
            if (value == ScpFormatConstants.FluxOverflowMarker)
            {
                overflow = checked(overflow + ScpFormatConstants.ZeroFluxIntervalOverflow);
                continue;
            }

            intervals.Add(checked(overflow + value));
            overflow = ScpFormatConstants.FluxOverflowMarker;
        }

        if (overflow != ScpFormatConstants.FluxOverflowMarker)
        {
            intervals.Add(overflow);
        }

        return new ScpRevolution(indexTime, fluxCount, intervals);
    }
}

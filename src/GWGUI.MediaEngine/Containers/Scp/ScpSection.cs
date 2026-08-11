namespace GWGUI.MediaEngine.Containers.Scp;

/// <summary>Identifie les sections SCP dont les limites sont validées pendant la lecture.</summary>
public enum ScpSection
{
    /// <summary>En-tête fixe du conteneur SCP.</summary>
    Header,

    /// <summary>Table contenant les positions des pistes.</summary>
    TrackOffsetTable,

    /// <summary>En-tête et descripteurs d'une piste.</summary>
    TrackHeader,

    /// <summary>Données de flux d'une révolution.</summary>
    RevolutionFlux
}

namespace GWGUI.Emulation.Contracts;

/// <summary>État du lecteur : nombre d’images, index sélectionné, éjection et description des images.</summary>
public sealed record DiskStatus(
    int ImageCount,
    int CurrentIndex,
    bool IsEjected,
    IReadOnlyList<DiskImageStatus> Images);

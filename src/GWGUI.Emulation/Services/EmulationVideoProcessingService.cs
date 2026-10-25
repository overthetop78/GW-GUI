using GWGUI.Emulation.Interop;

namespace GWGUI.Emulation.Services;

/// <summary>Relie le contrat vidéo aux algorithmes existants, sans copie ni allocation supplémentaire.</summary>
internal sealed class EmulationVideoProcessingService : IEmulationVideoProcessing
{
    public void ApplyGeometry(ExternalCoreApi.Geometry geometry, ref float aspectRatio) => EmulationVideoFunctions.ApplyGeometry(geometry, ref aspectRatio);
    public int FrameLength(uint height, nuint pitch) => EmulationVideoFunctions.FrameLength(height, pitch);
    public void CopyRows(nint source, byte[] destination, int height, int pitch) => EmulationVideoFunctions.CopyRows(source, destination, height, pitch);
    public byte[] CopyFramePixels(nint data, uint height, nuint pitch) => EmulationVideoFunctions.CopyFramePixels(data, height, pitch);
    public TimeSpan Timestamp(long startTimestamp) => EmulationVideoFunctions.Timestamp(startTimestamp);
    public string FormatStandard(EmulationVideoStandard standard) => EmulationVideoFunctions.FormatStandard(standard);
    public string FormatFrequency(double frequencyHz) => EmulationVideoFunctions.FormatFrequency(frequencyHz);
    public string FormatTiming(EmulationVideoStandard standard, double frequencyHz) => EmulationVideoFunctions.FormatTiming(standard, frequencyHz);
}

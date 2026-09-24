namespace GWGUI.MediaEngine.Constants;

/// <summary>Invariant stored and user-data dimensions for supported optical sector modes.</summary>
public static class OpticalSectorConstants
{
    public const int Data2048Size = 2048;
    public const int Mode2Data2336Size = 2336;
    public const int Mode2Form1DataSize = 2048;
    public const int Mode2Form2DataSize = 2324;
    public const int RawSectorSize = 2352;
    public const int Mode1RawUserDataOffset = 16;
    public const int Mode1RawUserDataLength = 2048;
    public const int Mode2RawUserDataOffset = 16;
    public const int Mode2RawUserDataLength = 2336;
    public const int AudioSectorSize = 2352;
    public const int SubchannelSize = 96;
}

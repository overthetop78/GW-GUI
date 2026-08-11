namespace GWGUI.MediaEngine.Decoding;

/// <summary>Identifie la nature d'une structure repérée dans un flux décodé.</summary>
public enum FluxStructureKind { Sync, IdAddressMark, DataAddressMark, DeletedDataAddressMark, AmigaSync, AppleAddress, AppleData, CommodoreSync, CommodoreHeader, FormatHeader, FormatData, TimingAnomaly }

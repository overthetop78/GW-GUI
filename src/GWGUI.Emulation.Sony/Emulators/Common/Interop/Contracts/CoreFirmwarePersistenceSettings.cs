namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;

internal sealed record CoreFirmwarePersistenceSettings(string IdentityFieldId, string DirectoryName, IReadOnlyList<CoreFirmwareDataFile> Files);

internal sealed record CoreFirmwareDataFile(string FieldId, string FileName, int Size);

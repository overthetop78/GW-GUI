using GWGUI.Domain.Settings.Hardware;
namespace GWGUI.App.Contracts.Services.Hardware;

public sealed record HardwareChoice(DriveSettings Drive, string Port, bool Available, string Label);

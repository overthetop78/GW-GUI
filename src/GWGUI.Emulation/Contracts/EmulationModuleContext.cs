namespace GWGUI.Emulation.Contracts;

public sealed record EmulationModuleContext(
    string DataDirectory,
    string ModuleDirectory,
    HttpClient HttpClient);

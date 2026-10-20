using System.Net.Http;

namespace GWGUI.Emulation.Contracts;

internal sealed record EmulatorManagementContext(
    HttpClient HttpClient,
    string CoreDirectory);

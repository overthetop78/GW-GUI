using GWGUI.Emulation;
using GWGUI.Emulation.Enums;

namespace GWGUI.App.Contracts.Emulation.Controllers;

internal sealed record EmulationControllerCategoryChoiceView(EmulationControllerCategory Category, string DisplayName, IReadOnlyList<EmulationControllerChoice> Choices);

using System.Text.RegularExpressions;

namespace GWGUI.App.Contracts.Localization;

internal sealed record ExplorerWarningPattern(string Key, Regex Regex);

using GWGUI.Infrastructure.Settings;
using GWGUI.App.Constants.Theming;

namespace GWGUI.App.Contracts.Theming;

internal sealed record ThemeDefinition(AppThemeDesign Id, string NameResourceKey, string DescriptionResourceKey,
    IReadOnlyList<AppTheme> Variants, AppTheme DefaultVariant)
{
    internal AppTheme Normalize(AppTheme variant) => Variants.Contains(variant) ? variant : DefaultVariant;
    internal string PaletteUri(bool dark) => ThemeConstants.PackPrefix + Id + (dark ? ThemeConstants.DarkPaletteSuffix : ThemeConstants.LightPaletteSuffix);
    internal string PreviewUri(AppTheme variant) => ThemeConstants.PackPrefix + Id + (variant == AppTheme.Dark ? ThemeConstants.DarkPreviewSuffix : ThemeConstants.LightPreviewSuffix);
}

using GWGUI.Tests.Application.TestInfrastructure;
using GWGUI.App.Dictionaries.Localization;

namespace GWGUI.Tests.Interface.Navigation;

[Collection("WPF")]
public sealed class NavigationTests(StaExecutionScenarios sta)
{
    [Theory]
    [InlineData(0, "ReadTabSection")]
    [InlineData(1, "WriteTabSection")]
    [InlineData(2, "ConversionTabSection")]
    [InlineData(3, "VisualizerTabSection")]
    [InlineData(4, "ExplorerSection")]
    [InlineData(5, "ToolsTabSection")]
    [InlineData(6, "EmulationSection")]
    public Task Switching_tabs_displays_the_expected_section_and_preserves_state(int index, string expectedType) =>
        sta.Run(() => NavigationScenarios.TabRoundTrip(index, expectedType));

    [Theory]
    [InlineData("preferences")]
    [InlineData("updates")]
    [InlineData("emulation")]
    [InlineData("history")]
    [InlineData("about")]
    public Task Main_window_routes_dialog_requests_once(string request) =>
        sta.Run(() => NavigationScenarios.ShellDialogRequest(request));

    [Theory]
    [InlineData("preferences", true)]
    [InlineData("preferences", false)]
    [InlineData("preferences", null)]
    [InlineData("updates", null)]
    [InlineData("emulation", null)]
    [InlineData("emulation-module", null)]
    [InlineData("history", null)]
    [InlineData("about", null)]
    [InlineData("tool", null)]
    public Task Dialog_navigation_preserves_arguments_owner_and_existing_return_contract(string request, bool? result) =>
        sta.Run(() => NavigationScenarios.DialogOwnerAndArguments(request, result));

    public static IEnumerable<object[]> LayoutCases()
    {
        for (var index = 0; index < 7; index++)
        {
            yield return [index, 1280d, 720d];
            yield return [index, 1360d, 820d];
        }
    }

    [Theory]
    [MemberData(nameof(LayoutCases))]
    public Task Shell_sections_and_primary_commands_fit_minimum_and_normal_content_areas(int index, double width, double height) =>
        sta.Run(() => WindowLayoutScenarios.ShellLayout(index, width, height));

    public static IEnumerable<object[]> MenuCultures() =>
        UiLanguageCatalog.Available.Select(language => new object[] { language.Code });

    [Theory]
    [MemberData(nameof(MenuCultures))]
    public Task Menu_labels_resolve_from_bindings_in_each_supported_language(string culture) =>
        sta.Run(() => ControlContractScenarios.MenuLabels(culture));

    [Theory]
    [InlineData(0, "ReadExecuteButton")]
    [InlineData(1, "WriteExecuteButton")]
    [InlineData(2, "ConvertExecuteButton")]
    public Task Primary_controls_expose_accessible_commands_and_forward_activation_once(int index, string automationId) =>
        sta.Run(() => ControlContractScenarios.PrimaryCommand(index, automationId));

    [Fact]
    public Task Path_section_binds_location_and_accessible_name_and_exposes_browse() =>
        sta.Run(ControlContractScenarios.PathControls);

    [Fact]
    public Task Updates_footer_replaces_close_while_a_downloaded_module_waits_for_installation() =>
        sta.Run(ControlContractScenarios.UpdatesFooterReflectsPendingModuleInstallation);

    [Theory]
    [InlineData("preferences")]
    [InlineData("updates")]
    [InlineData("emulation")]
    [InlineData("emulation-module")]
    [InlineData("history")]
    [InlineData("about")]
    [InlineData("tool")]
    public Task Dialog_presentation_failure_propagates_without_retry(string request) =>
        sta.Run(() => NavigationScenarios.DialogOwnerAndArguments(request, null, fail: true));

    [Theory]
    [InlineData("preferences")]
    [InlineData("updates")]
    [InlineData("emulation")]
    [InlineData("history")]
    [InlineData("documentation")]
    [InlineData("about")]
    [InlineData("info")]
    [InlineData("bandwidth")]
    [InlineData("rpm")]
    [InlineData("seek")]
    [InlineData("align")]
    [InlineData("pin")]
    [InlineData("reset")]
    [InlineData("delays")]
    [InlineData("update")]
    public Task Menu_dispatches_exactly_the_requested_action_once(string request) =>
        sta.Run(() => NavigationScenarios.MenuRequest(request));

    [Fact]
    public Task Tool_without_verb_does_not_dispatch() => sta.Run(NavigationScenarios.MissingToolVerbDoesNotDispatch);

    [Theory]
    [InlineData(1280, 720)]
    [InlineData(1360d, 820d)]
    public Task Top_level_menu_commands_fit_the_available_area(double width, double height) =>
        sta.Run(() => WindowLayoutScenarios.MenuFits(width, height));

    [Theory]
    [InlineData(1360d, 820d, -1800d, 100d, 1360d, 820d, -1800d, 100d)]
    [InlineData(100d, 100d, 0d, 0d, 1280d, 720d, 0d, 0d)]
    [InlineData(5000d, 2000d, 0d, 0d, 3840d, 1080d, -1920d, 0d)]
    [InlineData(1360d, 820d, 1800d, 900d, 1360d, 820d, 560d, 260d)]
    [InlineData(1360d, 820d, 6000d, 0d, 1360d, 820d, null, null)]
    [InlineData(1360d, 820d, null, null, 1360d, 820d, null, null)]
    [InlineData(1360d, 820d, double.NaN, 0d, 1360d, 820d, null, null)]
    [InlineData(1360d, 820d, 0d, double.PositiveInfinity, 1360d, 820d, null, null)]
    public void Restore_handles_saved_sizes_and_disconnected_monitors(double width, double height,
        double? left, double? top, double expectedWidth, double expectedHeight, double? expectedLeft, double? expectedTop) =>
        WindowLayoutScenarios.Restore(width, height, left, top, expectedWidth, expectedHeight, expectedLeft, expectedTop);

    [Theory]
    [InlineData(-1800d, 100d, -1800d, 100d)]
    [InlineData(200d, 900d, -1360d, 260d)]
    [InlineData(-3000d, -200d, -1920d, 40d)]
    [InlineData(null, null, -1920d, 40d)]
    public void Work_area_keeps_the_window_on_screen_and_clear_of_taskbar(double? left, double? top,
        double expectedLeft, double expectedTop) => WindowLayoutScenarios.WorkArea(left, top, expectedLeft, expectedTop);

    [Fact]
    public void Small_screen_limits_the_window_to_available_space() => WindowLayoutScenarios.SmallScreen();

    [Theory]
    [InlineData(0, 0, 0, 1080)]
    [InlineData(0, 0, 1920, -1)]
    [InlineData(double.NaN, 0, 1920, 1080)]
    [InlineData(0, double.PositiveInfinity, 1920, 1080)]
    [InlineData(0, 0, double.PositiveInfinity, 1080)]
    [InlineData(0, 0, 1920, double.NaN)]
    public void Invalid_work_area_preserves_the_existing_placement(double left, double top, double width, double height) =>
        WindowLayoutScenarios.InvalidWorkArea(left, top, width, height);
}

using GWGUI.App.Localization;
using GWGUI.Emulation.Atari;

namespace GWGUI.App.Controls;

internal static class AtariErrorLocalizationFunctions
{
    internal static string Describe(Exception error)
    {
        if (error is not AtariEmulationException atariError)
            return LocExtension.Get(AtariErrorLocalizationConstants.UnexpectedResource);

        var description = LocExtension.Get(Resource(atariError.Code));
        var details = atariError.Context.Values.Where(value => !string.IsNullOrWhiteSpace(value)).ToArray();
        return details.Length == AtariErrorLocalizationConstants.NoDetailsCount
            ? description
            : description + Environment.NewLine + LocExtension.Get(
                AtariErrorLocalizationConstants.DetailsResource,
                string.Join(AtariErrorLocalizationConstants.DetailSeparator, details));
    }

    private static string Resource(AtariErrorCode code) => code switch
    {
        AtariErrorCode.CoreNotFound => AtariErrorLocalizationConstants.CoreNotFoundResource,
        AtariErrorCode.CoreRejected => AtariErrorLocalizationConstants.CoreRejectedResource,
        AtariErrorCode.FirmwareMissing => AtariErrorLocalizationConstants.FirmwareMissingResource,
        AtariErrorCode.FirmwareInvalid => AtariErrorLocalizationConstants.FirmwareInvalidResource,
        AtariErrorCode.ContentNotFound => AtariErrorLocalizationConstants.ContentNotFoundResource,
        AtariErrorCode.ContentUnsupported => AtariErrorLocalizationConstants.ContentUnsupportedResource,
        AtariErrorCode.OptionInvalid => AtariErrorLocalizationConstants.OptionInvalidResource,
        AtariErrorCode.HostProtocolFailure => AtariErrorLocalizationConstants.HostProtocolFailureResource,
        AtariErrorCode.StateInvalid => AtariErrorLocalizationConstants.StateInvalidResource,
        AtariErrorCode.StateIncompatible => AtariErrorLocalizationConstants.StateIncompatibleResource,
        _ => AtariErrorLocalizationConstants.UnexpectedResource
    };
}

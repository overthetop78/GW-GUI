using GWGUI.App.Localization.Extensions;
using GWGUI.MediaEngine.Exploration.Metadata;

namespace GWGUI.App.Presenters.Explorer;

public static class ExplorerMetadataPresenter
{
    public static string Systems(DiskImageMetadata metadata) => metadata.SystemIds.Count == 0 ? LocExtension.Get("Explorer.Metadata.None") : string.Join(" + ", metadata.SystemIds.Select(id => LocExtension.Get($"System.{id}")));
    public static string Protection(DiskImageMetadata metadata) => metadata.ProtectionId is null ? LocExtension.Get("Explorer.Metadata.None") : LocExtension.Get($"Format.{metadata.ProtectionId}");
}

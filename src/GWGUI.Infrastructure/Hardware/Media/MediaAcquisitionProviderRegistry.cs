using MediaAcquisitionProgress = global::GWGUI.MediaEngine.Contracts.MediaAcquisitionProgress;
using MediaAcquisitionResult = global::GWGUI.MediaEngine.Contracts.MediaAcquisitionResult;
using GWGUI.MediaEngine.Enums;
using IMediaAcquisitionProvider = global::GWGUI.MediaEngine.Interfaces.IMediaAcquisitionProvider;

namespace GWGUI.Infrastructure.Hardware.Media;

/// <summary>Selects a physical acquisition provider by media family and device identifier.</summary>
public sealed class MediaAcquisitionProviderRegistry
{
    private readonly IReadOnlyList<IMediaAcquisitionProvider> providers;

    public MediaAcquisitionProviderRegistry(IEnumerable<IMediaAcquisitionProvider> providers)
    {
        ArgumentNullException.ThrowIfNull(providers);
        var registered = providers.ToArray();
        if (registered.Any(provider => provider is null))
            throw new ArgumentException("An acquisition provider cannot be null.", nameof(providers));
        if (registered.Any(provider => string.IsNullOrWhiteSpace(provider.Id)))
            throw new ArgumentException("An acquisition provider identifier cannot be empty.", nameof(providers));
        var duplicate = registered.GroupBy(provider => provider.Id, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Skip(1).Any());
        if (duplicate is not null)
            throw new ArgumentException($"Acquisition provider identifier '{duplicate.Key}' is registered more than once.", nameof(providers));
        this.providers = Array.AsReadOnly(registered);
    }

    public IReadOnlyList<IMediaAcquisitionProvider> Providers => providers;

    public IMediaAcquisitionProvider GetRequired(MediaKind mediaKind, string deviceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);
        var compatible = providers.Where(provider =>
            provider.SupportedMediaKinds.Contains(mediaKind)
            && provider.CanAcquire(mediaKind, deviceId)).ToArray();
        return compatible.Length switch
        {
            1 => compatible[0],
            0 => throw new NotSupportedException(
                $"No physical acquisition provider accepts device '{deviceId}' for media family '{mediaKind}'."),
            _ => throw new InvalidOperationException(
                $"Multiple physical acquisition providers accept device '{deviceId}' for media family '{mediaKind}'.")
        };
    }

    public Task<MediaAcquisitionResult> AcquireAsync(
        MediaKind mediaKind,
        string deviceId,
        IReadOnlyDictionary<string, string> options,
        IProgress<MediaAcquisitionProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        return GetRequired(mediaKind, deviceId)
            .AcquireAsync(mediaKind, deviceId, options, progress, cancellationToken);
    }
}

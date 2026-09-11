namespace GWGUI.Domain.Contracts;

/// <summary>Describes the files and optional hints supplied to a media image reader.</summary>
public sealed record MediaSourceDescriptor(
    string PrimaryPath,
    IReadOnlyList<string> AssociatedPaths,
    long? KnownLength = null,
    string? RequestedFormatId = null);

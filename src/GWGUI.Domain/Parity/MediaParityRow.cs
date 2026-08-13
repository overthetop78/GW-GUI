namespace GWGUI.Domain.Parity;

public sealed record MediaParityRow(
    string FormatId,
    string SourceContainer,
    string TargetContainer,
    string Geometry,
    ParityValidationStatus Read,
    ParityValidationStatus Conversion,
    ParityValidationStatus Reopen,
    ParityValidationStatus BlocksIdentical,
    ParityValidationStatus FilesIdentical,
    ParityValidationStatus MetadataIdentical,
    ParityValidationStatus FluxIdentical,
    ParityValidationStatus PhysicalWrite,
    bool GwFallbackAvailable,
    string? EvidenceId = null)
{
    public bool IsValidatedFor(MediaParityOperation operation)
    {
        return operation switch
        {
            MediaParityOperation.Read => Read == ParityValidationStatus.Passed,
            MediaParityOperation.Conversion => HasValidatedConversion(),
            MediaParityOperation.Reopen => HasValidatedConversion() && Reopen == ParityValidationStatus.Passed,
            MediaParityOperation.PhysicalWrite => HasValidatedConversion() && PhysicalWrite == ParityValidationStatus.Passed,
            _ => false
        };
    }

    private bool HasValidatedConversion() =>
        Read == ParityValidationStatus.Passed &&
        Conversion == ParityValidationStatus.Passed &&
        Reopen == ParityValidationStatus.Passed &&
        (BlocksIdentical == ParityValidationStatus.Passed || FluxIdentical == ParityValidationStatus.Passed) &&
        FilesIdentical is ParityValidationStatus.Passed or ParityValidationStatus.NotApplicable &&
        MetadataIdentical is ParityValidationStatus.Passed or ParityValidationStatus.NotApplicable;
}

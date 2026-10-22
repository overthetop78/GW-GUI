namespace GWGUI.Emulation.Amstrad.Common.Interfaces;

/// <summary>Capacités de pointage et de présentation annoncées par un adaptateur.</summary>
internal interface IEmulatorCapabilities : IEmulatorPointerCapabilities
{
    /// <summary>Indique si le backend expose les réglages vidéo.</summary>
    bool SupportsVideoSettings { get; }
}

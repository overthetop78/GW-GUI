namespace GWGUI.Emulation.Sega.Common.Interfaces;

/// <summary>Formats de contenu annoncés par le backend sans démarrer une machine.</summary>
/// <remarks>Common utilise ce catalogue pour présenter les médias acceptés ; cette description ne monte ni ne convertit leur contenu.</remarks>
internal interface IEmulatorContentFormats
{
    /// <summary>Extensions de contenus annoncées par l’adaptateur sans démarrer une machine.</summary>
    IReadOnlyList<string> SupportedContentExtensions { get; }
}

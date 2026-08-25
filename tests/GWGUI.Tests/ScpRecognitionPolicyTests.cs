using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Exploration.Scp;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using GWGUI.MediaEngine.Recognition.Policies;
using System.IO;
using System.Runtime.CompilerServices;
using GWGUI.MediaEngine.Recognition;

namespace GWGUI.Tests;

/// <summary>VÃ©rifie la signature, les demandes explicites et l'annulation de la politique SCP.</summary>
public sealed class ScpRecognitionPolicyTests
{
    /// <summary>VÃ©rifie la signature complÃ¨te et le rejet des contenus trop courts.</summary>
    [Fact]
    public void SignatureProbeRequiresCompleteScpSignature()
    {
        Assert.True(ScpSignature.IsPresent("SCP"u8));
        Assert.False(ScpSignature.IsPresent("SC"u8));
        Assert.False(ScpSignature.IsPresent("BAD"u8));
    }

    /// <summary>VÃ©rifie qu'un contenu tronquÃ© est refusÃ© par la politique et rejetÃ© par le Reader.</summary>
    [Fact]
    public async Task TruncatedSignatureIsRejectedByPolicyAndReader()
    {
        var path = await CreateFileAsync("SC"u8.ToArray(), ".media");
        try
        {
            var policy = CreatePolicy(new HashSet<string>());
            Assert.False(await policy.CanReadAsync(new(path, null), CancellationToken.None));
            await Assert.ThrowsAsync<InvalidDataException>(() => new ScpReader().ReadAsync(path));
        }
        finally { File.Delete(path); }
    }

    /// <summary>VÃ©rifie qu'un identifiant absent du catalogue produit le diagnostic de la politique renommÃ©e avant exploration.</summary>
    [Fact]
    public async Task UnsupportedRequestedFormatIsRejectedBeforeExploration()
    {
        var path = await CreateFileAsync("SCP"u8.ToArray(), ".scp");
        try
        {
            var exception = await Assert.ThrowsAsync<NotSupportedException>(() => CreatePolicy(new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "supported" }).ReadAsync(new(path, "unsupported"), CancellationToken.None));
            Assert.Contains(nameof(ScpRecognitionPolicy), exception.Message, StringComparison.Ordinal);
        }
        finally { File.Delete(path); }
    }

    /// <summary>VÃ©rifie que l'annulation de la lecture du contexte est propagÃ©e.</summary>
    [Fact]
    public async Task ContextReadCancellationIsPropagated()
    {
        var path = await CreateFileAsync("SCP"u8.ToArray(), ".scp");
        try
        {
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await CreatePolicy(new HashSet<string>()).CanReadAsync(new(path, null), cancellation.Token));
        }
        finally { File.Delete(path); }
    }

    /// <summary>CrÃ©e une politique dont le service ne sera pas appelÃ© par les scÃ©narios de prÃ©sÃ©lection et de rejet testÃ©s.</summary>
    private static ScpRecognitionPolicy CreatePolicy(IReadOnlySet<string> supportedFormatIds)
    {
        var service = (ScpImageExplorationService)RuntimeHelpers.GetUninitializedObject(typeof(ScpImageExplorationService));
        return new(service, supportedFormatIds);
    }

    /// <summary>CrÃ©e un fichier temporaire contenant les octets fournis.</summary>
    private static async Task<string> CreateFileAsync(byte[] bytes, string extension)
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-scp-policy-{Guid.NewGuid():N}{extension}");
        await File.WriteAllBytesAsync(path, bytes);
        return path;
    }
}

using System.Reflection;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Exploration;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.Images.ScpDetection;
using GWGUI.MediaEngine.Recognition;

namespace GWGUI.Tests;

/// <summary>Vérifie la composition et le partage des services du moteur par défaut.</summary>
public sealed class MediaEngineCompositionTests
{
    /// <summary>Vérifie la présence et l'ordre des politiques de reconnaissance composées.</summary>
    [Fact]
    public void DefaultExplorerContainsTheExpectedOrderedRecognitionPolicies()
    {
        var explorer = DiskImageExplorer.CreateDefault();
        var recognition = Field<DiskImageRecognitionRegistry>(explorer);
        var policies = Field<IReadOnlyList<IDiskImageRecognitionPolicy>>(recognition);
        Assert.Equal(20, policies.Count);
        Assert.Equal("CoherentImageRecognitionPolicy", policies[2].GetType().Name);
        Assert.Equal("DecRx02ImageRecognitionPolicy", policies[3].GetType().Name);
        Assert.Equal("AppleImageRecognitionPolicy", policies[10].GetType().Name);
        Assert.Equal("MsxImageRecognitionPolicy", policies[11].GetType().Name);
        Assert.Equal("AmstradImageRecognitionPolicy", policies[12].GetType().Name);
        Assert.Equal("RawImgRecognitionPolicy", policies[13].GetType().Name);
        Assert.Equal("ScpRecognitionPolicy", policies[^1].GetType().Name);
        Assert.Single(policies, policy => policy.GetType().Name == "ScpRecognitionPolicy");
    }

    /// <summary>Vérifie que l'exploration générale et SCP partagent les mêmes registres et interprétations.</summary>
    [Fact]
    public void DefaultExplorerSharesCompositionInstances()
    {
        var explorer = DiskImageExplorer.CreateDefault();
        var fileSystems = Field<FileSystemRegistry>(explorer);
        var interpretations = Field<DiskImageInterpretationService>(explorer);
        var scpExploration = Field<ScpImageExplorationService>(explorer);
        var automatic = Field<ScpAutomaticImageExplorer>(scpExploration);
        Assert.Same(fileSystems, Field<FileSystemRegistry>(automatic));
        Assert.Same(interpretations, Field<DiskImageInterpretationService>(automatic));
        var familyProbe = Field<ScpFamilyProbe>(automatic);
        Assert.NotEmpty(Field<FluxDecoderRegistry>(familyProbe).Decoders);
        Assert.NotEmpty(fileSystems.Readers);
    }

    /// <summary>Retourne l'unique champ d'un type donné dans l'objet.</summary>
    private static T Field<T>(object instance) where T : class => (T)instance.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Single(field => typeof(T).IsAssignableFrom(field.FieldType)).GetValue(instance)!;
}

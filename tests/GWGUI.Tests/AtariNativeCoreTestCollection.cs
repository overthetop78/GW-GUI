namespace GWGUI.Tests;

internal static class AtariNativeCoreTestConstants
{
    internal const string CollectionName = "Atari native core isolation";
}

[CollectionDefinition(AtariNativeCoreTestConstants.CollectionName, DisableParallelization = true)]
public sealed class AtariNativeCoreTestCollection;

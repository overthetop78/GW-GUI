namespace GWGUI.Emulation.Sony.Emulators.PokketStation.Constants;

internal static class LinkConstants
{
    internal const uint SubsystemId = 0x101;
    internal const uint UnitAFlashId = 0x100;
    internal const uint UnitBFlashId = 0x200;
    internal const int UnitCount = 2;
    internal const string LayoutKey = "pokket_link_layout";
    internal const string AudioKey = "pokket_link_audio";
    internal const string IdentityKey = "pokket_link_unit_b_id";
    internal const string SideBySide = "Side by side";
    internal const string Stacked = "Stacked";
    internal const string BothUnits = "Both units";
    internal const string UnitA = "Unit A";
    internal const string UnitB = "Unit B";
    internal const string SplitAudio = "Unit A left, unit B right";
    internal const string DifferentIdentity = "Different from unit A";
    internal const string SameIdentity = "Same as unit A";
    internal const string Group = "pokketLink";
    internal const string GroupLabel = "Emulation.Option.pokket.Link";
    internal const string Help = "Emulation.Option.pokket.Help";
    internal const string LayoutLabel = "Emulation.Option.pokket.Layout";
    internal const string AudioLabel = "Emulation.Option.pokket.Audio";
    internal const string IdentityLabel = "Emulation.Option.pokket.Identity";
    internal const string HorizontalLabel = "Emulation.Option.pokket.Horizontal";
    internal const string VerticalLabel = "Emulation.Option.pokket.Vertical";
    internal const string SelectUnitALabel = "Emulation.Option.pokket.SelectUnitA";
    internal const string SelectUnitBLabel = "Emulation.Option.pokket.SelectUnitB";
    internal const string SecondaryCardLabel = "PocketStation B";
    internal const string SecondarySavePrefix = "unit-b-";
}

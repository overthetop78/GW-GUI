namespace GWGUI.Emulation.Amiga.Dictionaries;

public static class AmigaControllerCatalog
{
    public static IReadOnlyList<AmigaControllerType> Types(AmigaModel model)
    {
        var types = new List<AmigaControllerType>
        {
            AmigaControllerType.Joystick,
            AmigaControllerType.AnalogJoystick
        };
        if (model.SupportsCd32Controller)
            types.Add(AmigaControllerType.Cd32Pad);
        types.Add(AmigaControllerType.None);
        return types;
    }

    public static AmigaControllerType Default(AmigaModel model) =>
        model.SupportsCd32Controller ? AmigaControllerType.Cd32Pad : AmigaControllerType.Joystick;

    public static AmigaControllerType Normalize(AmigaModel model, AmigaControllerType type) =>
        type != AmigaControllerType.Automatic && Types(model).Contains(type) ? type : Default(model);

    public static IReadOnlyList<AmigaControllerType> ParallelPortTypes { get; } =
        [AmigaControllerType.Joystick, AmigaControllerType.None];
}

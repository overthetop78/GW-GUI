namespace GWGUI.Tests.Emulation;

public sealed class TypedExceptionContractTests
{
    public static IEnumerable<object[]> ModuleTypes()
    {
        yield return [typeof(GWGUI.Emulation.Acorn.Common.Exceptions.EmulationException)];
        yield return [typeof(GWGUI.Emulation.Apple.Common.Exceptions.EmulationException)];
        yield return [typeof(GWGUI.Emulation.Atari.Emulators.Common.Exceptions.EmulationException)];
        yield return [typeof(GWGUI.Emulation.Bandai.Common.Exceptions.EmulationException)];
        yield return [typeof(GWGUI.Emulation.Coleco.Common.Exceptions.EmulationException)];
        yield return [typeof(GWGUI.Emulation.Epoch.Common.Exceptions.EmulationException)];
        yield return [typeof(GWGUI.Emulation.Fairchild.Common.Exceptions.EmulationException)];
        yield return [typeof(GWGUI.Emulation.Mattel.Common.Exceptions.EmulationException)];
        yield return [typeof(GWGUI.Emulation.Philips.Common.Exceptions.EmulationException)];
        yield return [typeof(GWGUI.Emulation.Sharp.Common.Exceptions.EmulationException)];
        yield return [typeof(GWGUI.Emulation.Sinclair.Common.Exceptions.EmulationException)];
        yield return [typeof(GWGUI.Emulation.Snk.Common.Exceptions.EmulationException)];
    }

    public static IEnumerable<object[]> ContextCases()
    {
        foreach (var entry in ModuleTypes())
        foreach (var supplied in new[] { false, true })
        foreach (var localized in new[] { false, true })
            yield return [entry[0], supplied, localized];
    }

    [Theory]
    [MemberData(nameof(ContextCases))]
    public void PublicModuleExceptionKeepsTypedDataAndBorrowedContext(Type type, bool supplied, bool localized)
    {
        var constructor = Assert.Single(type.GetConstructors());
        var parameters = constructor.GetParameters();
        var category = Enum.GetValues(parameters[0].ParameterType).GetValue(0)!;
        var code = Enum.GetValues(parameters[1].ParameterType).GetValue(0)!;
        var context = new Dictionary<string, string> { ["drive"] = "D1", ["detail"] = "échec" };
        var cause = new InvalidOperationException("cause");
        var exception = Assert.IsAssignableFrom<Exception>(constructor.Invoke(
            [category, code, "message précis", supplied ? context : null, cause, localized]));

        Assert.Equal(type, exception.GetType());
        Assert.True(type.IsSealed);
        Assert.True(type.IsPublic);
        Assert.Equal("message précis", exception.Message);
        Assert.Same(cause, exception.InnerException);
        Assert.Equal(category, type.GetProperty("Category")!.GetValue(exception));
        Assert.Equal(code, type.GetProperty("Code")!.GetValue(exception));
        Assert.Equal(localized, type.GetProperty("IsLocalized")!.GetValue(exception));
        var actualContext = Assert.IsAssignableFrom<IReadOnlyDictionary<string, string>>(
            type.GetProperty("Context")!.GetValue(exception));
        if (supplied)
        {
            Assert.Same(context, actualContext);
            context["later"] = "visible";
            Assert.Equal("visible", actualContext["later"]);
        }
        else
            Assert.Empty(actualContext);

        Assert.Equal(parameters[0].ParameterType, type.GetProperty("Category")!.PropertyType);
        Assert.Equal(parameters[1].ParameterType, type.GetProperty("Code")!.PropertyType);
    }

    [Theory]
    [MemberData(nameof(ModuleTypes))]
    public void OptionalDefaultsAndEmptyContextsRemainIndependent(Type type)
    {
        var constructor = Assert.Single(type.GetConstructors());
        var parameters = constructor.GetParameters();
        Assert.Equal(6, parameters.Length);
        Assert.False(parameters[0].IsOptional);
        Assert.False(parameters[1].IsOptional);
        Assert.False(parameters[2].IsOptional);
        Assert.True(parameters[3].IsOptional);
        Assert.True(parameters[4].IsOptional);
        Assert.True(parameters[5].IsOptional);
        Assert.Null(parameters[3].DefaultValue);
        Assert.Null(parameters[4].DefaultValue);
        Assert.Equal(false, parameters[5].DefaultValue);
        var category = Activator.CreateInstance(parameters[0].ParameterType);
        var code = Activator.CreateInstance(parameters[1].ParameterType);
        var first = Assert.IsAssignableFrom<Exception>(constructor.Invoke(
            [category, code, "first", Type.Missing, Type.Missing, Type.Missing]));
        var second = Assert.IsAssignableFrom<Exception>(constructor.Invoke(
            [category, code, "second", Type.Missing, Type.Missing, Type.Missing]));
        Assert.Null(first.InnerException);
        Assert.Equal(false, type.GetProperty("IsLocalized")!.GetValue(first));
        var context = Assert.IsAssignableFrom<IDictionary<string, string>>(
            type.GetProperty("Context")!.GetValue(first));
        var other = Assert.IsAssignableFrom<IReadOnlyDictionary<string, string>>(
            type.GetProperty("Context")!.GetValue(second));
        Assert.NotSame(context, other);
        context["only-first"] = "value";
        Assert.Empty(other);
    }
}

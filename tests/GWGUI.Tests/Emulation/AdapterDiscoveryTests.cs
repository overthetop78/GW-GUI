using System.Reflection;
using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class AdapterDiscoveryTests
{
    private static readonly List<string> Events = [];
    private static readonly InvalidOperationException ConstructorFailure = new("constructor failure");
    private static Assembly FixtureAssembly => typeof(AdapterDiscoveryTests).Assembly;

    [Fact]
    public void TypedDiscoveryKeepsBothFiltersAndConstructsBeforeIdentifierSorting()
    {
        Events.Clear();
        var expectedConstructionOrder = FixtureAssembly.GetTypes()
            .Where(type => type == typeof(ZFactory) || type == typeof(AFactory))
            .Select(type => type == typeof(ZFactory) ? "ctor:Z" : "ctor:A").ToArray();
        var candidates = EmulationAdapterFunctions.Create<Factory, IIncluded>(FixtureAssembly);
        Assert.Empty(Events);
        var result = candidates.OrderBy(factory => factory.Id, StringComparer.Ordinal).ToArray();
        Assert.Equal(new[] { typeof(AFactory), typeof(ZFactory) }, result.Select(factory => factory.GetType()));
        Assert.Equal(expectedConstructionOrder, Events.Take(2));
        Assert.All(Events.Skip(2), item => Assert.StartsWith("key:", item));
        Assert.DoesNotContain("excluded", Events);
        Assert.DoesNotContain("not-factory", Events);
    }

    [Fact]
    public void PublicDiscoveryKeepsTypeNameOrderingAndOpenGenericExclusion()
    {
        Events.Clear();
        var result = EmulationAdapterFunctions.Create<Factory>(FixtureAssembly);
        Assert.Equal(new[] { typeof(AFactory), typeof(ExcludedFactory), typeof(ZFactory) },
            result.Select(factory => factory.GetType()));
        Assert.Equal(new[] { "ctor:A", "excluded", "ctor:Z" }, Events);
        Assert.Empty(EmulationAdapterFunctions.Create<OpenFactoryBase>(FixtureAssembly));
    }

    [Fact]
    public void TypedDiscoveryKeepsOriginalFailureForAnOpenGenericFactory()
    {
        Assert.Throws<ArgumentException>(() =>
            EmulationAdapterFunctions.Create<OpenFactoryBase, IOpenIncluded>(FixtureAssembly).ToArray());
    }

    [Fact]
    public void AssemblyNullKeepsEachOriginalPolicy()
    {
        var error = Assert.Throws<ArgumentNullException>(() =>
            EmulationAdapterFunctions.Create<Factory>(null!));
        Assert.Equal("assembly", error.ParamName);
        Assert.Throws<NullReferenceException>(() =>
            EmulationAdapterFunctions.Create<Factory, IIncluded>(null!));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ConstructorFailureKeepsItsReflectionWrapper(bool publicPolicy)
    {
        var error = Assert.Throws<TargetInvocationException>(() =>
        {
            if (publicPolicy)
                _ = EmulationAdapterFunctions.Create<ThrowFactoryBase>(FixtureAssembly);
            else
                _ = EmulationAdapterFunctions.Create<ThrowFactoryBase, IThrowIncluded>(FixtureAssembly).ToArray();
        });
        Assert.Same(ConstructorFailure, error.InnerException);
    }

    private interface IIncluded { }
    private abstract class Factory { public abstract string Id { get; } }

    private sealed class ZFactory : Factory, IIncluded
    {
        private ZFactory() => Events.Add("ctor:Z");
        public override string Id { get { Events.Add("key:Z"); return "z"; } }
    }

    private sealed class AFactory : Factory, IIncluded
    {
        private AFactory() => Events.Add("ctor:A");
        public override string Id { get { Events.Add("key:A"); return "a"; } }
    }

    private abstract class AbstractFactory : Factory, IIncluded { }

    private sealed class ExcludedFactory : Factory
    {
        private ExcludedFactory() => Events.Add("excluded");
        public override string Id => "excluded";
    }

    private sealed class NotAFactory : IIncluded
    {
        private NotAFactory() => Events.Add("not-factory");
    }

    private interface IOpenIncluded { }
    private abstract class OpenFactoryBase { }
    private sealed class OpenFactory<T> : OpenFactoryBase, IOpenIncluded { }

    private interface IThrowIncluded { }
    private abstract class ThrowFactoryBase { }
    private sealed class ThrowFactory : ThrowFactoryBase, IThrowIncluded
    {
        private ThrowFactory() => throw ConstructorFailure;
    }
}

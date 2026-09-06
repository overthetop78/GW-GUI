using System.Reflection;

namespace GWGUI.Tests.Application.TestInfrastructure;

// Any accidental hardware, process, persistence or network call fails immediately.
public class ControlledDependencies : DispatchProxy
{
    private Func<MethodInfo, object?[], object?>? handler;

    public static T Simulate<T>(Func<MethodInfo, object?[], object?> handler) where T : class
    {
        var proxy = Create<T, ControlledDependencies>();
        ((ControlledDependencies)(object)proxy).handler = handler;
        return proxy;
    }

    public static T Reject<T>() where T : class => Create<T, ControlledDependencies>();

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) =>
        handler is null
            ? throw new InvalidOperationException($"Unexpected external call in test: {targetMethod}")
            : handler(targetMethod!, args ?? []);
}

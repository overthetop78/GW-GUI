using System.Collections;

namespace GWGUI.Emulation.Contracts;

public sealed class EmulationControllerControls : IReadOnlyDictionary<string, float>,
    IEquatable<EmulationControllerControls>
{
    private readonly IReadOnlyDictionary<string, float> _values;
    public static EmulationControllerControls Empty { get; } = new();

    public EmulationControllerControls() :
        this(new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase)) { }

    public EmulationControllerControls(IEnumerable<KeyValuePair<string, float>> values) =>
        _values = new Dictionary<string, float>(values, StringComparer.OrdinalIgnoreCase);

    public int Count => _values.Count;
    public IEnumerable<string> Keys => _values.Keys;
    public IEnumerable<float> Values => _values.Values;
    public float this[string key] => _values[key];
    public bool ContainsKey(string key) => _values.ContainsKey(key);
    public bool TryGetValue(string key, out float value) => _values.TryGetValue(key, out value);
    public IEnumerator<KeyValuePair<string, float>> GetEnumerator() => _values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Equals(EmulationControllerControls? other) =>
        other is not null && Count == other.Count &&
        _values.All(item => other.TryGetValue(item.Key, out var value) && item.Value.Equals(value));
    public override bool Equals(object? value) => value is EmulationControllerControls other && Equals(other);
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var item in _values.OrderBy(item => item.Key, StringComparer.OrdinalIgnoreCase))
        {
            hash.Add(item.Key, StringComparer.OrdinalIgnoreCase);
            hash.Add(item.Value);
        }
        return hash.ToHashCode();
    }
}

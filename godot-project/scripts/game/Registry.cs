using System.Collections.Generic;
using System.Linq;
using Godot;

internal partial class Registry<T> : Node where T : Node {
  private readonly List<T> _allRegistered = new();
  private readonly Dictionary<string, T> _register = new();

  public void Register(string key, T value) {
    // If it's already there, error
    if (_register.ContainsKey(key)) {
      GD.PushError($"Tried to register the Key {key}, when it is already registered.");
      return;
    }

    _register.Add(key, value);
    _allRegistered.Add(value);
    AddChild(value);
  }

  public bool TryGetValue(string key, out T value) {
    return _register.TryGetValue(key, out value);
  }

  public T GetValue(string key) {
    if (TryGetValue(key, out T value)) {
      return value;
    }

    GD.PushWarning(
      $"Tried to get {key} from Registry of type {typeof(T)}, but it hasn't been registered.");
    return default;
  }

  public bool Remove(string key) {
    if (TryGetValue(key, out T value)) {
      _allRegistered.Remove(value);
    }

    return _register.Remove(key);
  }

  public bool Has(T value) {
    return _allRegistered.Contains(value);
  }

  public string GetFirstMatchingKey(T value) {
    foreach (string key in _register.Keys) {
      if (value.Equals(key)) {
        return key;
      }
    }

    return null;
  }

  public void Clear() {
    _register.Clear();
  }

  public string[] GetKeys() {
    return _register.Keys.ToArray();
  }

  public T[] GetValues() {
    return _register.Values.ToArray();
  }
}
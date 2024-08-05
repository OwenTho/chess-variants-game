using Godot;
using System.Collections.Generic;
using System.Linq;

internal partial class Registry<T> : Node where T : Node
{
    private List<T> allRegistered = new List<T>();
    private Dictionary<string, T> register = new Dictionary<string, T>();

    public void Register(string key, T value)
    {
        // If it's already there, error
        if (register.ContainsKey(key))
        {
            GD.PushError($"Tried to register the Key {key}, when it is already registered.");
            return;
        }
        register.Add(key, value);
        allRegistered.Add(value);
        AddChild(value);
    }

    public bool TryGetValue(string key, out T value)
    {
        return register.TryGetValue(key, out value);
    }

    public T GetValue(string key)
    {
        if (TryGetValue(key, out T value))
        {
            return value;
        }
        GD.PushWarning($"Tried to get {key} from Registry of type {typeof(T)}, but it hasn't been registered.");
        return default;
    }

    public bool Remove(string key)
    {
        if (TryGetValue(key, out T value))
        {
            allRegistered.Remove(value);
        }
        return register.Remove(key);
    }

    public bool Has(T value)
    {
        return allRegistered.Contains(value);
    }

    public string GetFirstMatchingKey(T value)
    {
        foreach (string key in register.Keys)
        {
            if (value.Equals(key))
            {
                return key;
            }
        }
        return null;
    }

    public void Clear()
    {
        register.Clear();
    }

    public string[] GetKeys()
    {
        return register.Keys.ToArray();
    }

    public T[] GetValues()
    {
        return register.Values.ToArray();
    }
}
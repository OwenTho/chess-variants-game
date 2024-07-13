using Godot;
using System.Collections.Generic;

internal partial class Registry<T> : GodotObject
{
    private Dictionary<string, T> register = new Dictionary<string, T>();

    public void Register(string key, T value)
    {
        register.Add(key, value);
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
        return default(T);
    }

    public bool Remove(string key)
    {
        return register.Remove(key);
    }

    public void Clear()
    {
        register.Clear();
    }
}
using System;

public interface IArgument
{
    public string Name { get; protected set; }
    public string ShortName { get; protected set; }
    public Func<string, bool> CheckFunction { get; protected set; }
    public bool Use(string value);
}
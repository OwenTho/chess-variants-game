using System;

public abstract class Argument<T> : IArgument
{
    public string Name { get; set; }
    public string ShortName { get; set; }
    public Func<string, bool> CheckFunction { get; set; }
    public Action<T> UseFunction { get; set; }
    protected Argument(string name, string shortName, Func<string, bool> checkFunction, Action<T> useFunction)
    {
        Name = name;
        CheckFunction = checkFunction;
        UseFunction = useFunction;
    }
    
    protected abstract T GetValue(string value);

    public bool Use(string value)
    {
        if (!CheckFunction(value))
        {
            return false;
        }
        
        T convertedValue = GetValue(value);

        UseFunction(convertedValue);
        return true;
    }
}
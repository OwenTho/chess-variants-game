using System;

public class StringArgument : Argument<string>
{
    public StringArgument(string name, string shortName, Func<string, bool> checkFunction, Action<string> useFunction) : base(name, shortName, checkFunction, useFunction)
    {
    }
    protected override string GetValue(string value)
    {
        return value;
    }
}
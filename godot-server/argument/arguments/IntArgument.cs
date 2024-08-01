using System;

public class IntArgument : Argument<int>
{
    public IntArgument(string name, string shortName, Func<string, bool> checkFunction, Action<int> useFunction) : base(name, shortName, checkFunction, useFunction)
    {
    }
    protected override int GetValue(string value)
    {
        return int.Parse(value);
    }
}
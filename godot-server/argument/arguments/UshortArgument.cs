using System;

public class UshortArgument : Argument<ushort>
{
    public UshortArgument(string name, string shortName, Func<string, bool> checkFunction, Action<ushort> useFunction) : base(name, shortName, checkFunction, useFunction)
    {
    }
    protected override ushort GetValue(string value)
    {
        return ushort.Parse(value);
    }
}
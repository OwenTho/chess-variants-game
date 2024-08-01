using System.Collections.Generic;
using System.Linq;

public class ArgumentList
{
    private List<IArgument> arguments = new();

    public void AddArgument(IArgument argument)
    {
        arguments.Add(argument);
    }
    
    
    public void Parse(string[] args)
    {
        foreach (var arg in args)
        {
            ParseArg(arg);
        }
    }

    private bool ParseArg(string arg)
    {
        // First check for "-" or "--"
        bool useShortName = true;
        if (arg.StartsWith("--"))
        {
            arg = arg.Substring(2);
            useShortName = false;
        }
        else if (arg.StartsWith("-"))
        {
            arg = arg.Substring(1);
        }
        else
        {
            return false;
        }
        // Split into name and value
        string[] splitArg = arg.Split("=", 2);
        // If it didn't split, then return
        if (splitArg.Count() == 1)
        {
            return false;
        }
        string name = splitArg[0];
        string value = splitArg[1];
        for (var i = arguments.Count - 1 ; i >= 0 ; i--)
        {
            IArgument argument = arguments[i];
            // Ignore if argument doesn't have a matching name
            if (!(useShortName && argument.ShortName == name) && !(!useShortName && argument.Name == name))
            {
                continue;
            }
            
            // Try to parse the argument
            if (argument.Use(value))
            {
                // If argument is parsed, remove it and move to the next.
                arguments.RemoveAt(i);
                return true;
            }
        }

        return false;
    }
}
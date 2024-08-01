using Godot;
using System;

public partial class Base : Node
{
    internal Server server;
    public static string ApplicationFile = "ChessVariantsGame (Dedicated Lobby).exe";

    public const int MinPort = 1024;
    public const int MaxPort = 49151;
    public override void _Ready()
    {
        server = new Server();
        
        // Process Arguments
        CheckArguments(OS.GetCmdlineArgs());

        AddChild(server);
    }
    
    private void CheckArguments(string[] args)
    {
        ArgumentList argList = new ArgumentList();
        
        argList.AddArgument(new UshortArgument("port", "p",
            IsValidPort,
            value =>
            {
                Console.WriteLine($"Using Port {value}.");
                server.Port = value;
            })
        );
        
        argList.AddArgument(new UshortArgument("max-lobby-port", "mxp",
            IsValidPort,
            value =>
            {
                Console.WriteLine($"Using Max Lobby Port {value}.");
                server.MaxLobbyPort = value;
            })
        );
        
        argList.AddArgument(new UshortArgument("min-lobby-port", "mnp",
            IsValidPort,
            value =>
            {
                Console.WriteLine($"Using Min Lobby Port {value}.");
                server.MinLobbyPort = value;
            })
        );
        
        argList.AddArgument(new IntArgument("buffer-size", "bs",
            argValue =>
            {
                if (!int.TryParse(argValue, out int bufferSize))
                {
                    throw new ArgumentException("Buffer Size must be an integer above 0.");
                }

                if (bufferSize < 0)
                {
                    throw new ArgumentException("Buffer Size must be an integer above 0");
                }

                return true;
            },
            value =>
            {
                Console.WriteLine($"Using Buffer Size of {value}.");
                server.BufferSize = value;
            })
        );
        
        argList.AddArgument(new StringArgument("app-location", "al",
            argValue =>
            {
                // Check is done by Program
                return true;
            },
            value =>
            {
                ApplicationFile = value;
            })
        );

        argList.Parse(args);
    }

    private static bool IsValidPort(string portString)
    {
        if (portString.Length > 6)
        {
            throw new ArgumentException("Port has a maximum length of 5.");
        }

        if (!int.TryParse(portString, out int port))
        {
            throw new ArgumentException("Port must be an integer 1024 - 49151.");
        }

        if (port < MinPort || port > MaxPort)
        {
            throw new ArgumentException("Port must be an integer 1024 - 49151.");
        }

        return true;
    }
}

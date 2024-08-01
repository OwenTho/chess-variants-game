using System;
using System.Diagnostics;
using Godot;

public class Lobby: IDisposable
{
    public Lobbies lobbies { get; private set; }
    public string LobbyCode { get; internal set; }
    public Process lobbyProcess { get; private set; }
    public int Port { get; private set; }

    public Lobby(Lobbies lobbies, string lobbyCode, int port)
    {
        this.lobbies = lobbies;
        LobbyCode = lobbyCode;
        Port = port;
    }
    
    internal bool Start()
    {
        if (lobbyProcess != null)
        {
            Console.WriteLine($"Already running the Lobby with code {LobbyCode}.");
            return true;
        }

        lobbyProcess = new Process();
        lobbyProcess.StartInfo.FileName = Base.ApplicationFile;
        lobbyProcess.StartInfo.Arguments = $"--default-port={Port} --lobby-code={LobbyCode}";

        // If lobby process fails to start, return false
        if (lobbyProcess.Start())
        {
            GD.Print($"Started new Lobby {LobbyCode}.");
            return true;
        }

        GD.Print($"Failed to start Lobby {LobbyCode}.");
        lobbyProcess.Dispose();
        lobbyProcess = null;
        return false;
    }

    public void Dispose()
    {
        if (!lobbyProcess.HasExited)
        {
            lobbyProcess.Kill();
        }

        lobbyProcess.Dispose();
    }
}
using System;
using System.Collections.Generic;

public class Lobbies : IDisposable
{
    private Server server;
    
    private List<int> _usedPorts = new();
    private Dictionary<string, Lobby> _lobbies = new();

    public Lobbies(Server server)
    {
        this.server = server;
    }

    internal Lobby CreateLobby()
    {
        int port = NewPort();
        string lobbyCode = NewCode();

        Lobby newLobby = new Lobby(this, lobbyCode, port);

        _usedPorts.Add(port);
        _lobbies.Add(lobbyCode, newLobby);
        return newLobby;
    }

    public int NewPort()
    {
        // Randomly get a Port in the valid range that isn't already
        // taken.
        bool taken = true;
        int newPort = 1024;
        while (true)
        {
            newPort = Random.Shared.Next(server.MinLobbyPort, server.MaxLobbyPort);
            // Can't be in used ports or the server port
            if (newPort == server.Port)
            {
                continue;
            }

            if (_usedPorts.Contains(newPort))
            {
                continue;
            }

            return newPort;
        }
    }

    private const string Digits = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public string NewCode()
    {
        // Randomly generate a new Lobby Code that isn't
        // already taken.
        int max = Digits.Length-1;
        while (true)
        {
            string code = "";
            for (var i = 0; i < server.CodeLength; i++)
            {
                code += Digits[Random.Shared.Next(0, max)];
            }
            
            // Make sure code doesn't already exist
            if (_lobbies.ContainsKey(code))
            {
                continue;
            }

            return code;
        }
    }

    public char GetCodeLetter(int index)
    {
        if (index < 0 || index > Digits.Length)
        {
            return '0';
        }

        return Digits[index];
    }
    
    public byte[] CodeToByte(string lobbyCode)
    {
        byte[] returnData = new byte[lobbyCode.Length];

        // Convert the lobby code into bytes
        for (int i = 0; i < lobbyCode.Length; i++)
        {
            char thisLetter = lobbyCode[i];
            int letterPos = Digits.IndexOf(thisLetter);
            returnData[lobbyCode.Length - i - 1] = (byte)letterPos;
        }
        
        return returnData;
    }

    public bool TryGetLobby(string lobbyCode, out Lobby lobby)
    {
        return _lobbies.TryGetValue(lobbyCode, out lobby);
    }

    public void WaitForClose(Lobby lobby)
    {
        lobby.lobbyProcess.Exited += (_, _) => CloseLobby(lobby);
    }

    private void CloseLobby(Lobby lobby)
    {
        _usedPorts.RemoveAt(lobby.Port);
        _lobbies.Remove(lobby.LobbyCode);
        // If the process is still running, stop it
        if (!lobby.lobbyProcess.HasExited)
        {
            try
            {
                lobby.lobbyProcess.Kill();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Tried to kill Lobby Application {lobby.LobbyCode}, but got an Error {e.Message}");
            }
        }

        lobby.Dispose();
    }
    
    public bool TryCloseLobby(string lobbyCode)
    {
        if (_lobbies.TryGetValue(lobbyCode, out Lobby lobby))
        {
            CloseLobby(lobby);
            return true;
        }

        return false;
    }

    public bool TryCloseLobby(Lobby lobby)
    {
        if (_lobbies.TryGetValue(lobby.LobbyCode, out Lobby matchingLobby))
        {
            // Lobbies must be the same
            if (lobby != matchingLobby)
            {
                Console.WriteLine($"Tried to remove Lobby {lobby.LobbyCode}, but the given class is an incorrect reference.");
                return false;
            }

            CloseLobby(lobby);
            return true;
        }

        return false;
    }

    public void Dispose()
    {
        foreach (var lobby in _lobbies.Values)
        {
            lobby.Dispose();
        }
    }
}
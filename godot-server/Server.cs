using Godot;
using System;

public partial class Server : Node
{
    public ushort Port { get; internal set; } = 9813;
    public ushort MinLobbyPort { get; internal set; } = Base.MinPort;
    public ushort MaxLobbyPort { get; internal set; } = Base.MaxPort;
    public int BufferSize { get; internal set; } = 1024;

    public int CodeLength { get; internal set; } = 5;



    private TcpServer server;
    private Clients clients;
    private Lobbies lobbies;
    
    public override void _Ready()
    {
        server = new TcpServer();
        clients = new Clients(this);
        lobbies = new Lobbies(this);


        if (server.Listen(Port) == Error.Ok)
        {
            GD.Print($"Listening on Port {Port}");
        }
        else
        {
            GD.Print("Failed to start listening.");
        }
    }

    public override void _Process(double delta)
    {
        if (server.IsConnectionAvailable())
        {
            StreamPeerTcp newPeer = server.TakeConnection();
            if (newPeer != null)
            {
                GD.Print($"New Client Connection: {newPeer.GetConnectedHost()}");
                clients.AddClient(newPeer);
            }
        }

        clients.CheckClients();
    }

    internal void ProcessClient(ServerClient client)
    {
        // Poll
        StreamPeerTcp serverPeer = client.serverPeer;
        serverPeer.Poll();

        // If no longer connected, stop here
        if (serverPeer.GetStatus() != StreamPeerTcp.Status.Connected)
        {
            return;
        }
        
        // If there is data available...
        if (serverPeer.GetAvailableBytes() == 0)
        {
            return;
        }
        
        // Get the first byte
        int data = serverPeer.GetU8();
        
        // Process based on Transmission
        switch (data)
        {
            case ReceivingTransmissions.Create:
                GD.Print("Create requested.");
                // If creating, make a new Lobby
                Lobby newLobby = lobbies.CreateLobby();

                GD.Print($"Starting new Lobby {newLobby.LobbyCode} on Port {newLobby.Port}.");
                
                newLobby.Start();

                lobbies.WaitForClose(newLobby);
                
                serverPeer.PutU8(SendingTransmissions.LobbyCreated);
                serverPeer.PutVar(newLobby.Port);
                
                // Close connection
                serverPeer.DisconnectFromHost();
                break;
            case ReceivingTransmissions.Join:
                GD.Print("Joined requested...");
                // Next bit of data is Lobby Code
                Variant lobbyCode = serverPeer.GetVar();
                bool failed = true;
                if (lobbyCode.VariantType == Variant.Type.String)
                {
                    // Try to get the lobby by the lobbyCode
                    string code = ((string)lobbyCode).ToUpper();
                    GD.Print($"... on Lobby {code}");
                    if (lobbies.TryGetLobby(code, out Lobby lobby))
                    {
                        failed = false;
                        serverPeer.PutU8(SendingTransmissions.LobbyPort);
                        serverPeer.PutVar(lobby.Port);
                        GD.Print($"Sending Lobby port: {lobby.Port}.");
                    }
                    else
                    {
                        GD.Print($"Lobby {code} does not exist.");
                    }
                }
                
                if (failed) {
                    // If failed to get lobby, send failed
                    serverPeer.PutU8(SendingTransmissions.Failed);
                }
                serverPeer.DisconnectFromHost();
                break;
            default:
                GD.Print("Invalid Transmission Code received.");
                // If it's an invalid code, send invalid
                serverPeer.PutU8(SendingTransmissions.InvalidCode);
                break;
        }
    }

    private bool closed = false;
    public override void _Notification(int what)
    {
        if (what == NotificationPredelete || what == NotificationWMCloseRequest)
        {
            if (closed)
            {
                return;
            }

            closed = true;
            
            // Stop the server
            server.Stop();
            
            // Close all the Processes
            lobbies.Dispose();
        }
    }
}

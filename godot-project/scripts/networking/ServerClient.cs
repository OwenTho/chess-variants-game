using Godot;

public partial class ServerClient : Node
{
    public string DefaultIp = "127.0.0.1";
    public int DefaultPort = 9813;
    
    private StreamPeerTcp streamPeer = new StreamPeerTcp();
    private StreamPeerTcp.Status status;

    public void ConnectToHost(string host, int port)
    {
        // If already connected, disconnect
        Disconnect();
        GD.Print($"Connecting to {host}:{port}");
        if (streamPeer.ConnectToHost(host, port) != Godot.Error.Ok)
        {
            GD.Print("Error connecting to the host.");
        }
    }
    
    [Signal]
    public delegate void PortReceivedEventHandler(int port);

    [Signal]
    public delegate void ConnectedEventHandler();

    [Signal]
    public delegate void DisconnectedEventHandler();

    [Signal]
    public delegate void ErrorEventHandler();
    

    public override void _Process(double delta)
    {
        streamPeer.Poll();
        StreamPeerTcp.Status newStatus = streamPeer.GetStatus();
        if (newStatus != status)
        {
            status = newStatus;
            switch (status)
            {
                case StreamPeerTcp.Status.None:
                    GD.Print("Disconnected from host.");
                    EmitSignal(SignalName.Disconnected);
                    break;
                case StreamPeerTcp.Status.Connecting:
                    GD.Print("Connecting to host.");
                    break;
                case StreamPeerTcp.Status.Connected:
                    GD.Print("Connected to host.");
                    EmitSignal(SignalName.Connected);
                    break;
                case StreamPeerTcp.Status.Error:
                    GD.Print("Error when connecting to host.");
                    EmitSignal(SignalName.Error);
                    break;
            }
        }
        
        if (status != StreamPeerTcp.Status.Connected)
        {
            return;
        }
        
        if (streamPeer.GetAvailableBytes() == 0)
        {
            return;
        }

        int data = streamPeer.GetU8();

        switch (data)
        {
            case ReceivingTransmissions.LobbyCreated: case ReceivingTransmissions.LobbyPort:
                // The following should be a Port
                Variant lobbyCode = streamPeer.GetVar();
                if (lobbyCode.VariantType != Variant.Type.Int)
                {
                    GD.Print($"Invalid Port type: {lobbyCode.VariantType}");
                    Disconnect();
                    break;
                }
                // If it's successful, take the port and emit
                int port = (int)lobbyCode;
                GD.Print($"Got port {port}");
                EmitSignal(SignalName.PortReceived, port);
                Disconnect();
                break;
            case ReceivingTransmissions.Failed:
                GD.Print("Failed Transmission.");
                Disconnect();
                break;
            case ReceivingTransmissions.InvalidCode:
                GD.Print("Invalid Code Transmission.");
                Disconnect();
                break;
            default:
                GD.Print("Received Unknown Code Transmission.");
                Disconnect();
                break;
        }
    }

    private void Disconnect()
    {
        if (streamPeer.GetStatus() != StreamPeerTcp.Status.Connected)
        {
            streamPeer.DisconnectFromHost();
            return;
        }
        GD.Print($"Disconnected from Server {streamPeer.GetConnectedHost()}.");
        streamPeer.DisconnectFromHost();
    }

    public void RequestCreate()
    {
        if (status != StreamPeerTcp.Status.Connected)
        {
            GD.Print("Can't request to create a Lobby as the Stream is not connected.");
            return;
        }
        
        streamPeer.PutU8(SendingTransmissions.Create);
    }

    public void RequestJoin(string lobbyCode)
    {
        if (status != StreamPeerTcp.Status.Connected)
        {
            GD.Print("Can't request to create a Lobby as the Stream is not connected.");
            return;
        }
        
        streamPeer.PutU8(SendingTransmissions.Join);
        streamPeer.PutVar(lobbyCode);
    }
}

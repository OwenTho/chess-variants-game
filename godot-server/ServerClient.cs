using Godot;

public class ServerClient
{
    public StreamPeerTcp serverPeer;

    public ServerClient(StreamPeerTcp serverPeer)
    {
        this.serverPeer = serverPeer;
    }
}
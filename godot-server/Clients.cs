using System.Collections.Generic;
using Godot;

public class Clients
{
    private Server server;
    
    private List<ServerClient> connectingClients = new List<ServerClient>();
    private List<ServerClient> clients = new List<ServerClient>();

    public Clients(Server server)
    {
        this.server = server;
    }
    
    internal void AddClient(StreamPeerTcp client)
    {
        if (client.GetStatus() == StreamPeerTcp.Status.Connecting)
        {
            connectingClients.Add(new ServerClient(client));
        }
        else if (client.GetStatus() == StreamPeerTcp.Status.Connected)
        {
            connectingClients.Add(new ServerClient(client));
        }
    }

    internal void RemoveClient(ServerClient client)
    {
        if (client.serverPeer.GetStatus() != StreamPeerTcp.Status.None)
        {
            client.serverPeer.DisconnectFromHost();
        }

        connectingClients.Remove(client);
        clients.Remove(client);
    }

    internal void CheckClients()
    {
        
        for (var i = clients.Count-1 ; i >= 0 ; i--)
        {
            ServerClient client = clients[i];
            
            // Tell Server to process if Client is connected
            server.ProcessClient(client);
            
            // If no longer connected, disconnect
            if (client.serverPeer.GetStatus() != StreamPeerTcp.Status.Connected)
            {
                RemoveClient(client);
                continue;
            }
        }
        for (var i = connectingClients.Count-1 ; i >= 0 ; i--)
        {
            ServerClient client = connectingClients[i];
            if (client.serverPeer.GetStatus() == StreamPeerTcp.Status.Connected)
            {
                connectingClients.RemoveAt(i);
                clients.Add(client);
            }
        }
    }
}
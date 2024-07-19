from twisted.internet.protocol import DatagramProtocol
from twisted.internet import reactor
from time import sleep

import random

import sys

MAX_IP_COUNT = 3 # per IP
MAX_TOTAL_CONNECTIONS = 50 # total unique IPs

MAX_SESSION_COUNT = 50

class ServerProtocol(DatagramProtocol):

    def __init__(self):
        # A Dictionary of active sessions
        self.active_sessions = {}
        # A Dictionary of the currently registered clients
        self.registered_clients = {}
        # Dictionary of registered ips,
        # to how many connections it has
        self.registered_ips = {}
    
    def client_is_registered(self, id: str) -> bool:
        return id in self.registered_clients
    
    def session_is_registered(self, id: str) -> bool:
        return id in self.active_sessions
    
    def ip_connected(self, ip: str) -> None:
        if not ip in self.registered_ips:
            self.registered_ips[ip] = 1
            return
        self.registered_ips[ip] += 1
    
    def ip_disconnected(self, ip: str) -> None:
        if not ip in self.registered_ips:
            print(f"Tried to remove an unregistered ip: {ip}")
            return
        self.registered_ips[ip] -= 1
        if self.registered_ips[ip] <= 0:
            del self.registered_ips[ip]

    def is_ip_valid(self, ip: str) -> bool:
        # If the IP isn't there, then it's valid
        if ip not in self.registered_ips:
            return True
        
        # Limit the number of connections each IP has.
        # Just makes it easier.
        if self.registered_ips[ip] >= MAX_IP_COUNT:
            return False
        
        return True

    def create_session(self, session_id, client_list: list["Client"] = None) -> "Session":
        # Make sure session with this id doesn't exist
        if session_id in self.active_sessions:
            return
        if client_list == None:
            client_list = []


    def register_client(self, session_id) -> None:
        # Try to add the client to the list
        new_id: str = Client.gen_id()
        pass

    def datagramReceived(self, data: bytes, addr: random.Any) -> None:
        print(data)
        data_string = data.decode("utf-8")
        msg_type = data_string[:2]
        pass

class Client():
    
    ip: str
    port: int
    session: "Session"
    
    def __init__(self):
        pass

    @classmethod
    def gen_id(cls) -> str:
        pass

class Session():
    
    server: ServerProtocol

    id: str
    ip: str
    port: int

    max_clients: int

    _clients: list[Client]

    def has_client(self, client: Client) -> None:
        return client in self._clients

    def _add(self, client: Client) -> bool:
        # If this session already has this client, ignore
        if self.has_client(client):
            return False
        client.session = self
        self._clients.append(client)
        return True

    def connect(self, client: Client) -> None:
        prev_session: Session = client.session
        # Try to add this client
        if not self._add(client):
            # If it fails, don't continue
            return
        # If the client already previously had another session,
        # disconnect them
        if prev_session is not None:
            client.session.disconnect(client)
        pass

    def _remove(self, client: Client) -> bool:
        # If the client isn't in this session, ignore
        if not self.has_client(client):
            return False
        self._clients.append(client)
        return True

    def disconnect(self, client: Client) -> None:
        # Try to remove this client
        self._remove(client)

    def forward_msg(self, msg: bytes) -> bool:
        for client in self._clients:
            self.server.transport.write()
    
    @classmethod
    def gen_id(cls) -> str:
        pass

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: ./server.py PORT")
        quit()
    
    # TODO: Add Scheduler below
    # scheduler = Scheduler()
    # reactor.callWhenRunning(scheduler.run)
    port = int(sys.argv[1])
    reactor.listenUDP(port, ServerProtocol())
    print(f"Listening on Port {port}.")
    reactor.run()
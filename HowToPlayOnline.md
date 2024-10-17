# How to play online

![The options in the game for making a server](img/htpo/1_play_buttons.png)  
Default Server IP: 127.0.0.1  
Default Server Port: 9813

## How to play using a playit.gg tunnel

1. Make an account for [https://playit.gg](https://playit.gg/).

2. Press "Set up a new agent", or go to [https://playit.gg/setup/agent](https://playit.gg/setup/agent).

![Button to set up a playit.gg agent](img/htpo/2_setup_agent_button.png)

3. Download ([https://playit.gg/download](https://playit.gg/download)) the program.

![Step 1 of setting up the playit program](img/htpo/3_download_text.png)

4. Run the program, and go to the link the program provides (or go to [https://playit.gg/claim](https://playit.gg/claim) and input the code manually). The link provided should look similar to this:

![Output from console window for link to setup agent link](img/htpo/4_example_claim_link.png)

5. Wait for the program to connect. Once it connects, it should look similar to this:

![Output from console window showing the agent is active with 0 tunnels](img/htpo/5_example_agent_registered.png)

6. Open up the link provided and press "Add Tunnel" under the "Tunnels" tab.

![Menu of agent, with an arrow pointing at the button to add a new tunnel](img/htpo/6_agent_screen.png)

7. In "Tunnel Type", select "UDP (protocol)". It *must* be UDP as the game uses UDP to communicate. If a warning regarding malware pops up, press the button stating you will not host malware:

!["No Malware!" playit.gg warning with an arrow pointing to the "Yes, I will not host malware" button](img/htpo/7_dont_host_malware.png)  
Then, once you have "UDP (protocol)" selected, you need to set the Local Port. This can either be the default port for the game (9813), or you can set it between the range 1025 \- 49151 (if not between that range it will use the default 9813).

Only you will use this, and it won't be shared with whoever you're playing with, so the default 9813 is best as you do not have to re-input it every time.

If you leave it blank, you'll need to use the automatically assigned Shared Port. *This may change between uses, so it's recommended to set it manually.*  
![The "Add Tunnel" menu, with "Global Anycast (free)" for Region, "UDP (protocol)" for Tunnel Type, 1 for Port Count and 9813 for Local Port](img/htpo/8_add_tunnel_screen.png)

8. If you do not remember the Local Port after the previous menu, you can view or set it below the usage graph in the "Update Local Address" section *(image below)*. Leave "Local Address" as 127.0.0.1, as that is an address that refers to your own device. But you must set the Local Port to whatever you (the server) plan to use.

![The "Update Local Address" panel on the tunnel page. It shows that Local Address is 127.0.0.1, and Local Port is 9813 as set in "Add Tunnel"](img/htpo/9_local_address_panel.png)

9. Now that the tunnel is set up, enable it. You will need to share the IP and port with the person you plan to play with. They must use this IP and Port:

![The top of the tunnel page, with an arrow pointing at the IP and Port a client should use to join your server. The domain link is scribbled out as it cannot be used](img/htpo/10_ip_and_port.png)  
Note that you must use the number IP as the game does not support domain IPs.

10. Finally, before the other player can join, you must create the server. Remember to use the port you set for yourself in Local Port. *You* (the server) must use the *Local* IP and Port that you set, while instead giving the *Shared* IP and Port to the person joining (the clients).

You (Server):  
Local IP (default 127.0.0.1)  
Local Port (default 9813\)  
![An image of what the server has to do to start. In this instance, the server uses the Local Port they set, 9813, and presses "Create Server"](img/htpo/11_create_server_button.png)  
The other player (Client):  
![An image of what the client has to do to join. In this instance, the client uses the public ip "147.185.221.21" and public port "3877" retrieved from the tunnel page in part 9](img/htpo/12_join_server_button.png)  
***If it does not work, you may need to restart the tunnel program running on your device.***

When in game's server lobby, you (the server admin) can give yourself a name, define who is Player 1 (white) and who is Player 2 (black) and start the game:  
![An image of the player list in the server lobby. It shows the name text input, and the buttons to change around the players.](img/htpo/13_player_lobby_list.png)  
Although not displayed visually, you can set one player to be P1 and P2, and they will be able to play the game against themselves (as if it were a local game).

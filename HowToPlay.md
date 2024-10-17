# Chess Variant Game

<a name="table-of-contents"></a>

## Table of Contents

[**Table of Contents**](#table-of-contents)

[**Starting The Game**](#starting-the-game)

[Downloading](#downloading)

[Local Play](#local-play)

[Online Play](#online-play)

[**Playing the Game**](#playing-the-game)

[Controls](#controls)

[Major Cards](#major-cards)

[Minor Cards](#minor-cards)

[Taking Your Turn](#taking-your-turn)

[How to play Chess](#how-to-play-chess)

[**Content**](#content)

[Major Cards](#major-cards-1)

[Minor Cards](#minor-cards-1)

[Pieces](#pieces)

[Pawn](#pawn)

[Rook](#rook)

[Knight](#knight)

[Bishop](#bishop)

[Queen](#queen)

[King](#king)

[Unique Pieces](#unique-pieces)

[Prawn](#prawn)

[Rock](#rock)

[Warp Bishop](#warp-bishop)

[Blockade](#blockade)

<a name="starting-the-game"></a>

## Starting The Game

<a name="downloading"></a>

### Downloading

To start playing the game, you first need to download it. This can be found in the [releases](https://github.com/OwenTho/chess-variants-game/releases/) on GitHub or by following the instructions in the "How to build" in the [Read Me](README.md).

The data is stored in a .zip file, and therefore needs to be extracted.  
![Zip file of the game](img/htp/1_zip_file.png)  
Windows devices have extraction for this built in by right clicking and pressing "Extract All", or opening it and dragging the files out into a separate folder, but you can also use applications such as WinRAR or 7zip.

Upon extracting the game, you should have the following:  
![The game files once extracted from the zip file](img/htp/2_game_files.png)  
A folder with the game .dll libraries, and the executable to run the game. The game needs both of these to function. The game can then easily be started by opening the executable file.

<a name="local-play"></a>

### Local Play

Local play is easy to start, only requiring you to press "Play Offline Game" in the middle of the screen.  
![A screenshot of the game menu, with arrows pointing to the "Play Offline Game" button](img/htp/3_play_offline.png)  
In Local Play, both players need to use the same device. You can see who's turn it currently is by if this arrow is being displayed next to the player's info display. Player 1 is White, and Player 2 is Black.  
![A screenshot of the player info when in game, with an arrow indicating that it's the player's turn](img/htp/4_offline_player_info.png)  

<a name="online-play"></a>

### Online Play

Online play is slightly more complex. There is no available public server, so you will either need to port forward, or create a tunnel to allow another player to connect to your game.

There is an explanation on how to set up a network tunnel using [https://playit.gg](https://playit.gg) in "[How to play Online](HowToPlayOnline.md)".

Another way is to set up a server if you know how to open a port. Running the executable with the "--dedicated\_server" argument will open a server directly on starting, with the first player who joins the server being given control over starting the game, and which player is which team.

You can start or join an Online game with these two buttons:  
![A screenshot of the game menu, with arrows pointing at the "Create Server" and "Join Server" buttons](img/htp/5_play_online.png)  
You will need to set the Server IP and Server Port if joining, whereas only the Server Port matters if creating the server.  
By default, the Server Port used is 9813\. *If using playit.gg for a tunnel, remember to use the public playit.gg port to join, not the local port set in the game.*

Similarly to Local Play, Online Play displays who the current player is using an arrow. However, it also displays the name of the player that they set in the server lobby before the game starts:  
![A screenshot of the player info in the game, with an arrow indicating that it's the player's turn. In addition, the player's "Name" appears under their number](6_online_player_info.png)

<a name="playing-the-game"></a>

## Playing the Game

<a name="controls"></a>

### Controls

Left Mouse Button \- Select  
Right Mouse Button \- Rotate Board

<a name="major-cards"></a>

### Major Cards

The first thing that happens for each player when you start a game is being shown 3 Major Cards.  
![A screenshot of a Major Card selection. The three displayed cards are "Lonely Pieces", "Level Up" and "Bigger Board"](img/htp/7_major_card_selection.png)  
Each Major Card impacts the game in different ways, making the experience slightly different each time. There are currently 7 different cards, and 2 cards will be used per game as each player is required to select and use one of them.

<a name="minor-cards"></a>

### Minor Cards

Minor Cards make smaller changes to the game, to individual pieces. These are given as the game progresses. Every turn a player takes gets them closer to obtaining a Minor Card. You can see how close a player is to getting their next Minor Card with the yellow bar at the bottom of their info display.  
![A screenshopt of a Minor Card selection. The two displayed cards are "Diagonal Move Further" and "Change Piece"](img/htp/8_minor_card_selection.png)  
However, it's important to know that taking your enemy's pieces will put them much closer to getting their next card.

<a name="taking-your-turn"></a>

### Taking Your Turn

When it is your turn in the game, the first thing you will need to do is select a piece. When hovering your mouse over the board, you should be able to see this red highlight:  
![A screenshot of the game with an arrow pointing at the player's hover highlight](img/htp/9_hover_highlight.png)

If you can see this highlight, it means that the game is registering your mouse hovering over the space. If you hover a piece and click the Left Mouse Button, you will select the piece:  
![A screenshot of the game with the Pawn in front of the white queen selected](img/htp/10_pawn_selected.png)

The piece will show an outline showing it is selected. In addition to that, green highlights will appear to show you what actions you can take. These actions can then be taken by clicking on a cell with a green highlight:  
![A screenshot of the game with the Pawn in front of the white queen having moved 2 spaces forwards.](img/htp/11_pawn_moved.png)  
After the action is finished (including piece promotion if it occurs), the turn will swap over to the next player.

<a name="how-to-play-chess"></a>

### How to play Chess

The basic rules of chess are below:

- The "King Piece" cannot be taken, and is instead put into "Check" when targeted by an attack.  
- If your "King Piece" is in "Check", then you must remove it from "Check" either by moving the "King Piece", or by moving another piece to protect it.  
- If it is not possible to take the "King Piece" out of "Check", then that player is in "Checkmate" and loses the game. This is checked automatically, so if the game does not end then there is a possible move to escape Checkmate.  
- If it is not possible for a player to take their turn, as there are no valid actions for them to take AND their "King Piece" is not in "Check", then the game results in a "Stalemate" as it isn't possible for the game to continue. Neither player wins in this instance.  
- A player can resign from the game, which immediately makes them lose the game. *There is no confirmation, so be careful to not press the button by accident.*

In addition to the above rules, there are a few more to take into account:

- If a player does not have a "King Piece", they lose the game. Losing your "King Piece" is not considered being put into "Check", and therefore is not prevented.  
- The "King Piece" can be a piece other than the King. For example, the "Pawn Army" card changes the "King Piece" into the Pawn.  
- When there is more than one "King Piece", they are treated as normal pieces. Only when there is one "King Piece" left is it able to be put into "Check".  
- The "King Piece" is unable to move through "Check". This means that the "King Piece" can still be trapped.

<a name="content"></a>

## Content

<a name="major-cards-1"></a>

### Major Cards

- **Bigger Board** : Makes the board bigger by 2 spaces in all directions.  
- **Friendly Fire** : Pieces are able to attack pieces on their own team.  
- **Level Up** : Pieces have all their actions immediately set to level 1, and have them increase with each Piece taken after.  
- **Lonely Pieces** : Pieces are unable to take any actions if they are not directly adjacent to another piece.  
- **Pawn Army** : Immediately changes all of the pieces on the board into Pawns.  
- **Shapeshift** : Whenever a piece takes another piece, the attacking piece will change into the piece it takes.  
- **Shuffle** : Immediately shuffles the pieces on the board amongst team pieces. The shuffle will be mirrored for both teams.

<a name="minor-cards-1"></a>

### Minor Cards

The following are the available *rule* cards.

- **Promotion** : Allows promotion of a piece into a *different* piece once the piece reaches the end of the board. This works for all pieces.  
- **Move Further** : Allows the piece to move further horizontally and vertically, in addition to attacking the space.  
- **Diag. Move Further** : Allows the piece to move further diagonally, in addition to attacking the space.  
- **Nothing** : Allows a piece to do nothing. This means that it can spend the turn without moving.

<a name="pieces"></a>

### Pieces

For the pieces, there are a couple things to know about in advance. The action cells are displayed, showing the distance each piece can move initially. In addition, attacks that aren't displayed are shown via a red arrow.  
If an action path isn't obvious, it will be shown with green arrows.  
Finally, the levels of actions are shown, with the initial level shown in blue.

<a name="pawn"></a>

#### Pawn

Initially, the Pawn is able to move one space forward, an extra space on its first turn and is able to attack diagonally forward one space. When levelling, the distance on forward movement and diagonal movement increases.

*Forward direction does not go to 2 spaces with the "Move Further" card, as it's move \+ attack, while Pawn's forward movement is only movement.*

*(En passant is also implemented)*

First Turn (Initial forward movement):  
![A screenshot of the pawn's initial 2 space forward movement](img/htp/piece/pawn_initial.png)  
Following Turns:  
![An image showing the possible actions for a pawn after its initial movement](img/htp/piece/pawn.png)

<a name="rook"></a>

#### Rook

Able to move and attack horizontally and vertically, initially 7 spaces. It is able to travel further as it levels up.  
![An image showing the possible actions for a rook](img/htp/piece/rook.png)

<a name="knight"></a>

#### Knight

Initially jumps and attacks positions in an L-shape away from it. When levelling up, jumps take a zig-zag pattern diagonally. In addition, every 4 levels an additional, closer jump will appear.  
Initial:  
![A screenshot of the knight's possible actions](img/htp/piece/knight_simple.png)  
With paths and levels:  
![An image showing the possible actions based on level for a knight](img/htp/piece/knight.png)

<a name="bishop"></a>

#### Bishop

Able to move and attack diagonally, initially 7 spaces. As its level increases, it is able to move further.  
![An image showing the possible actions for a bishop](img/htp/piece/bishop.png)

<a name="queen"></a>

#### Queen

Able to move and attack in all 8 directions, initially 7 spaces. As its level increases, it is able to move further.  
![An image showing the possible actions for a queen](img/htp/piece/queen.png)

<a name="king"></a>

#### King

Able to move and attack the 8 spaces surrounding it. In addition to that, as its level increases it can move further horizontally and vertically similarly to a Rook.  
![An image showing the possible actions for a king](img/htp/piece/king.png)

<a name="unique-pieces"></a>

### Unique Pieces

<a name="prawn"></a>

#### Prawn

A piece that flips between horizontal/vertical and diagonal actions for movement and attacks every time it moves. Initially, it goes 2 spaces.  
*The movement is based on how many times the piece has turned, so it may start diagonally if the piece has moved before.*  
Initial Turn:  
![An image showing the possible actions for a prawn on even turns](img/htp/piece/prawn_even.png)  
Alternate Turns:  
![An image showing the possible actions for a prawn on odd turns](img/htp/piece/prawn_odd.png)

<a name="rock"></a>

#### Rock

A piece that can do nothing initially.  
*(This means the piece can use your turn with no actions being done that affect the game)*  
![An image showing the possible actions for a rock](img/htp/piece/rock.png)

<a name="warp-bishop"></a>

#### Warp Bishop

A piece that can move like a Rook, ignoring other pieces, and is capable of attacking pieces 2 spaces away diagonally.  
![An image showing the possible actions for a warp bishop](img/htp/piece/warp_bishop.png)

<a name="blockade"></a>

#### Blockade

A piece that can initially only move left and right 2 spaces. It is invincible to other pieces, but can not attack any other pieces itself.  
![An image showing the possible actions for a blockade](img/htp/piece/blockade.png)
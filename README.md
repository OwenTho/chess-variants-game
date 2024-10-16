# Chess Variants Game

"Chess Variants Game" is a variant of chess that implements cards that modify the game. These cards are intended to alter the game in such a way that makes the experience vary significantly depending on the cards used.

## How to play

Look at [How to play](HowToPlay.md).

## How to play online

Look at [How to play online](HowToPlayOnline.md).

## How to build

To build the game, you'll first need to downloaad the source. There are various ways to do this, the easiest by pressing the "Code" button and pressing "Download ZIP" and extracting the files.

- Once you have the source code, you'll need to open it using Godot. The version of Godot that the project uses is **Godot 4.3** (which you can download [here](https://godotengine.org/download/archive/4.3-stable/)). You ***must*** get the .NET version of the engine, otherwise you won't be able to build the game.

- Once you have opened it with Godot, you *should* find that there are errors. This is because the .NET files need building. This is easy to do, as you will just need to press the hammer next to the play button (which is only on the .NET version of the engine). Once this is done, press Project -> Reload current project.

- When the project has reloaded, you should no longer have any errors. This means that you can now go to Project -> Exports and export the game. The only two build options set up are for Windows, so you may need to create one for your device if you are on a different Operating System.

- Once the game is built, you can play it by opening the "ChessVariantsGame.exe" application that it creates.

*Note: If you do not disable the "Debug" option when choosing output folder, the game will be exported with the debug features enabled.*

## Credits

### Resources used for making the game

- Godot Engine: <https://godotengine.org/>

- Visual Studio for initial C#: <https://visualstudio.microsoft.com/vs/>

- Jetbrains Rider for C# and profiler: <https://www.jetbrains.com/>

- (CC0) Colour Palette shader (used for Piece colouring): <https://godotshaders.com/shader/extensible-color-palette>

- (MIT) Outline that disrespects boundaries (used for Piece selection): <https://godotshaders.com/shader/outline-that-disrespects-boundaries>

- Sounds for moving, taking and checking Pieces: <https://pixabay.com/sound-effects/chess-pieces-60890>

### Resources used for learning how to use Godot

- Godot Documentation: <https://docs.godotengine.org/en/stable/index.html>

- "All 219 Godot Nodes Explained In 42 Minutes !" by Lukky: <https://www.youtube.com/watch?v=tO2gthp45MA>

- "The ultimate introduction to Godot 4" by Clear Code: <https://youtu.be/nAh_Kx5Zh5Q?si=1dwWUhFzsynsuCmX>

- "Make a Vampire Survivors in Godot 4" by Branno: <https://www.youtube.com/playlist?list=PLtosjGHWDab682nfZ1f6JSQ1cjap7Ieeb>
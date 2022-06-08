# WorldReader
Axiom Verge 2 World Reader to create modificated game files.
![Screenshot (124)](https://user-images.githubusercontent.com/51925459/171414361-7bc54cb9-4be9-41f4-befe-5d481005ffe6.png)

## Installation
- Download [.net 6.0](https://dotnet.microsoft.com/en-us/download)
- Download this app from the [release](https://github.com/MaragonMH/WorldReader/releases) section

## Usage
###### Recommended:
- Backup your World.xnb file from Programms/Epic Games/AxiomVerge2/Content/Art/TileMaps/.

###### Do this once:
- Place your World.xnb file from Programms/Epic Games/AxiomVerge2/Content/Art/TileMaps/ in the compressed folder of the application.
- Execute AssetDecompression.exe.
- Place the resulting file from the decompressed folder in your working directory for this app.
- Use [.xnb Converter](https://lybell-art.github.io/xnb-js/) to convert the World.xnb file from Programms/Epic Games/AxiomVerge2/Content/Art/TileSets/ to World.png.

###### Do this every time you want to change the World of Axiom Verge 2:
- Open this application.
- Press the open Button in the toolbar and select your World.xnb file.
- Wait for the request to open the World.png file.
- Wait for the app to load your files.
- Make the neccessary changes.
- Press the save Button and save the file back in your Programms/Epic Games/AxiomVerge2/Content/Art/TileMaps/ directory.

## Features
This app supports almost all core features to change the World of Axiom Verge 2.

### Open and Save 
This app is able to Open and Save the World.xnb file from Programms/Epic Games/AxiomVerge2/Content/Art/TileMaps/ 

### Interactive Map
This app supports a interactive map with pan with right mouse and zoom with mouse wheel for easy traversal. 
It features a selection for tiles and objects with the left mouse.
In case of an emergency you can always recenter the frame with the button in the toolbar.

https://user-images.githubusercontent.com/51925459/171423875-8df4189c-5e46-45dd-8e82-f6f59f4a3cd5.mp4

### Properties Menu
This app allows you to customize all supported parts of each tile and object, to a varying degree.
You need to select a tile or object with you left mouse beforehand.
|Property|Section|Description|
| --- | --- | --- |
|Properties|Tile Collision|This field is for specific tile based properties, like snow for particles or damageable.|
|Collision Set|Tile Collision|This field determines the collision for slopes and other specific filled tiles.|
|Collision Flags|TileCollision|A collection of the flags represented by the checkboxes below. Field itself is readonly|
|Layer|TileAppearance|This is the index of your Tile Map. For example the first entry of the 2nd row is 192. This is only visual.|
|Group Name|Map Object|This is the category name of the group.|
|Group Type|Map Object|Another category name for this object. This one is more important then the last one.|
|Properties|Map Object|Important properties for this object.|
|Enemy Route Vertices|Map Object|Only used in the generic Group Type. Mainly used for cutscene paths and those underwater spinning discs patrol path.| 
|Name|Map Object|Name of this object.|
|Id|Map Object|Readonly id of this object.|
|Position X|Map Object|The x position of the topleft corner of the object. Right is bigger. Tile Width is 16.|
|Position Y|Map Object|The y position of the topleft corner of the object. Down is bigger. Tile Height is 16.|
|Width|Map Object|The width of the object. Tile Width is 16.|
|Height|Map Object|The height of the object. Tile Height is 16.|
|Flip Horizontally|Map Object|Flips the object along the horizontal axis.|
|Flip Vertically|Map Object|Flips the object along the vertical axis.|
|Rotation|Map Object|Rotate the object. In °.|
|World Width|General|Readonly tile width of the whole world file. Default is 1242|
|World Height|General|Readonly tile height of the whole world file. Default is 450|
|Tileset Map Width|General|The tileset width of the whole tileset file. Default is 192|
|Tileset Map Height|General|The tile height of the whole tileset file. Default is 128|

### Visibility Options
This app grants you the ability to change the selected 
- Tile Map (Outside, Inside, Breach), 
- Render Layer (PrePreFG, PreFG, PreFGWater, FG, BG, BGProps, BG, BGWater) and
- Object Group (ApocalypseUrn, Boss, BreachPortal, Damageable, DamageTrigger, ExteriorDoor, ForceField, Generic, GlitchableTile,  GenericRegion, Item, Lattice, NPC, ParticleObject, Room, RoomAction, RoomTransition, PasscodeAction, SavePoint, SecretWorld, SecretWorldEntrance, SecretWorldItem, TileNPC, TriggerRegion, UdugDoor).

### Randomize Items
Added in version 1.2. This allows you to randomize all items in the World file.
These exclude the 9 apocalypse flasks m from the bosses and the health node inside the refill station.
One note and one apocalypse urn occur twice.
You can set each individual item to the place of another. To do that, open the Randomize menu in the toolbar, select a item and assign another one.
Continue until you set all your item.
After that you can take the decision to enter a seed and randomize all unset items as well.
If you don't want to randomize the other items just leave the seed field empty.
  
### Create Map Object
Comming soon. Hopefully

### Undo and Redo
Comming soon. Hopefully

### Copy, Cut and Paste
Comming soon. Hopefully

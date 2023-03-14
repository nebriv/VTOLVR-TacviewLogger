# VTOL VR Tacview Data Logger Mod

![header2](https://user-images.githubusercontent.com/1825214/85210315-da335c80-b30c-11ea-9200-7704414ddb69.png)

Hello and welcome to the VTOL VR Tacview Data Logger mod! 

It's been long time in the making and a fairly significant undertaking for me. The Tacview data log allows players to record their flights in real time and save all enemy, allied and player vehicle flight data. The data log (in ACMI format) can then be opened within the Tacview software for review, tactical analysis and determination as to why exactly the carrier did not clear you to land!

Just a couple of notes to start off with. I'm not a game developer and I don't work in C#, so I may have taken some shortcuts. I'm well aware this mod is not as well optimized as it could and should be. On large maps with numerous actors this mod *could* introduce significant lag as it loops through and collects all the necessary data. Future iterations will be able to handle this a bit better.

## Demos
Example flight (2v2) on Akutan

![Example flight](https://user-images.githubusercontent.com/1825214/85156214-c06c1980-b227-11ea-9710-9b2cab264ade.gif)

Highlighting the various Tacview models

![Showing off the various models](https://user-images.githubusercontent.com/1825214/85156218-c2ce7380-b227-11ea-9aa5-ab78476b107c.gif)

Fox 2! Showing off the flares and missile tracking

![Fox 2!](https://user-images.githubusercontent.com/1825214/85156223-c4983700-b227-11ea-8e3b-a08a981648e5.gif)

Even supports bombing runs!

![Pickle!](https://user-images.githubusercontent.com/1825214/85156234-c6fa9100-b227-11ea-8c45-c8f251bc1cea.gif)

Rolling scissors with an ASF-33.

![Rolling Scissors](https://user-images.githubusercontent.com/1825214/85156236-c82bbe00-b227-11ea-9151-bc2eb9adeeda.gif)


So thats why I wasn't cleared for a conventional landing...

![Whoops!](https://user-images.githubusercontent.com/1825214/85156242-c95ceb00-b227-11ea-8a1f-5f71c2ea0de1.gif)


## Usage
To use the mod simply install the mod, launch the game, and enable the mod. At the time of writing it is important that you enable the mod every time you start the game. Once you begin a flight scene the data logger will automatically begin recording, as of now there is no positive indication of the recording in flight.

### Requirements
- Tacview must be installed. I've been developing against the Advanced version, but I believe the starter version should work.
	 - https://www.tacview.net/product/about/en/
- VTOLVR Modloader must be installed and functional.
	 - https://vtolvr-mods.com/
- VTOL VR must be legitimately purchased and installed.
 	 - https://vtolvr.bdynamicsstudio.com/
	 
### Downloading and Installing
This mod will be available for download on [https://vtolvr-mods.com](https://vtolvr-mods.com/mod/ctvzwocs/). I would recommend using the One-Click Install function to automatically download the mod.

Alternatively you can download the latest release here:
https://github.com/nebriv/VTOLVR-TacviewLogger/releases/latest

If you are manually installing you need to place the downloaded zip file in your VTOL VR Game folder inside the `VTOLVR_ModLoader\mods\` directory. The Mod Loader should automatically detect and unzip it for you once launched.

### Running
To use the mod simply launch the game via the Mod Loader (open the Mod Loader and press play). Once in game, after you click start you should see a "Mods" button. Open the mods page, select the Tacview Data Logger and click load. You should be good to go! At the start of the first few missions (and on those of particular importance) I would recommend browsing to the `TacViewDataLogs` folder within the VTOL VR game folder, and making sure you see a new folder with today's date and time, in that folder should be the three files described below.

### Data Structure

All flight recordings are stored in the VTOL VR Game Folder under `TacViewDataLogs`. Within this folder multiple folders with the datetime stamp of the flight will be created. Within this flight folder three files should be created.
 - `datalog.acmi`
	 - This is the primary datalog created for viewing in Tacview. With Tacview installed, you should simply be able to double click this and open it.
 - `customHeightMapXML.txt`
	 - This is custom XML used to display the exported heightmap. See the heightmap section below.
 - `heightmap_MAPName.data`
	 - This is the heightmap data. Copy this file into `C:\ProgramData\Tacview\Data\Terrain\Custom\`. See Height Maps/Terrain for more details.

### Logged Parameters
All active actors (vehicles and projectiles) within a scene are logged.
The following parameters are generally all logged. Some ground and water vehicles may not contain all applicable data.
 - AOA
 - IAS
 - Altitude
 - AGL
 - Location
 - Callsign/Designation

### Models
VTOL VR models can be added to Tacview via the following steps:
 1. Copy the vtolvr.xml file to `C:\ProgramData\Tacview\Data\Database\Default` Properties
 2. Copy all the .obj files from the assets folder into `C:\ProgramData\Tacview\Data\Meshes`

All VTOL VR models are property of Boundless Dynamics, LLC. except the F-45 and F/A-26 Models created by F/A-26B on the VTOL VR Discord Channel.

### Height Maps/Terrain
Height maps are generally generated and exported at the start of every mission. They will be located in the mission's datalog folder. You should see `customHeightMapXML.txt` and `heightmap_MAPNAME.data`. To use this heightmap in Tacview follow the below steps.
 1. If you haven't done so, copy the `CustomHeightmapList.xml` file from `C:\Program Files (x86)\Tacview (beta)\Data\Terrain\Custom` into `C:\ProgramData\Tacview\Data\Terrain\Custom`.
 2. Copy the `.data` file to `C:\ProgramData\Tacview\Data\Terrain\Custom`.
 3. Copy the contents of the `customHeightMapXML.txt` file (Except for `<?xml version="1.0" encoding="utf-8"?>`) into the new `CustomHeightmapList.xml` (`C:\ProgramData\Tacview\Data\Terrain\Custom\CustomHeightmapList.xml`) between the `<CustomHeightmapList>` nodes. Any XML parsing errors in this file will cause Tacview to not properly load the terrain. 

## Reporting Bugs
If you wish to report any bugs you can do so by creating a new Issue in this Github repository or you can contact me on the VTOL VR modding discord (@nebriv0001). When reporting bugs please include as much detail as possible including steps to reproduce. If possible please include your player.log file as well as your TacviewDataLogger data for the mission(s) in question.

### Known Issues
- Bullets "bounce" back and are reused when fired for a long time.

## Planned Features
- Additional performance optimizations. 
- Live streaming to a Tacview Client near you! I plan on adding functionality to enable live streaming of data.
- Additional telemetry data.
- Support for Explosions/Chaff/Shrapnel/Smoke.

## Contributors/Resources/Special Thanks

 - BahamutoD, of Boundless Dynamics LLC.
	 - The creator of VTOL VR has been supportive and a great resource in my development efforts. Of course without his significant undertaking VTOL VR wouldn't exist in the first place.
	 https://bdynamicsstudio.com/
 - Ketkev and .Marsh.Mello.
	 - The developers of the VTOL VR modloader have been essential to helping me dive into creating mods, debugging, and optimizing. Without their support I would have given up long ago.
	 https://vtolvr-mods.com
 -  Frantz Raia, of Raia Software inc.
	 - The developer of Tacview patiently supported my endeavors as I learned the Tacview tool and its capabilities. He provided significant guidance towards my efforts of getting custom heightmaps into Tacview.
	 https://www.tacview.net
 - The guys on the VTOL VR Modding Discord Channel!
	 - GentleLeviathan, Temperz87,  THE GREAT OVERLORD OF ALL CHEESE, and many more people. Everyone has supported me in someway or another - whether it is through long and late nights of debugging, feedback on videos and GIFS, or just supportive commentary when the rough got rougher.
	 https://discord.gg/SsE2Nud
 - F/A-26B on the VTOL VR Discord Channel
	 - He provided guidance and a couple of custom plane models to use within Tacview.
 - TheFalcon
 	 - He graciously created the header image you see above! Check out his awesome work at https://twitter.com/RallyPointComic
 - TytanRock 
         - Thanks for the big PR with some bug patches!
 - thehamsterjam
         -  Providing a bug fix for internationalization! Thanks!
 - mattidg
         -  Providing some updates to support the latest version of VTOL VR.


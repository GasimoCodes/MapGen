# MapGen

For a given set of valid user-selected data generate an appropriate 2D map (in style of 2012 [SCP Containment Breach](https://github.com/Regalis11/scpcb)). This repository is under active development, and although mostly usable, is not intended for production.

![image](https://user-images.githubusercontent.com/22917863/232846068-3ee4f28c-2c7d-433a-9555-9570d819ec3a.png)


## Features

 - [x] Generate N amount of zones
 - [x] Pick up custom zone size and set of rooms to occupy each zone
 - [x] Select which zones to connect and by up to how many connectors
 - [x] Define rooms using ScriptableObjects
 - [x] Gurantee the spawn of rooms marked as MustSpawn
 - [x] Place rooms which occupy more than 1 cell (as long as room exits are within the same cell)
 - [x] Gurantee that all generated rooms are reachable
 - [x] Fully deterministic
 - [ ] The generator may fail under some conditions (specific config and seed). This appears to be caused by the zone state not properly being restored after a failed zone generation attempt. 
 - [ ] Not all fail-states are properly documented

## Quick Start
The generator uses Map and Room Presets ScriptableObjects to tweak the generation variables. You may create or modify these files using the intuitive ScriptableObject Editor within Unity or your text editor. You may create these assets using the **Create/SCPEditor** Unity project tab context menu.

#### Defining RoomPreset
Contains necessary definitions to define one room preset. These files are then used in the zones field in MapPresets.

|Property name|Description  |
|--|--|
| Room Name | Name of room displayed to players |
| Shape | Defines the shape of room for the generator |
| Path Finder Travel Cost | Changes the probability of this room being a walk-through room. |
| Large | If the room extends past a single cell. |
| Expand Relative to Origin | If the Large field is marked, this array identifies all the relative cells which this room expands into. |
|Is Exit | Marks this room to be used as a zone connector. |
|Room Addr | The 3D art asset of the room. This can be a prefab/addressable. |
|MustSpawn | Marks this room as important - all these rooms must spawn. |



#### Defining MapPreset
Contains necessary definitions to define the map and its zones.

|Property name|Description  |
|--|--|
| Zones | Array of zone definitions. |
| Connections | An Array of connections between zones |
| Path Finder Travel Cost | Name of room displayed to players |
| ZoneLayout | A modifiable 2D string array representing the layout of zones to spawn. |
| GridSizeX | Amount of cells per each axis of each zone. |
| Spacing | The size of each cell. |



## Documentation and Tech Overview
[Google Doc](https://docs.google.com/document/d/1rY4tgInwJ9if1UFdFK7_NyAuGlZ8Y0QX9g3KuYgEy9A/edit?usp=sharing)

#### Attribution
This project uses 3rd party asset Array2DEditor by Eldoir https://github.com/Eldoir/Array2DEditor for displaying neat 2D arrays in the editor.


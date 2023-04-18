# MapGen

For a given set of valid user-selected data generate an appropriate 2D map (in style of 2012 [SCP Containment Breach](https://github.com/Regalis11/scpcb)). This repository is under active development, and although mostly usable, is not intended for production.

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

## Quick Start
The generator uses Map and Room Presets ScriptableObjects to tweak the generation variables. You may create or modify these files using the intuitive UI integration within Unity or your text editor. You may create these assets using the Create/SCPEditor Unity project tab context menu.

### Defining RoomPreset
Contains necessary definitions to define one room preset. These files are then used in the zones field in MapPresets.

### Defining MapPreset

Contains necessary definitions to define the map and its zones.



## Documentation and Tech Overview


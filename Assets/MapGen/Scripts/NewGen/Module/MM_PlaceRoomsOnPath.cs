using ALOB.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALOB.Map
{
    public class MM_PlaceRoomsOnPath : MapGenModule, IGenRoomSpawn
    {

        bool enableLargeRoomsFillPath;

        public MM_PlaceRoomsOnPath(System.Random randomGen, GeneratorMapPreset gMP, bool enableLargeRoomsFillPath) : base(randomGen, gMP)
        {
            this.enableLargeRoomsFillPath = enableLargeRoomsFillPath;
        }

        /// <summary>
        /// Finds valid room for path and assigns it
        /// </summary>
        /// <param name="zoneObj"></param>
        /// <returns></returns>
        public bool PerZoneModule(ref Zone curZone)
        {
            return setRoomsAlongPath(ref curZone);
        }

        
        private bool setRoomsAlongPath(ref Zone zoneObj)
        {

            // Cache rooms we can pick to spawn!
            // Todo: Probability

            List<RoomData> shapeHallwayCandidates = new List<RoomData>();
            List<RoomData> shapeEndroomCandidates = new List<RoomData>();
            List<RoomData> shapeCRoomCandidates = new List<RoomData>();
            List<RoomData> shapeTCandidates = new List<RoomData>();
            List<RoomData> shapeFourCandidates = new List<RoomData>();

            // Filter rooms and put em into candidates based on shape
            foreach (catalogueEntry cE in zoneObj.roomCatalogue)
            {
                // Do not use mustSpawn or exit rooms as they have been placed previously
                if (cE.room.isExit || cE.room.mustSpawn)
                    continue;

                // Disable large rooms to fill path if disabled in inspector
                if (!enableLargeRoomsFillPath)
                    if (cE.room.large)
                    {
                        Debug.Log("[Gen] Excluded room " + cE.room.roomName + " from Path as enableLargeRoomsFillPath is false.");
                        continue;
                    }

                switch (cE.room.shape)
                {
                    case RoomShapes.ENDROOM:
                        {
                            shapeEndroomCandidates.Add(cE.room);
                            break;
                        }
                    case RoomShapes.HALLWAY:
                        {
                            shapeHallwayCandidates.Add(cE.room);
                            break;
                        }
                    case RoomShapes.CROOM:
                        {
                            shapeCRoomCandidates.Add(cE.room);
                            break;
                        }
                    case RoomShapes.TSHAPE:
                        {
                            shapeTCandidates.Add(cE.room);
                            break;
                        }
                    case RoomShapes.FOURDOORS:
                        {
                            shapeFourCandidates.Add(cE.room);
                            break;
                        }
                }
            }

            // If we happen not to have room of each shape, cease execution. This could be improved to instead throw errors only when necessary.
            if (shapeHallwayCandidates.Count == 0 || shapeEndroomCandidates.Count == 0 || shapeCRoomCandidates.Count == 0 || shapeTCandidates.Count == 0 || shapeFourCandidates.Count == 0)
            {
                Debug.LogError("[Gen] Missing room shape to spawn along path!" + (enableLargeRoomsFillPath ? "" : " (large rooms disabled to fill path and have been skipped)" + zoneObj.name));
                return true;
            }

            // For each position of path in the map, assign room
            foreach (Vector2Int location in zoneObj.pathLocation)
            {

                CellData cD = zoneObj.getCellAt(location);

                if (cD.containerType == containerType.ROOM)
                {
                    continue;
                }

                /*
                Here we need to select which room fits the path requirement. This is a little lengthly as we need to check rotations too! Yikes!
                We figure which shape the room is, the rotation and then assign it randomly from the valid candidates list.
                BoolArray pattern: N W E S
                */


                bool spawnLargeCheck = false;
                Room toSpawn = null;
                int rotateBy = 0;

                // Convert bool array to int so I can SWITCH() this
                int shape = 0000;

                if (cD.ph_exits[0])
                    shape += 1000;
                if (cD.ph_exits[1])
                    shape += 0100;
                if (cD.ph_exits[2])
                    shape += 0010;
                if (cD.ph_exits[3])
                    shape += 0001;


                while (!spawnLargeCheck)
                {
                    //NWES
                    switch (shape)
                    {
                        // FourDoors, rotation independent
                        case 1111:
                            {
                                toSpawn = new Room(shapeFourCandidates[randomGen.Next(0, shapeFourCandidates.Count)]);
                                break;
                            }
                        //-------------------------------------------------- T SHAPE
                        case 0111: // Facing North
                            {
                                toSpawn = new Room(shapeTCandidates[randomGen.Next(0, shapeTCandidates.Count)]);
                                break;
                            }
                        case 1101: // Facing East
                            {
                                toSpawn = new Room(shapeTCandidates[randomGen.Next(0, shapeTCandidates.Count)]);
                                rotateBy = 1;
                                break;
                            }
                        case 1110: // Facing SOUTH
                            {
                                toSpawn = new Room(shapeTCandidates[randomGen.Next(0, shapeTCandidates.Count)]);
                                rotateBy = 2;
                                break;
                            }
                        case 1011: // Facing WEST
                            {
                                toSpawn = new Room(shapeTCandidates[randomGen.Next(0, shapeTCandidates.Count)]);
                                rotateBy = 3;
                                break;
                            }
                        //-------------------------------------------------- HALLWAY

                        case 1001: //North or South
                            {
                                toSpawn = new Room(shapeHallwayCandidates[randomGen.Next(0, shapeHallwayCandidates.Count)]);
                                break;
                            }
                        case 0110: // WEST or EAST
                            {
                                toSpawn = new Room(shapeHallwayCandidates[randomGen.Next(0, shapeHallwayCandidates.Count)]);
                                rotateBy = 1;
                                break;
                            }
                        //-------------------------------------------------- CROOM

                        case 0011: //North
                            {
                                toSpawn = new Room(shapeCRoomCandidates[randomGen.Next(0, shapeCRoomCandidates.Count)]);
                                break;
                            }

                        case 0101: //EAST
                            {
                                toSpawn = new Room(shapeCRoomCandidates[randomGen.Next(0, shapeCRoomCandidates.Count)]);
                                rotateBy = 1;
                                break;
                            }

                        case 1100: //SOUTH
                            {
                                toSpawn = new Room(shapeCRoomCandidates[randomGen.Next(0, shapeCRoomCandidates.Count)]);
                                rotateBy = 2;
                                break;
                            }

                        case 1010: //WEST
                            {
                                toSpawn = new Room(shapeCRoomCandidates[randomGen.Next(0, shapeCRoomCandidates.Count)]);
                                rotateBy = 3;
                                break;
                            }

                        //-------------------------------------------------- END ROOM

                        case 0001: //NORTH
                            {
                                toSpawn = new Room(shapeEndroomCandidates[randomGen.Next(0, shapeEndroomCandidates.Count)]);
                                break;
                            }


                        case 0100: // EAST
                            {
                                toSpawn = new Room(shapeEndroomCandidates[randomGen.Next(0, shapeEndroomCandidates.Count)]);
                                rotateBy = 1;
                                break;
                            }


                        case 1000: // SOUTH
                            {
                                toSpawn = new Room(shapeEndroomCandidates[randomGen.Next(0, shapeEndroomCandidates.Count)]);
                                rotateBy = 2;
                                break;
                            }


                        case 0010: // WEST
                            {
                                toSpawn = new Room(shapeEndroomCandidates[randomGen.Next(0, shapeEndroomCandidates.Count)]);
                                rotateBy = 3;
                                break;
                            }

                        case 0000:
                            Debug.Log("GEN Unset!! " + " at " + cD.loc);
                            Debug.LogError("Room record not found when building room path.");
                            break;
                    }



                    spawnLargeCheck = true;

                    if (rotateBy != 0)
                        toSpawn.RotateClockwise(rotateBy);

                    cD.setRoom(toSpawn);

                    // Large room validity check!
                    if (toSpawn.getData().large)
                        foreach (CellData expCD in zoneObj.getExpansionFromRoomGlobal(toSpawn))
                        {
                            if (expCD.containerType != containerType.EMPTY)
                            {
                                spawnLargeCheck = false;
                                Debug.Log("ROom check isLarge failed for " + toSpawn.getData().roomName);
                                break;
                            }
                        }
                }
            }


            return false;
        }
    }

}

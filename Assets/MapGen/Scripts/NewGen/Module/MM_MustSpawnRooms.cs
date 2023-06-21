using ALOB.Map;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ALOB.Map
{
    public class MM_MustSpawnRooms : MapGenModule, IGenRoomSpawn
    {
        bool disableBFS;
        int maxIterationsBeforeFallback;

        public MM_MustSpawnRooms(System.Random randomGen, GeneratorMapPreset gMP, bool disableBFS, int maxIterationsBeforeFallback) : base(randomGen, gMP)
        {
            this.disableBFS = disableBFS;
            this.maxIterationsBeforeFallback = maxIterationsBeforeFallback;
        }

        public bool PerZoneModule(ref Zone curZone)
        {
            return populateGrid(ref curZone);
        }


        /// <summary>
        /// Loop which populates the zoneObj with its rooms. Returns true if the generation successfuly finished.
        /// </summary>
        /// <param name="zoneObj"></param>
        bool populateGrid(ref Zone zoneObj)
        {

            // List of rooms which must spawn sorted by size
            List<RoomData> mustHaves = CellsUtils.getMustSpawnListSorted(zoneObj, randomGen);

            // List of possible positions we can put rooms into
            List<CellData> mustHavesCandidates = CellsUtils.getCellPrimeCandidates(zoneObj);

            // List of target cells for Room pathfinding check.
            List<CellData> mustBeReachable = new List<CellData>();
            mustBeReachable.AddRange(zoneObj.getExitLocations());

            // Ammount of iterations we spent on this zone.
            int zoneAttempts = 0;

            // Check we do not have more mustSpawn rooms than the candidates
            if (mustHaves != null && mustHavesCandidates.Count < mustHaves.Count)
            {
                MapGenLogger.LogError("[Gen] The amount of cell candidates (" + mustHavesCandidates.Count + ") is not large enough for all rooms (" + mustHaves.Count + ") (" + zoneObj.name + ")");
                return false;
            }


            // We make a copy of the current zoneObj state so we can revert if anything goes bad.
            Zone tempZone = (Zone)zoneObj.Clone();



            // - - - POPULATE CANDIDATES WITH ROOMS RANDOMLY HERE - - -

            // To avoid infinite loops
            bool failed = false;
            int counter = 0;

            // Loop where we pick room position and rotation on the grid. Will cease execution shall an maximum amount of tries be exceeded. Introduced emergency to cap infinite loops.
            while (zoneAttempts < maxIterationsBeforeFallback)
            {


                // If this is not the first try
                if (zoneAttempts > 0)
                {
                    MapGenLogger.Log($"<color=blue>{zoneObj.name}: {zoneObj.getCellAt(new Vector2Int(3, 0))}</color>");

                    // This is a hacky solution, I know.
                    CellData[] exitsBackup = zoneObj.getExitLocations();

                    // Here we reset the zone to last working state
                    zoneObj = tempZone.Clone() as Zone;

                    foreach(CellData exits in exitsBackup)
                    {
                        zoneObj.cellGrid[exits.loc.x, exits.loc.y] = exits;
                        zoneObj.setRoomAt(exits.loc.x, exits.loc.y, exits.getRoom());
                    }

                    // And the list of possible positions we can put rooms into
                    mustHavesCandidates = CellsUtils.getCellPrimeCandidates(zoneObj);

                    // Reset MustBeReachable Cells to prev state
                    mustBeReachable = new List<CellData>();
                    mustBeReachable.AddRange(zoneObj.getExitLocations());

                    
                    MapGenLogger.Log("<color=yellow>[" + zoneAttempts + "] Attempt to generate " + zoneObj.name + ".</color>");


                }

                

                failed = false;


                // For each roomData we have, we want to spawn a room
                if (mustHaves != null)
                {
                    foreach (RoomData rD in mustHaves)
                    {
                        int tries = 0;
                        Room result = null;

                        // List of locations we tried and which failed, so we dont search it again.
                        List<Vector2Int> wrongLocations = new List<Vector2Int>();

                        // Attempt to spawn a room
                        while (tries < maxIterationsBeforeFallback && result == null)
                        {
                            // Get random cell
                            CellData cand = CellsUtils.getRandomEmptyCell(mustHavesCandidates, randomGen, wrongLocations);

                            // If we ran out of cells to try
                            if (cand == null)
                            {
                                Debug.Log("[Gen] All cells not viable for " + rD.name + ".");
                                Shuffler.Shuffle(mustHaves, randomGen);
                                break;
                            }

                            // Debug.Log("Spawn attempt - " + tries + "/" + zoneAttempts + ": " + rD.name + " at " + cand.loc);
                            // Check validity
                            result = placeRoomImportant(zoneObj, rD, cand, mustBeReachable);


                            if (result == null)
                            {
                                wrongLocations.Add(cand.loc);
                                // Try to place the room again
                                tries++;
                                // Debug.Log("Spawn attempt: " + tries + " for " + rD.roomName + "/" + zoneObj.name);
                                continue;
                            }
                            else
                            {
                                // CONFIRM SET THE ROOM HERE
                                zoneObj.setRoomAt(cand.loc.x, cand.loc.y, result);
                                mustBeReachable.Add(cand);
                                // Debug.Log("[Gen] <color=green>Planting room " + result.getData().roomName + " to grid at: " + cand.loc + "/" + zoneObj.name + " </color>");
                                counter++;
                            }
                        }

                        // If the loop above failed to provide us a room
                        if (result == null)
                        {
                            // Try to generate the zone again.
                            MapGenLogger.Log("<color=red>Failed to place " + rD.name  + " " + zoneObj.name + "</color>");

                            zoneAttempts++;
                            failed = true;
                            break;
                        }

                    }
                }
                else
                {
                    MapGenLogger.Log("There is no room to spawn for " + zoneObj.name);
                }

                if (!failed)
                {

                    zoneObj.mustSpawnLocations = mustBeReachable.Select(x => x.loc).ToList();

                    break;
                }

            }

            return failed;
        }


        /// <summary>
        /// Place a room in zone (includes validy checks). Returns false is spawn failed.
        /// </summary>
        /// <param name="zoneObj">Zone into which to place the room</param>
        /// <param name="r">The desired room data</param>
        /// <param name="cDPreset">A Location to attempt to put the room into</param>
        public Room placeRoomImportant(Zone zoneObj, RoomData rDP, CellData cDPreset, List<CellData> pathFindTargets)
        {


            // Backup.
            CellData temp = new CellData(zoneObj.getCellAt(cDPreset.loc).containerType);
            // CellData[] largeRoomBefores = (CellData)zoneObj.getCellAt(cDPreset.loc).Clone();

            // Init room   
            Room r = new Room(rDP);
            r.x = cDPreset.loc.x;
            r.y = cDPreset.loc.y;
            cDPreset.setRoom(r);
            string debugMessage = "";

            // Define if we passed the generation, this is false if we tried too many times to no avail.
            bool allPass = true;

            // Keep track of tries we had to place a room.
            int retryCount = 0;

            //Debug.Log("<color=orange>ATTEMPT TO SPAWN: " + r.getData().roomName + cDPreset.loc + "</color>");

            // Init PlacementValidator
            PlacementValidator validator = new PlacementValidator(zoneObj);

            // Capped to 4 to try all rotations in this cell
            while (retryCount <= 4)
            {

                // Debug.Log("Spawning room: " + r.getData().roomName + " attempt: " + (retryCount + 1) + " rot: " + r.angle);

                allPass = true;


                // If room is large, we make sure the extensions are valid positions.
                if (r.getData().large)
                {
                    allPass = validator.PositionFitnessCheckLargeRoom(r);
                    if (!allPass)
                    {
                        debugMessage = "Large room expanse into invalid space.";

                    }
                }


                // Check whether the room exits are valid (Execute only if we havent screwed up previous check)
                if (allPass)
                {
                    allPass = validator.ExitsOcclusionCheck(r, cDPreset);
                    if (!allPass)
                    {
                        debugMessage = "Exit leads into invalid space.";
                    }
                }

                // Check if the room doesnt obscure different rooms exits
                if (allPass)
                {
                    allPass = validator.RoomDoesntOccludeCheck(r, cDPreset, temp);
                    if (!allPass)
                    {
                        debugMessage = "Room obscures an existing room exit.";
                    }
                }


                // Check if all rooms are still reachable
                if (disableBFS)
                {
                    MapGenLogger.Log("<color=red>Warning: BFS Disabled!</color>");
                }

                if (allPass)
                    // Check reachibility of all rooms
                    if (allPass && !disableBFS)
                    {
                        int cantReach = validator.RoomPathfindingCheck(r, cDPreset, pathFindTargets);
                        if (cantReach != 0)
                        {
                            allPass = false;
                            // Debug.Log("Pathfinding cant reach " + cantReach);
                        }
                    }


                if (allPass == false)
                {
                    // No need to rotate, all exits same.
                    if (r.getData().shape == RoomShapes.FOURDOORS)
                    {
                        break;
                    }

                    retryCount++;
                    r.RotateClockwise(1);
                    continue;

                }
                else
                {
                    // All finished and ok, we can exit the rotation loop.
                    break;
                }

            }


            if (allPass)
            {

                // Set BLOCKED Area if LARGE
                if (r.getData().large)
                {
                    foreach (CellData occupiedCells in zoneObj.getExpansionFromRoomGlobal(r))
                    {
                        occupiedCells.containerType = containerType.BLOCKED;
                    }
                }

                // Debug.Log("<color=green>Room spawned: " + r.getData().roomName + cDPreset.loc + " " + r.angle + "</color>");
                // If no problems occured, mark blocked cells, reserved cells and go.
                return r;
            }
            else
            {
                // Room cannot be here.
                // Debug.Log("[Gen] " + zoneObj.name + " Spawn attempt failed for " + r.getData().name + cDPreset.loc + ", due to: " + debugMessage + " at " + cDPreset.loc);
                cDPreset.containerType = temp.containerType;
                return null;

            }
        }


    }
}
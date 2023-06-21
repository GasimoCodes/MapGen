using ALOB.Map;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ALOB.Map
{
    public class MM_ConPlacer : MapGenModule, IGenRoomSpawn
    {
        bool disableBFS;
        int maxIterationsBeforeFallback;
        

        public MM_ConPlacer(System.Random randomGen, GeneratorMapPreset gMP, bool disableBFS, int maxIterationsBeforeFallback) : base(randomGen, gMP)
        {
            this.moduleName = "ConPlacer";
            this.disableBFS = disableBFS;
            this.maxIterationsBeforeFallback = maxIterationsBeforeFallback;
        }

        public bool PerZoneModule(ref Zone curZone)
        {
            return populateGrid(ref curZone);
        }


        /// <summary>
        /// Loop which populates the zoneObj with exit rooms. Returns true if the generation successfuly finished.
        /// </summary>
        /// <param name="zoneObj"></param>
        bool populateGrid(ref Zone zoneObj)
        {
            
            string refName = zoneObj.name;
            zoneConnector[] connectors = gMP.connections.Where(c => (c.fromZone == refName || c.toZone == refName)).ToArray();

            // For each connector we have described
            foreach (zoneConnector connector in connectors)
            {
                Direction dir;
                CellData[] exitLocations;

                // Candidates for exit rooms
                List<catalogueEntry> candidates = zoneObj.roomCatalogue.Where(x => x.room.isExit).ToList();

                if (candidates.Count == 0)
                {
                    MapGenLogger.Log("<color=red>Cannot spawn exits because there are no defined exit rooms in the template!</color>", moduleName);
                    return false;
                }

                if (connector.maxAmountOfConnections > (gMP.gridSizeX / 2))
                {
                    MapGenLogger.Log("<color=orange>Exit spawner may fail because you have selected a big amount of exits / gridSize</color>", moduleName);
                }

                // Get direction to orient exit rooms in
                if (connector.fromZone == refName)
                {
                    dir = connector.getZoneFacing();
                    exitLocations = connector.getConnectorsFrom();
                }
                else
                {
                    dir = VectorUtils.reverseDirection(connector.getZoneFacing());
                    exitLocations = connector.getConnectorsTo();
                }


                foreach (CellData cD in exitLocations)
                {
                    // Make sure not to overwrite existing exits
                    if (cD.getRoom() != null)
                        continue;

                    Room r = placeExit(zoneObj, candidates.ToArray(), cD, exitLocations, dir);

                    zoneObj.setRoomAt(cD.loc.x, cD.loc.y, r);
                }


            }



            return false;
        }


        /// <summary>
        /// Place a room in zone (includes validy checks). Returns false is spawn failed.
        /// </summary>
        /// <param name="zoneObj">Zone into which to place the room</param>
        /// <param name="r">The desired room data</param>
        /// <param name="cDPreset">A Location to attempt to put the room into</param>
        public Room placeExit(Zone zoneObj, catalogueEntry[] exitTemplates, CellData cDPreset, CellData[] pathFindTargets, Direction dir)
        {
            // Define if we passed the generation, this is false if we tried too many times to no avail.
            bool allPass = true;

            // Backup.
            CellData temp = new CellData(zoneObj.getCellAt(cDPreset.loc).containerType);

            // Templates to try to put in the exit place
            exitTemplates = Shuffler.Shuffle(exitTemplates, randomGen);

            // Init PlacementValidator
            PlacementValidator validator = new PlacementValidator(zoneObj);

            // Init room              
            string debugMessage = "";

            Room r = null;


            // Since this is exit, we ought to try all the combinations before giving up. They are not rotation dependend since we have fixed direction!
            foreach (catalogueEntry cE in exitTemplates)
            {

                r = new Room(cE.room);
                r.x = cDPreset.loc.x;
                r.y = cDPreset.loc.y;

                // Rotate appropriately
                while (r.angle != dir)
                {
                    r.RotateClockwise(1);
                }

                cDPreset.setRoom(r);

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
                    allPass = validator.ExitsOcclusionCheck(r, cDPreset, true);
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
                        int cantReach = validator.RoomPathfindingCheck(r, cDPreset, pathFindTargets.ToList());
                        if (cantReach != 0)
                        {
                            allPass = false;
                            MapGenLogger.Log("Pathfinding cant reach " + cantReach);
                        }
                    }


                if (allPass == false)
                {
                    MapGenLogger.Log("<color=orange>FAILED: " + r + " " + zoneObj.name + " due " + debugMessage + "</color>", moduleName);
                    continue;
                }
                else
                {
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

                MapGenLogger.Log("<color=green>EXIT spawned: " + r + " " + zoneObj.name + "</color>", moduleName);
                // If no problems occured, mark blocked cells, reserved cells and go.
                return r;
            }
            else
            {
                // Room cannot be here.
                MapGenLogger.LogError(zoneObj.name + " Spawn attempt failed for " + r.getData().name + cDPreset.loc + ", due to: " + debugMessage);
                cDPreset.containerType = temp.containerType;
                return null;

            }
        }


    }
}
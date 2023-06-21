using ALOB.Map;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ALOB.Map
{
    public class MM_PathfindConnect : MapGenModule, IGenRoomSpawn
    {
        public MM_PathfindConnect(System.Random randomGen, GeneratorMapPreset gMP) : base(randomGen, gMP)
        {

        }

        /// <summary>
        /// Connects all currently placed rooms
        /// </summary>
        /// <param name="zoneObj"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool PerZoneModule(ref Zone zoneObj)
        {

            string mustSpawns = "Must cvak:: ";
            foreach (Vector2Int v2 in zoneObj.mustSpawnLocations)
                mustSpawns = mustSpawns + v2;

            if (zoneObj.mustSpawnLocations.Count == 0 && zoneObj.exitLocations.Count == 0)
            {
                Debug.Log("[Gen] Skipping pathfinding on empty zone");
                return false;
            }

            #region lists


            List<Vector2Int> mustConnect = new List<Vector2Int>();
            mustConnect.AddRange(zoneObj.mustSpawnLocations);

            foreach (Vector2Int loc in zoneObj.exitLocations)
            {
                mustConnect.Add(loc);
            }

            CellData origin;

            // 1) Pick any room as origin (todo prefer mid)
            if (zoneObj.mustSpawnLocations.Count != 0)
            {
                origin = zoneObj.getCellAt(zoneObj.mustSpawnLocations[0]);
                mustConnect.RemoveAt(0);

            }
            else
            {
                origin = zoneObj.getCellAt(zoneObj.exitLocations[0]);
            }




            #endregion lists

            // 3) Find shortest path to rest of rooms, prefer traversing already generated paths. (Cheaper to traverse existing paths)

            #region Pathfinder 

            for (int x = 0; x < mustConnect.Count; x++)
            {
                // We dont want to traverse onto self
                if (origin.loc == mustConnect[x])
                    continue;

                /*
                if (origin.getRoom().getData().name == "Exit_2Hallway")
                {
                    Debug.Log("Trace");
                    throw (new Exception());
                    List<CellData> kids = origin.getReachableNeighbours(true);
                }
                */

                // This is a hack to refresh assigned zones after copying data from failed generation attempts
                origin.setAssignedZone(zoneObj);

                List<CellData> path = VectorUtils.getPath(origin, zoneObj.getCellAt(mustConnect[x]), zoneObj);



                // Failure should never happen as we check validy with BFS prior! Report exception.
                if (path == null)
                {
                    throw new Exception("[Gen] A* Pathfinding failed from: " + origin + " to " + mustConnect[x] + " / " + zoneObj.name + ", This is not an expected behavior and is a bug. Please report it alongside your map seed and configuration.");
                }

                // Mark the path!
                for (int i = 0; i < path.Count(); i++)
                {
                    // Get relation from current room to the next, then mark appropriately.
                    Direction d;

                    // If we have a room ahead on our path
                    if (i < path.Count - 1)
                    {
                        // Path node
                        d = VectorUtils.getRelation(path[i].loc, path[i + 1].loc);

                        // Based on relation between rooms we:
                        switch (d)
                        {
                            // Boolean array [N,W,E,S]
                            case Direction.NORTH_UP:
                                {
                                    // Exit is NORTH
                                    path[i].ph_exits[0] = true;
                                    break;
                                }

                            case Direction.WEST_LEFT:
                                {
                                    // LEFT
                                    path[i].ph_exits[1] = true;
                                    break;
                                }

                            case Direction.EAST_RIGHT:
                                {
                                    path[i].ph_exits[2] = true;
                                    break;
                                }

                            case Direction.SOUTH_DOWN:
                                {
                                    path[i].ph_exits[3] = true;
                                    break;
                                }
                        }
                    }


                    // Mark Path backward too here (assuming we arent the 0th node)
                    if (i != 0 && path.Count > 1)
                    {
                        // Debug.Log("Origin " + origin.loc + "Path count " + path.Count + "/" + path[path.Count-1].loc + " start " + (path.Count() - 1) + " end " + (path.Count - 2));
                        d = VectorUtils.getRelation(path[i].loc, path[i - 1].loc);

                        switch (d)
                        {
                            // Boolean array [N,W,E,S]
                            case Direction.NORTH_UP:
                                {
                                    path[i].ph_exits[0] = true;
                                    break;
                                }

                            case Direction.WEST_LEFT:
                                {
                                    // 
                                    path[i].ph_exits[1] = true;
                                    break;
                                }

                            case Direction.EAST_RIGHT:
                                {
                                    path[i].ph_exits[2] = true;
                                    break;
                                }

                            case Direction.SOUTH_DOWN:
                                {
                                    path[i].ph_exits[3] = true;
                                    break;
                                }
                        }

                    }

                    path[i].ph_travelCost = zoneObj.traversingExistingPathsCost;

                    // Mark pathway and cache it in Zone object for quicker referencing.
                    if (path[i].containerType != containerType.ROOM && path[i].containerType != containerType.EXIT)
                    {
                        path[i].containerType = containerType.RESERVED;
                        zoneObj.pathLocation.Add(path[i].loc);
                    }

                    // If we are a room, get neighbours and attach them
                    if (path[i].containerType == containerType.ROOM)
                    {

                        foreach (CellData cD in path[i].getReachableNeighbours(true))
                        {
                            // If we are already referenced
                            if (path.Contains(cD))
                            {
                                continue;
                            }

                            // Debug.Log("Hello world. at" + cD.loc);

                            switch (VectorUtils.getRelation(path[i].loc, cD.loc))
                            {
                                case Direction.NORTH_UP:
                                    {
                                        // Exit is down
                                        path[i].ph_exits[0] = true;
                                        cD.ph_exits[3] = true;
                                        break;
                                    }

                                case Direction.WEST_LEFT:
                                    {
                                        // 
                                        path[i].ph_exits[1] = true;
                                        cD.ph_exits[2] = true;
                                        break;
                                    }

                                case Direction.EAST_RIGHT:
                                    {
                                        path[i].ph_exits[2] = true;
                                        cD.ph_exits[1] = true;
                                        break;
                                    }

                                case Direction.SOUTH_DOWN:
                                    {
                                        path[i].ph_exits[3] = true;
                                        cD.ph_exits[0] = true;
                                        break;
                                    }
                            }

                            zoneObj.pathLocation.Add(cD.loc);

                        }

                    }


                }

                foreach (CellData cD in zoneObj.cellGrid)
                {
                    cD.ph_resetAll();
                }

            }


            #endregion Pathfinder


            // 4) Fill unused exits with ENDROOMS? 



            // DONE

            return false;

        }






    }
}
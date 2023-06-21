using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using ALOB.Editor;

namespace ALOB.Map
{
    /// <summary>
    /// Class for handling all logic for room generation.
    /// </summary>
    public class NewGen : MonoBehaviour
    {

        #region Variables

        [Header("Setup")]

        /// <summary>
        /// Contains an map setting preset to generate from.
        /// </summary>
        public GeneratorMapPreset gMP;

        /// <summary>
        /// Contains 2D Matrix of all zones present.
        /// </summary>
        Zone[,] zoneGrid;

        [Header("Utils")]
        public int maxIterationsBeforeFallback = 8;

        public System.Random randomGen;

        /// <summary>
        /// Contains reference to the parent node (GameObject) under which all the rooms and zones will belong
        /// </summary>
        private GameObject mapParent;

        private string mapHash;

        [Header("Gizmo")]
        public bool debugGizmo = false;
        public bool disableGizmoText = false;

        [Header("Process (Debugs)")]
        public bool disableBFS = false;
        public bool disableAStar = false;
        public bool enableLargeRoomsFillPath = false;
        public bool disableSpawn = false;
        public bool disablePathRoomPlacement = false;
        public bool cleanUpOnFail = true;



        #endregion


        /// <summary>Generates and spawns a new map with given seed.
        /// </summary>
        /// <param name="mapSeed">Seed the map will generate with.</param>
        public bool generateMap(int mapSeed)
        {

            // Clean up
            CleanUp();

            // Measure
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            // Generate random seed
            randomGen = new System.Random(mapSeed);

            MapGenLogger.Log("<color=blue>[Gen] Started generating map seed: " + mapSeed + "</color>");


            // Prevent spam of new parents on new generation
            if (mapParent == null)
                mapParent = new GameObject("GeneratorParent");


            // Move zones from editor array into our coordinate array which we work with using the modules below.
            placeZones();

            // - - - MODULES FOR ZONEGRID BEFORE ROOM SPAWN

            // Generate grids for each zone
            MM_GridGen MM_GG = new MM_GridGen(randomGen, gMP);
            MM_GG.OnPrepareZones(zoneGrid);


            // Generate exits for each zone
            MM_Connector MM_C = new MM_Connector(randomGen, gMP);
            MM_C.OnPrepareZones(zoneGrid);


            // - - - MODULES FOR ROOM SPAWNS PER GRID

            // Populate each zone with mustSpawn rooms on the grid.
            for (int x = 0; x < zoneGrid.GetLength(0); x += 1)
            {
                for (int y = 0; y < zoneGrid.GetLength(1); y += 1)
                {
                    if (zoneGrid[x, y] == null)
                    {
                        continue;
                    }

                    // Populate exit cells
                    bool failed = new MM_ConPlacer(randomGen, gMP, false, maxIterationsBeforeFallback).PerZoneModule(ref zoneGrid[x, y]);

                    // Populates the grid with containers for rooms which must spawn.
                    if(!failed)
                    failed = new MM_MustSpawnRooms(randomGen, gMP, disableBFS, maxIterationsBeforeFallback).PerZoneModule(ref zoneGrid[x, y]);

                    // Find paths to connect all rooms together
                    if (!disableAStar && !failed)
                        failed = new MM_PathfindConnect(randomGen, gMP).PerZoneModule(ref zoneGrid[x, y]);

                    // Place viable room candidates along the pathfinding path from previous step
                    if (!failed && !disablePathRoomPlacement)
                        failed = new MM_PlaceRoomsOnPath(randomGen, gMP, enableLargeRoomsFillPath).PerZoneModule(ref zoneGrid[x, y]);

                    if (failed)
                    {
                        throw (new Exception("Generation failed for " + zoneGrid[x, y].name + ", try to increase the map size or lower the occupied rooms amount to reduce the chances of spawn errors. You can also try and increase the amount of tries to force the generator to try harder."));

                        if (cleanUpOnFail)
                            CleanUp();
                        return false;
                    }
                }
            }

            // Do a spawn pass separately after all zones are configd correctly
            for (int x = 0; x < zoneGrid.GetLength(0); x += 1)
            {
                for (int y = 0; y < zoneGrid.GetLength(1); y += 1)
                {
                    if (zoneGrid[x, y] == null)
                    {
                        continue;
                    }

                    bool failed = false;

                    // Spawn room assets
                    if (!disableSpawn && !failed)
                        failed = spawnRoomsAll(ref zoneGrid[x, y]);

                    if (failed)
                    {
                        throw (new Exception("Spawning room assets failed for " + zoneGrid[x, y].name + ", "));

                        if (cleanUpOnFail)
                            CleanUp();
                        return false;
                    }
                }
            }




            // We are done here.
            watch.Stop();
            MapGenLogger.Log("<color=green>[Gen] Finished generating at " + watch.ElapsedMilliseconds + " ms. for seed " + mapSeed + "</color>");

            return true;

        }


        /// <summary>
        /// Place all zones which are defined into the map queue
        /// </summary>
        public void placeZones()
        {
            // Grid(List) of all spawned zones
            zoneGrid = new Zone[gMP.zoneLayout.GridSize.x, gMP.zoneLayout.GridSize.y];

            for (int y = 0; y < gMP.zoneLayout.GridSize.y; y++)
            {
                for (int x = 0; x < gMP.zoneLayout.GridSize.x; x++)
                {
                    foreach (Zone zn in gMP.zones)
                    {
                        // ! Invert YX axis as array display is shown by default in TOP to BOTTOM in editor
                        if (gMP.zoneLayout.GetCells()[(gMP.zoneLayout.GridSize.y - 1) - y, x] == zn.name)
                        {
                            zoneGrid[x, y] = zn.Clone() as Zone;
                            zoneGrid[x, y].setGlobalLocation(new Vector2Int(x, y));

                            // List of must spawn rooms. We reset it here in case it was saved dirty last generation.
                            zoneGrid[x, y].mustSpawnLocations = new List<Vector2Int>();
                            break;
                        }
                    }
                }
            }
        }




        /// <summary>
        /// Spawns all containers and rooms
        /// </summary>
        /// <param name="zoneObj"></param>
        /// <returns></returns>
        public bool spawnRoomsAll(ref Zone zoneObj)
        {

            float offsetX = (zoneObj.getGridSizeX() * gMP.spacing * zoneObj.getGlobalLocation().x);
            float offsetY = (zoneObj.getGridSizeY() * gMP.spacing * zoneObj.getGlobalLocation().y);

            HashSet<Vector2Int> RoomsToSpawn = new HashSet<Vector2Int>();
            RoomsToSpawn.UnionWith(zoneObj.mustSpawnLocations);
            RoomsToSpawn.UnionWith(zoneObj.pathLocation);


            foreach (Vector2Int location in RoomsToSpawn)
            {

                CellData cD = zoneObj.getCellAt(location);
                GameObject container = new GameObject(cD.loc.ToString() + "(" + cD.getRoom().getData().name + "/" + zoneObj.name + ")");

                Vector3 loc = new Vector3(transform.position.x + offsetX + (cD.loc.x * gMP.spacing) + gMP.spacing / 2, transform.position.y, transform.position.z + offsetY + (cD.loc.y * gMP.spacing) + gMP.spacing / 2);

                container.transform.SetParent(mapParent.transform);
                container.transform.position = loc;

                cD.spawnedContainer = container;
                if (cD.getRoom() != null)
                {
                    cD.LoadRoom();
                }
                else
                {
                    // Assign missing 
                    MapGenLogger.Log("Room missing!! at " + cD);
                }
            }

            return false;

        }


        #region Utilities




        /// <summary>
        /// Begins to clean and reset some runtime variables so the map can be properly regenerated at runtime. 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public void CleanUp()
        {
            Destroy(mapParent);

            // Reset Cache
            if (zoneGrid != null)
                foreach (Zone z in zoneGrid)
                {
                    if (z != null)
                    {
                        z.pathLocation.Clear();
                        z.exitLocations.Clear();
                        z.mustSpawnLocations.Clear();
                    }
                }


            // Cleanup connection redundant data
            foreach (zoneConnector zC in gMP.connections)
            {
                zC.setConnectorsFrom(null);
                zC.setConnectorsTo(null);
            }

            // Reset ZONES
            zoneGrid = null;

        }




        #endregion


        #region DebugEditor


        void OnDrawGizmos()
        {
            if (!debugGizmo)
                return;

            if (zoneGrid != null)
                foreach (Zone z in zoneGrid)
                {

                    Gizmos.color = new Color(1, 1, 1, 0.5f);
                    if (z != null)
                    {

                        float offsetX = (z.getGridSizeX() * gMP.spacing * z.getGlobalLocation().x);
                        float offsetY = (z.getGridSizeY() * gMP.spacing * z.getGlobalLocation().y);
                        Gizmos.DrawWireCube(new Vector3(transform.position.x + (gMP.gridSizeX * gMP.spacing * z.getGlobalLocation().x) + (gMP.gridSizeX * gMP.spacing) / 2, transform.position.y, transform.position.z + ((gMP.gridSizeX * gMP.spacing * z.getGlobalLocation().y) + (gMP.gridSizeX * gMP.spacing) / 2)), new Vector3(gMP.gridSizeX * gMP.spacing - 1, gMP.spacing, gMP.gridSizeX * gMP.spacing - 1));

                        foreach (CellData cD in z.cellGrid)
                        {
                            String name = cD.loc.ToString();

                            switch (cD.containerType)
                            {

                                case containerType.EMPTY:
                                    {
                                        Gizmos.color = new Color(1, 0, 0, 0.4f);
                                        Gizmos.DrawWireCube(new Vector3(transform.position.x + offsetX + (cD.loc.x * gMP.spacing) + gMP.spacing / 2, transform.position.y, transform.position.z + offsetY + (cD.loc.y * gMP.spacing) + gMP.spacing / 2), new Vector3(gMP.spacing - 1, 1, gMP.spacing - 1));
                                        break;
                                    }
                                case containerType.EXIT:
                                    {
                                        Gizmos.color = new Color(0, 1, 0, 0.2f);
                                        break;
                                    }
                                case containerType.ROOM:
                                    {

                                        if (cD.getRoom() != null)
                                        {
                                            if (cD.getRoom().getData().isExit)
                                            {
                                                Gizmos.color = new Color(0, 1, 1, 0.1f);
                                            }
                                            else if (cD.getRoom().getData().mustSpawn)
                                            {
                                                Gizmos.color = new Color(0.2f, 0.2f, 0.8f, 0.1f);
                                            }
                                            else
                                            {
                                                Gizmos.color = new Color(0, 0, 1, 0.1f);
                                            }
                                            
                                            // TEXT RENDERING
                                            name = cD.loc + "/" + cD.getRoom().getData().name + "\n" + cD.getRoom().angle;

                                        } else
                                        {
                                            Gizmos.color = new Color(1, 0, 0, 0.1f);
                                            name = cD.loc.ToString();
                                        }


                                        
                                        break;
                                    }
                                case containerType.RESERVED:
                                    {
                                        Gizmos.color = new Color(0, 1, 1, 0.4f);
                                        Gizmos.DrawWireCube(new Vector3(transform.position.x + offsetX + (cD.loc.x * gMP.spacing) + gMP.spacing / 2, transform.position.y, transform.position.z + offsetY + (cD.loc.y * gMP.spacing) + gMP.spacing / 2), new Vector3(gMP.spacing - 1, 1, gMP.spacing - 1));
                                        break;
                                    }
                                case containerType.BLOCKED:
                                    {
                                        Gizmos.color = new Color(0.92f, 0.92f, 0.92f, 0.4f);
                                        Gizmos.DrawWireCube(new Vector3(transform.position.x + offsetX + (cD.loc.x * gMP.spacing) + gMP.spacing / 2, transform.position.y, transform.position.z + offsetY + (cD.loc.y * gMP.spacing) + gMP.spacing / 2), new Vector3(gMP.spacing - 1, 1, gMP.spacing - 1));
                                        break;
                                    }
                            }

                            if (!disableGizmoText)
                                GizmosUtils.DrawText(GUI.skin, name, new Vector3(transform.position.x + offsetX + (cD.loc.x * gMP.spacing) + gMP.spacing / 2, transform.position.y, transform.position.z + offsetY + (cD.loc.y * gMP.spacing) + gMP.spacing / 2), Color.white);

                            if (cD.containerType != containerType.EMPTY && cD.containerType != containerType.RESERVED)
                                Gizmos.DrawCube(new Vector3(transform.position.x + offsetX + (cD.loc.x * gMP.spacing) + gMP.spacing / 2, transform.position.y, transform.position.z + offsetY + (cD.loc.y * gMP.spacing) + gMP.spacing / 2), new Vector3(gMP.spacing - 1, gMP.spacing, gMP.spacing - 1));

                            if (cD.containerType == containerType.ROOM || cD.containerType == containerType.RESERVED && cD.ph_exits != null)
                            {
                                Gizmos.color = Color.yellow;

                                Vector3 center = new Vector3(transform.position.x + offsetX + (cD.loc.x * gMP.spacing) + gMP.spacing / 2, transform.position.y, transform.position.z + offsetY + (cD.loc.y * gMP.spacing) + gMP.spacing / 2);

                                //NWES


                                // EXIT NORTH?
                                if (cD.ph_exits[0])
                                {
                                    Gizmos.DrawLine(center, center + new Vector3(0, 0, 10));
                                }

                                // WEST
                                if (cD.ph_exits[1])
                                {
                                    Gizmos.DrawLine(center, center + new Vector3(-10, 0, 0));
                                }

                                // EAST
                                if (cD.ph_exits[2])
                                {
                                    Gizmos.DrawLine(center, center + new Vector3(10, 0, 0));
                                }

                                // SOUTH
                                if (cD.ph_exits[3])
                                {
                                    Gizmos.DrawLine(center, center + new Vector3(0, 0, -10));
                                }


                            }


                        }
                    }
                }
        }

        #endregion

    }
}
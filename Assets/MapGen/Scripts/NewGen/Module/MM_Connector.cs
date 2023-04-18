using ALOB.Map;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Reserves area for exits of each zone
/// </summary>
public class MM_Connector : MapGenModule, IGenBeforeSpawn
{
    public MM_Connector(System.Random randomGen, GeneratorMapPreset gMP) : base(randomGen, gMP)
    {
    }

    public void OnPrepareZones(Zone[,] zoneGrid)
    {
        foreach (Zone curZone in zoneGrid)
        {
            if (curZone == null)
                continue;

            reserveConnector(curZone, zoneGrid);
        }
    }

    /// <summary>
    /// Reserves area for ZONE EXITS (Connectors between zones)
    /// </summary>
    /// <param name="zoneObj"></param>
    void reserveConnector(Zone zoneObj, Zone[,] zoneGrid)
    {
        // Get connections which we require and assign them into grid!
        foreach (zoneConnector zC in gMP.connections)
        {
            if (zC.toZone == zoneObj.name || zC.fromZone == zoneObj.name)
            {
                // Check for existing connections assuming other zones were spawned
                if (zC.getConnectorsFrom() != null && zC.getConnectorsFrom().Length != 0)
                {
                    // There is no need for connectors since they were auto-marked by the previous zones.
                }
                else
                {
                    // Randomly select "n" amount of exits
                    int connectionCount = randomGen.Next(1, zC.maxAmountOfConnections);

                    // Find the zone we are to connect to
                    foreach (Zone refZone in zoneGrid)
                    {
                        // We want to connect to refZone from zoneObj
                        if (refZone != null && refZone.name == zC.toZone && zoneObj.name == zC.fromZone)
                        {
                            // Debug.Log("Create connection from " + zoneObj.name + " to " + refZone.name);

                            // - - - Get the relative direction the other zone is at
                            
                            // Target Y
                            int refCellIndex = 0;
                            // Ours Y
                            int curCellIndex = 0;
                            // Vertical or Horizontal?
                            int horizontalSelector = 0;

                            // This part is to pick "n" rooms we mark as exits
                            List<CellData> cD = new List<CellData>();
                            Direction d = VectorUtils.getRelation(zoneObj.getGlobalLocation(), refZone.getGlobalLocation());

                            zC.setZoneFacing(d);

                            // This part is used to determine whether to use first/last column/row
                            switch (d)
                            {
                                case Direction.SOUTH_DOWN:
                                    {
                                        // We want horizontal or vertical? (0 - Horizontal)
                                        horizontalSelector = 1;
                                        // Pick from
                                        refCellIndex = zoneObj.cellGrid.GetLength(0) - 1;
                                        // Place to
                                        curCellIndex = 0;
                                        break;
                                    }
                                case Direction.NORTH_UP:
                                    {
                                        horizontalSelector = 1;
                                        refCellIndex = 0;
                                        curCellIndex = zoneObj.cellGrid.GetLength(0) - 1;
                                        break;
                                    }
                                case Direction.WEST_LEFT:
                                    {
                                        horizontalSelector = 0;
                                        refCellIndex = zoneObj.cellGrid.GetLength(1) - 1;
                                        curCellIndex = 0;
                                        break;
                                    }
                                case Direction.EAST_RIGHT:
                                    {
                                        horizontalSelector = 0;
                                        refCellIndex = 0;
                                        curCellIndex = zoneObj.cellGrid.GetLength(1) - 1;
                                        break;
                                    }
                                default: break;
                            }


                            // Pick available rooms from the row/column defined previously
                            for (int i = 0; i < zoneObj.cellGrid.GetLength(horizontalSelector); i += 1)
                            {
                                // Horizontal
                                if (horizontalSelector == 0)
                                {
                                    if (zoneObj.cellGrid[curCellIndex, i].containerType == containerType.EMPTY)
                                    {
                                        cD.Add(zoneObj.cellGrid[curCellIndex, i]);
                                    }
                                }
                                // Vertical
                                else
                                {
                                    if (zoneObj.cellGrid[i, curCellIndex].containerType == containerType.EMPTY)
                                    {
                                        cD.Add(zoneObj.cellGrid[i, curCellIndex]);
                                    }
                                }
                            }


                            // Shuffle the candidates for exits
                            CellData[] cD2 = (CellData[])Shuffler.Shuffle(cD.ToArray(), randomGen);
                            cD.Clear();

                            PlacementValidator validator = new PlacementValidator(zoneObj);


                            /*
                            // Pick N rooms from shuffle and mark for exit, add these to the zConnector for caching
                            int roomIndexLoop = 0;
                            int index = 0;
                            while (roomIndexLoop < connectionCount)
                            {

                                // Fallback if we fail to generate on exitCount *2 tries
                                index++;
                                if (index > connectionCount*2 && zoneObj.exitLocations.Count > 1)
                                {
                                    break;
                                }

                                // Make sure we arent close to existing exit:
                                if (!validator.ExitPlacementCheck(cD2[roomIndexLoop]))
                                    continue;



                                cD2[roomIndexLoop].containerType = containerType.EXIT;
                                zoneObj.addExitLocation(cD2[roomIndexLoop]);
                                // Debug.Log("[Gen] Made exit for " + zoneObj.name + " at " + cD2[i].loc + " (" + zC.toZone + ")");
                                cD.Add(cD2[roomIndexLoop]);

                                roomIndexLoop++;
                            }
                            zC.setConnectorsFrom(zoneObj.getExitLocations());
                            */

                            // Pick N rooms from shuffle and mark for exit, add these to the zConnector for caching
                            

                            int roomIndexLoop = 0;
                            foreach (CellData exitCandidate in cD2)
                            {
                                // Make sure we arent close to existing exit:
                                if (!validator.ExitPlacementCheck(exitCandidate))
                                    continue;

                                if(roomIndexLoop ==  connectionCount)
                                {
                                    break;
                                }

                                exitCandidate.containerType = containerType.EXIT;
                                zoneObj.addExitLocation(exitCandidate);
                                // Debug.Log("[Gen] Made exit for " + zoneObj.name + " at " + cD2[i].loc + " (" + zC.toZone + ")");
                                cD.Add(exitCandidate);
                                roomIndexLoop++;
                            }

                            zC.setConnectorsFrom(zoneObj.getExitLocations());


                            // - - - Lets plant actual rooms into the current zone candidates!
                            /*
                            List<catalogueEntry> roomList = zoneObj.roomCatalogue.Where(x => x.room.isExit).ToList();
                            if(roomList.Count == 0)
                            {
                                Debug.Log("[GEN] Exit spawner will fail because no rooms were marked as exits");
                            }

                            MM_MustSpawnRooms exitSpawner = new MM_MustSpawnRooms(randomGen, gMP, false, 32);
                            
                            foreach (CellData cells in zC.getConnectorsFrom())
                            {
                                // Based on where the above zone is, well need to rotate the exit. The default exit location when facing north is DOWN for all the rooms.
                                ALOB.Map.Room r = exitSpawner.placeRoomImportant(zoneObj, roomList[randomGen.Next(0, roomList.Count - 1)].room, cells, zC.getConnectorsFrom().ToList());
                            }

                            */

                            // Find the corresponding exits in the zone we are connecting to and mark the rooms there
                            foreach (CellData connector in cD)
                            {
                                // Vertical
                                if (horizontalSelector == 1)
                                {
                                    for (int x = 0; x < refZone.cellGrid.GetLength(0); x += 1)
                                    {
                                        if (refZone.cellGrid[x, refCellIndex].loc.x == connector.loc.x)
                                        {
                                            refZone.cellGrid[x, refCellIndex].containerType = containerType.EXIT;
                                            refZone.addExitLocation(refZone.cellGrid[x, refCellIndex]);
                                        }
                                    }
                                }

                                // Horizontal
                                else
                                {
                                    for (int y = 0; y < refZone.cellGrid.GetLength(1); y += 1)
                                    {
                                        if (refZone.cellGrid[refCellIndex, y].loc.y == connector.loc.y)
                                        {
                                            refZone.cellGrid[refCellIndex, y].containerType = containerType.EXIT;
                                            refZone.addExitLocation(refZone.cellGrid[refCellIndex, y]);
                                        }
                                    }
                                }
                            }

                            zC.setConnectorsTo(refZone.getExitLocations());


                            // - - - Lets plant actual rooms into the ref zone candidates!
                            /*

                            roomList = refZone.roomCatalogue.Where(x => x.room.isExit).ToList();
                            if (roomList.Count == 0)
                            {
                                Debug.Log("[GEN] Exit spawner will fail because no rooms were marked as exits");
                            }

                            exitSpawner = new MM_MustSpawnRooms(randomGen, gMP, false, 32);

                            foreach (CellData cells in refZone.getExitLocations().ToList())
                            {
                                // Based on where the above zone is, well need to rotate the exit. The default exit location when facing north is DOWN for all the rooms.
                                ALOB.Map.Room r = exitSpawner.placeRoomImportant(refZone, roomList[randomGen.Next(0, roomList.Count - 1)].room, cells, refZone.getExitLocations().ToList());
                            }

                            */



                        }
                    }
                }
            }
        }
    }




}

using ALOB.Map;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CellsUtils
{


    /// <summary>
    /// Randomly pick an empty cell from a given list 
    /// </summary>
    public static CellData getRandomEmptyCell(List<CellData> candidates, System.Random randomGen, List<Vector2Int> exclude = null)
    {

        Shuffler.Shuffle(candidates, randomGen);

        foreach (CellData cD in candidates)
        {
            if (cD.containerType == containerType.EMPTY || cD.containerType == containerType.RESERVED)
            {
                bool excluded = false;

                // Search excluded filter
                if (exclude != null || exclude.Count != 0)
                {
                    foreach (Vector2Int v2 in exclude)
                    {
                        if (cD.loc == v2)
                        {
                            excluded = true;
                            break;
                        }
                    }
                }

                if (!excluded)
                {
                    return cD;
                }
            }
        }

        // Debug.Log("Chosen " + "null" + " count " + candidates.Count);
        return null;
    }


    /// <summary>
    /// Pick cells which we will place the rooms into. (Implements filters here if needed, defaults no filters.)
    /// </summary>
    /// <param name="zoneObj"></param>
    public static List<CellData> getCellPrimeCandidates(Zone zoneObj)
    {

        // List of possible positions we can put rooms into
        List<CellData> mustHavesCandidates = new List<CellData>();

        //Former code excluded cells in a pattern so that every room is 1 cell apart from each other and the map boundaries.
        // Removed

        // Add all cells which are empty to candidates.
        foreach (CellData cD in zoneObj.cellGrid)
        {
            if (cD.containerType == containerType.EMPTY || cD.containerType == containerType.RESERVED)
                mustHavesCandidates.Add(cD);
        }

        return mustHavesCandidates;
    }


    /// <summary>
    /// Get a list of rooms which must spawn in the given zone. Sorted by the biggest room.
    /// </summary>
    /// <param name="zoneObj"></param>
    public static List<RoomData> getMustSpawnListSorted(Zone zoneObj, System.Random randomGen)
    {

        List<RoomData> mustHaves = new List<RoomData>();

        // Get info on rooms for target grid (shuffled)
        foreach (catalogueEntry cEntry in Shuffler.Shuffle(zoneObj.roomCatalogue, randomGen))
        {
            // If a room is marked as MUST SPAWN
            if (cEntry.room.mustSpawn)
            {
                mustHaves.Add(cEntry.room);
                //    Debug.Log("NAME: " + cEntry.room.roomName + " SIZE: " + cEntry.room.getOccupiedCellsSize());
            }
        }

        // In case there are no rooms marked to spawn here, return.
        if (mustHaves.Count == 0)
        {
            Debug.Log("[Gen] There are no rooms marked for spawn. (" + zoneObj.name + ")");
            return null;
        }

        // Sort here from biggest room to smallest
        List<RoomData> SortedList = mustHaves.OrderByDescending(o => o.getOccupiedCellsSize()).ToList();

        /*
        Debug.Log("SORTED:");
        SortedList.ForEach(x=>Debug.Log(x));
        */

        return mustHaves;

    }


}

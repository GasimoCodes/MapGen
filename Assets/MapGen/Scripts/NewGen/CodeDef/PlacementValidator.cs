using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALOB.Map
{
    public class PlacementValidator
    {
        Zone zoneObj;

        public PlacementValidator(Zone zoneObj)
        {
            this.zoneObj = zoneObj;
        }



        public bool PositionFitnessCheckLargeRoom(Room r)
        {
            // Make sure room does not exceed our grid
            foreach (CellData occupiedCells in zoneObj.getExpansionFromRoomGlobal(r))
            {
                // If we exceed outside bounds OR into another room, we try again with different orientation
                if (occupiedCells == null || occupiedCells.containerType != containerType.EMPTY)
                {
                    return false;
                }
            }

            return true;
        }


        public bool ExitsOcclusionCheck(Room r, CellData cDPreset, bool isConnectorRoom = false)
        {
            foreach (CellData cD in zoneObj.getRoomExitCells(cDPreset, false, isConnectorRoom))
            {
                
                // If we are heading to null
                if (cD == null)
                {
                    return false;
                }

                // If we are blocked on any of the exits, we know this rotation is not suitable.
                if (cD.containerType == containerType.BLOCKED)
                {
                    return false;
                }

                // Second pass to check we arent going into anothers room wall
                if (cD.containerType == containerType.ROOM)
                {
                    bool passExitCheck = false;

                    // Get the room exits and check whether we are init.
                    foreach (CellData exCd in zoneObj.getRoomExitCells(cD, true))
                    {
                        // If our cell is the exit:
                        if (cDPreset.loc == exCd.loc)
                        {
                            // Debug.Log("Hello nursey for: " + cDPreset.getRoom().getData().roomName + cDPreset.loc +  " to " + exCd.getRoom().getData().roomName + exCd.loc);
                            passExitCheck = true;
                        }
                    }

                    if (!passExitCheck)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Checks whether spawning our room wont cause another room to lead into wall
        /// </summary>
        /// <param name="r">Room to spawn</param>
        /// <param name="cDPreset">Cell in which room is in</param>
        /// <param name="temp">Cell backup from before the room was added</param>
        /// <returns></returns>
        public bool RoomDoesntOccludeCheck(Room r, CellData cDPreset, CellData temp)
        {
            // If we are on an cell which other room has leading exits into, we must know we arent blocking!
            if (temp.containerType == containerType.RESERVED || temp.containerType == containerType.EXIT)
            {
                // Get all surrounding cells
                foreach (CellData surroundingCell in zoneObj.getSurroundingCells(cDPreset.loc))
                {
                    // Filter cells which are rooms
                    if (surroundingCell == null || surroundingCell.containerType != containerType.ROOM)
                    {
                        continue;
                    }

                    // For each room
                    foreach (CellData surCellExits in zoneObj.getRoomExitCells(cDPreset, true))
                    {
                        // If room has any exits which lead into our cell
                        if (surCellExits.loc == cDPreset.loc)
                        {
                            bool passExitCheck = false;

                            // Check if our exits lead into the room which leads into us
                            foreach (CellData ourExitCells in zoneObj.getRoomExitCells(cDPreset, true))
                            {
                                // if so, we may skip this room and move onto next one
                                if (surroundingCell.loc == ourExitCells.loc)
                                {
                                    passExitCheck = true;
                                    // YAY! We are connected!
                                    break;
                                }
                            }

                            if (!passExitCheck)
                            {
                                return false;
                            }


                            continue;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Check whether pathFindTargets are reachable from cDPreset using BFS.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="cDPreset"></param>
        /// <param name="pathFindTargets"></param>
        /// <returns></returns>
        public int RoomPathfindingCheck(Room r, CellData cDPreset, List<CellData> pathFindTargets)
        {
            bool allPass = true;
            int resCount = 0;

            // Debug.Log("PF For " + cDPreset + " " + r.getData().name);

            // Temporarily set largeRoom blocks for pathproofing.
            if (r.getData().large)
            {
                foreach (CellData occupiedCells in zoneObj.getExpansionFromRoomGlobal(r))
                {
                    if (occupiedCells != null)
                    {
                        occupiedCells.containerType = containerType.BLOCKED;
                    }
                }
            }

            // BFS HERE
            List<Vector2Int> targets = new List<Vector2Int>();
            pathFindTargets.ForEach(x => targets.Add(x.loc));

            List<CellData> res = VectorUtils.BFS_CheckReachibility(cDPreset.loc, targets, zoneObj);
            if (res.Count == 0)
            {
                // OK
            }
            else
            {
                resCount = res.Count;
            }

            // Reset largeRoom blocks
            if (r.getData().large)
            {
                foreach (CellData occupiedCells in zoneObj.getExpansionFromRoomGlobal(r))
                {
                    // If we exceed outside bounds OR into another room, we try again with different orientation
                    if (occupiedCells != null)
                    {
                        occupiedCells.containerType = containerType.EMPTY;
                    }
                }
            }

            return resCount;

        }


        /// <summary>
        /// Checks whether the area surrounding EXIT is empty. Used to space out exits.
        /// </summary>
        /// <param name="cDPreset"></param>
        /// <returns></returns>
        public bool ExitPlacementCheck(CellData cDPreset)
        {

            foreach(CellData cD in zoneObj.getSurroundingCells(cDPreset.loc, true))
            {
                if(cD != null && cD.containerType == containerType.EXIT)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ALOB.Map
{
    /// <summary>
    /// Contains the representation of an existing Game Map Cell
    /// </summary>
    public class CellData : ICloneable
    {
        // Cell representation in map
        public GameObject spawnedContainer;


        public containerType containerType = containerType.EMPTY;
        public Vector2Int loc;

        private Room containerRoom;
        Zone z;

        public Zone getAssignedZone()
        {
            return z;
        }

        public void setAssignedZone(Zone z)
        {
            this.z = z;
        }

        #region pathfinding

        // Travel cost for pathfinding prioritizations. Default for empty cells is 5, for cells with passthrough rooms we lower to 2.
        public int ph_travelCost = 5;
        public int ph_hCost = 0; // Current to end
        public int ph_gCost = 0; // Start to current
        public int ph_fCost
        {
            get
            {
                return ph_hCost + ph_gCost + ph_travelCost; // + travelCost?
            }
        }

        public bool ph_isOnOpenList = false; // Vis. A* Open List, used for caching so we dont iterate.

        public bool ph_isOnClosedList = false; // Vis. A* Closed List, also used for BFS isAlreadyVisted check
        public CellData ph_parent;

        public bool[] ph_exits = new bool[4]; // Where we have exits? Is true when exit is in direction. N W E S. Saved this way to speed iterations.

        public void ph_resetAll()
        {
            ph_hCost = 0;
            ph_gCost = 0;
            ph_isOnClosedList = false;
            ph_isOnOpenList = false;
            ph_parent = null;
        }


        public List<CellData> getReachableNeighbours(bool stripInvalid)
        {

            List<CellData> reachableNeighbours = new List<CellData>();
            List<CellData> temp = new List<CellData>();

            // If we are a room, return exits
            if (containerType == containerType.ROOM)
            {
                // This should not return any nulls since we already did a check for that previously.
                reachableNeighbours.AddRange(z.getRoomExitCells(this, true));
            }
            else
            {
                reachableNeighbours.AddRange(z.getSurroundingCells(loc, true));
            }

            // Copy previous list.
            temp.AddRange(reachableNeighbours.ToArray());

            if (stripInvalid)
            {
                foreach (CellData cD in temp)
                {
                    // If null, is a blocked cell or is self.
                    if (cD == null || cD.containerType == containerType.BLOCKED || cD.loc == this.loc)
                    {
                        reachableNeighbours.Remove(cD);
                    }

                    // Check if its not a wall!

                    else if (cD.containerType == containerType.ROOM)
                    {

                        bool isNotWall = false;

                        // Get exists of destination
                        foreach (CellData cDExt in z.getRoomExitCells(cD, true))
                        {
                            // Compare exits to us, if equal, we are connected.
                            if (cDExt.loc == this.loc)
                            {
                                isNotWall = true;
                                break;
                            }
                        }

                        if (!isNotWall)
                        {
                            reachableNeighbours.Remove(cD);
                        }

                        // End

                    }
                }
            }


            return reachableNeighbours;
        }

        #endregion


        public Room getRoom()
        {
            return this.containerRoom;
        }

        public void setRoom(Room room)
        {
            containerRoom = room;
            this.ph_travelCost = room.getData().pathFinderTravelCost;
            containerType = containerType.ROOM;
            // Cache location in room object.
            room.x = loc.x;
            room.y = loc.y;
        }

        public CellData(containerType type)
        {
            containerType = type;
        }

        public CellData(Vector2Int location, Zone z)
        {
            loc = location;
            this.z = z;
        }

        public CellData(Room room)
        {
            containerRoom = room;
            containerType = containerType.ROOM;
        }


        // Reserved for room streaming

        #region RoomStreaming


        public void LoadRoom()
        {
            if (containerRoom != null && containerRoom.getData() != null)
            {
                // Debug.Log("SPAWN: BEGIN FOR " + containerRoom.getData().roomName);
                Addressables.InstantiateAsync(containerRoom.getData().roomAddr, spawnedContainer.transform).Completed += instantiate_Completed;
            }
        }


        private void instantiate_Completed(AsyncOperationHandle<GameObject> obj)
        {

            containerRoom.spawnedContainer = obj.Result;
            containerRoom.spawnedContainer.AddComponent(typeof(ReleaseAddresableOnDestroy));

            containerRoom.spawnedContainer.transform.localPosition = new Vector3(0, 0, 0);
            containerRoom.spawnedContainer.transform.rotation = Quaternion.Euler(0, containerRoom.getAngleInDegrees(), 0);

            // Room init code?
            // containerRoom.spawnedContainer.GetComponent<Rooms>().InvokeRepeating();

        }


        public void UnloadRoom()
        {

        }

        #endregion RoomStreaming


        public object Clone()
        {
            CellData tempCd = (CellData)this.MemberwiseClone();
            tempCd.loc = new Vector2Int(this.loc.x, this.loc.y);

            if (this.containerRoom != null)
                tempCd.setRoom((Room)this.containerRoom.Clone());

            if (z != null)
                tempCd.z = this.z;

            tempCd.ph_isOnClosedList = this.ph_isOnClosedList;
            tempCd.ph_isOnOpenList= this.ph_isOnOpenList;


            return tempCd;
        }

        public override string ToString()
        {
            string tmp = "<color=gray>Loc: " + loc.ToString() + ", Type: " + containerType + " </color>";

            if (getRoom()  != null)
            {
                tmp += getRoom().getData().name;
                
            }
            
            return tmp;
        }

    }

    /// <summary>
    /// Contains shorthand types for cells on the grid.
    /// </summary>
    public enum containerType
    {
        ROOM,
        EMPTY,
        RESERVED,
        BLOCKED,
        EXIT
    }
}
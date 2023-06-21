using System.Collections.Generic;
using UnityEngine;
using System;

namespace ALOB.Map
{
    /// <summary>
    /// Contains all data and helper methods for managing and storing a Zone map object.
    /// </summary>
    [System.Serializable]
    public class Zone : ICloneable
    {
        public string name = "Generic Map Zone";

        // x(col) y(row) grid
        public CellData[,] cellGrid = new CellData[0, 0];

        // Total spawned exits
        int generatedExits;

        public int traversingExistingPathsCost = 3;

        // For faster grid search we keep the zone location cached here        
        Vector2Int globalLocation;

        // List of all room adepts
        public catalogueEntry[] roomCatalogue;

        #region Caching

        [HideInInspector]
        public List<Vector2Int> mustSpawnLocations = new List<Vector2Int>();

        [HideInInspector]
        public List<Vector2Int> exitLocations = new List<Vector2Int>();

        [HideInInspector]
        public List<Vector2Int> pathLocation = new List<Vector2Int>();

        #endregion Caching

        public Vector2Int getGlobalLocation()
        {
            return globalLocation;
        }

        public void addExitLocation(CellData exit)
        {
            exitLocations.Add(exit.loc);
        }

        public CellData[] getExitLocations()
        {
            List<CellData> cdS = new List<CellData>();
            foreach (Vector2Int v in exitLocations)
                cdS.Add(getCellAt(v));

            return cdS.ToArray();
        }

        public void setGlobalLocation(Vector2Int loc)
        {
            globalLocation = loc;
        }

        public int getGridSizeX()
        {
            return cellGrid.GetLength(0);
        }

        public int getGridSizeY()
        {
            return cellGrid.GetLength(1);
        }

        public string getGridSizeReadable()
        {
            return (cellGrid.GetLength(0) + ", " + cellGrid.GetLength(1));
        }

        public CellData getCellAt(int x, int y)
        {
            // If cell is outside of bounds.
            if (x > getGridSizeX() - 1 || x < 0 || y > getGridSizeY() - 1 || y < 0)
            {
                return null;
            }
            // Debug.Log("REQ " + x + " " + y);
            return cellGrid[x, y];
        }

        // Alias
        public CellData getCellAt(Vector2Int at)
        {
            return getCellAt(at.x, at.y);
        }


        public void setRoomAt(int x, int y, Room r)
        {
            // Assign room to cell
            cellGrid[x, y].setRoom(r);
            cellGrid[x, y].containerType = containerType.ROOM;

            // Mark exits here
            foreach (Vector2Int loc in cellGrid[x, y].getRoom().localExitCells)
            {
                CellData tar = getCellAt(VectorUtils.translateLocalToGlobal(loc, new Vector2Int(x, y)));

                // In case we are exit
                if (tar == null)
                {
                    continue;
                }

                if (tar.containerType == containerType.EMPTY)
                {
                    tar.containerType = containerType.RESERVED;
                }
                else if (tar.containerType == containerType.BLOCKED)
                {
                    Debug.LogWarning("An blocked cell at doors found while trying to spawn " + cellGrid[x, y]+ ": " + tar + " Make sure the room is properly configured.");
                }
            }

            // Cache the room position here so we can find it without querying the entire map later
            mustSpawnLocations.Add(new Vector2Int(x, y));

        }

        public List<CellData> convertMapTo1DArray()
        {
            int index = 0;
            //Getting the no of rows of 2d array 
            int NoOfRows = cellGrid.GetLength(0);
            //Getting the no of columns of the 2d array
            int NoOfColumns = cellGrid.GetLength(1);
            //Creating 1d Array by multiplying NoOfRows and NoOfColumns
            List<CellData> OneDimensionalArray = new List<CellData>();

            //Assigning the elements to 1d Array from 2d array
            for (int y = 0; y < NoOfColumns; y++)
            {
                for (int x = 0; x < NoOfRows; x++)
                {
                    OneDimensionalArray.Add(cellGrid[x, y]);
                    index++;
                }
            }

            return OneDimensionalArray;
        }

        /// <summary>
        /// Unmarks all cells from being traversed
        /// </summary>
        public void unMarkTraversed()
        {
            int index = 0;
            //Getting the no of rows of 2d array 
            int NoOfRows = cellGrid.GetLength(0);
            //Getting the no of columns of the 2d array
            int NoOfColumns = cellGrid.GetLength(1);

            //Assigning the elements to 1d Array from 2d array
            for (int y = 0; y < NoOfColumns; y++)
            {
                for (int x = 0; x < NoOfRows; x++)
                {
                    cellGrid[x, y].ph_isOnClosedList = false;
                    index++;
                }
            }

        }

        /// <summary>
        /// Returns a list of surrounding cells given a cell
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="excludeDiagonals"></param>
        /// <returns></returns>
        public CellData[] getSurroundingCells(Vector2Int loc, bool excludeDiagonals = false)
        {
            int x = loc.x;
            int y = loc.y;

            // Bottom left, bottom mid, bottom right, middle left, middle right, up left, up mid, up right
            CellData[] tempCD = new CellData[8];

            if (cellGrid[x, y] == null)
            {
                Debug.LogWarning("[Gen] Requested list of surrounding cells around (" + x + ", " + y + ") failed - no such cell exists!");
                return null;
            }

            // Debug.Log("[Gen] Requested list of surrounding cells around (" + x + ", " + y + ").");

            // There is no border below us
            if ((y - 1) >= 0)
            {
                // Mid bottom
                tempCD[1] = cellGrid[x, y - 1];

                // There is no border on the left
                if ((x - 1) >= 0 && !excludeDiagonals)
                {
                    // Bottom left
                    tempCD[0] = cellGrid[x - 1, y - 1];
                }

                // There is no border on the right
                if ((x + 1) < cellGrid.GetLength(0) && !excludeDiagonals)
                {
                    // Bottom right
                    tempCD[2] = cellGrid[x + 1, y - 1];
                }
            }

            // Middle Row, check left right 
            if ((x - 1) >= 0)
            {
                // Middle left
                tempCD[3] = cellGrid[x - 1, y];
            }
            if ((x + 1) < cellGrid.GetLength(0))
            {
                // Middle right
                tempCD[4] = cellGrid[x + 1, y];
            }

            // There is no border above us
            if ((y + 1) < cellGrid.GetLength(1))
            {
                // Top mid
                tempCD[6] = cellGrid[x, y + 1];

                // There is no border on the left
                if ((x - 1) >= 0 && !excludeDiagonals)
                {
                    // Top left
                    tempCD[5] = cellGrid[x - 1, y + 1];
                }

                // There is no border on the right
                if ((x + 1) < cellGrid.GetLength(0) && !excludeDiagonals)
                {
                    // Top right
                    tempCD[7] = cellGrid[x + 1, y + 1];
                }
            }

            return tempCD;
        }


        /// <summary>
        /// Returns exit cells of cell located in givenCelldata
        /// </summary>
        /// <param name="cD"></param>
        /// <returns></returns>
        public CellData[] getRoomExitCells(CellData cD, bool filterNulls = false, bool isExitStrip = false)
        {

            if (cD.getRoom() == null)
            {
                Debug.LogWarning("[Gen] Cannot get exit cells for room at " + cD + " as no room has been added to the cell. (" + this.name + ")");
                return null;
            }

            Room r = cD.getRoom();

            // Now we seek the rooms available
            List<CellData> cDs = new List<CellData>();


            int i = 0;
            foreach (Vector2Int obj in VectorUtils.translateLocalToGlobal(r.localExitCells, cD.loc))
            {

                // If not null and self
                if (getCellAt(obj.x, obj.y) != null)
                {
                    cDs.Add(cellGrid[obj.x, obj.y]);
                }
                else
                {
                    // If we include nulls
                    if (!filterNulls)
                    {
                        // Check if we are an exit room, it deserves special attention here
                        if (isExitStrip)
                        {
                            if (r.angle == VectorUtils.getRelation(cD.loc, obj))
                            {
                                // Ignore this cell as its actually connected to another zone
                                continue;
                            }
                        }

                        cDs.Add(null);

                    }
                }

                i++;

            }

            return cDs.ToArray();
        }


        /// <summary>
        /// Returns occupied cells of a given room
        /// </summary>
        /// <returns></returns>
        public CellData[] getExpansionFromRoomGlobal(Room r)
        {

            List<CellData> converted = new List<CellData>();

            foreach (Vector2Int vInt in r.expandRelativeToOrigin)
            {
                converted.Add(getCellAt(VectorUtils.translateLocalToGlobal(new Vector2Int(vInt.x, vInt.y), new Vector2Int(r.x, r.y))));
            }

            return converted.ToArray();
        }

        /// <summary>
        /// Clones the Zone Object
        /// </summary>
        /// <returns></returns>
        public System.Object Clone()
        {


            Zone z = this.MemberwiseClone() as Zone;
            z.name = this.name;


            
            

            
			z.exitLocations = new List<Vector2Int>();
			z.exitLocations.AddRange(this.exitLocations);
			

            // Set the grid
            z.cellGrid = new CellData[getGridSizeX(), getGridSizeY()];
            z.roomCatalogue = roomCatalogue.Clone() as catalogueEntry[];


            // for each cell in grid (x y)
            for (int x = 0; x < z.cellGrid.GetLength(0); x += 1)
            {
                for (int y = 0; y < z.cellGrid.GetLength(1); y += 1)
                {
                    // Target 
                    CellData refCell = getCellAt(x, y);

                    // New
                    CellData cD = new CellData(new Vector2Int(x, y), z);
                    cD.containerType = refCell.containerType;

                    if (refCell.getRoom() != null)
                    {
                        Room rRef = refCell.getRoom();
                        Room r = rRef.Clone() as Room;
                        r.localExitCells = rRef.localExitCells.Clone() as Vector2Int[];

                        if (rRef.angle != Direction.NORTH_UP)
                        {
                            while (true)
                            {
                                if (rRef.angle != r.angle)
                                {
                                    // Improve later
                                    r.RotateClockwise(1);

                                }
                                else
                                {
                                    break;
                                }

                            }
                        }
                    }

                    // Write positions into the grid for improved performance with caching
                    z.cellGrid[x, y] = cD;
                }
            }



            return z;
        }

        public override string ToString()
        {
            return "Name: " + this.name + " Global Location: " + this.globalLocation + " RoomCatalogue: " + this.roomCatalogue + " Cells: ";
        }

    }
}
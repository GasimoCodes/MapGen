using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALOB.Map
{
    public class MM_GridGen : MapGenModule, IGenBeforeSpawn
    {
        public MM_GridGen(System.Random randomGen, GeneratorMapPreset gMP) : base(randomGen, gMP)
        {
        }


        public void OnPrepareZones(Zone[,] zoneGrid)
        {
            foreach (Zone zoneObj in zoneGrid)
            {
                if (zoneObj == null)
                    continue;

                // Set the grid
                zoneObj.cellGrid = new CellData[gMP.gridSizeX, gMP.gridSizeX];

                // for each cell in grid (x y)
                for (int x = 0; x < zoneObj.cellGrid.GetLength(0); x += 1)
                {
                    for (int y = 0; y < zoneObj.cellGrid.GetLength(1); y += 1)
                    {
                        // Write positions into the grid for improved performance with caching
                        zoneObj.cellGrid[x, y] = new CellData(new Vector2Int(x, y), zoneObj);

                        // Re-Init Caching Lists 
                        zoneObj.exitLocations = new List<Vector2Int>();
                        zoneObj.mustSpawnLocations = new List<Vector2Int>();
                        zoneObj.pathLocation = new List<Vector2Int>();
                    }
                }

            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALOB.Map
{

    /// <summary>
    /// Defines connections between zones.
    /// </summary>
    [System.Serializable]
    public class zoneConnector
    {
        public string fromZone;
        public string toZone;

        [Range(1, 10)]
        public int maxAmountOfConnections;

        // 0 means we spawned no connections yet, higher number means connections already exist
        CellData[] exitsFrom;
        CellData[] exitsTo;

        public CellData[] getConnectorsFrom()
        {
            return exitsFrom;
        }

        public CellData[] getConnectorsTo()
        {
            return exitsTo;
        }

        public void setConnectorsFrom(CellData[] exitArray)
        {
            exitsFrom = exitArray;
        }
        public void setConnectorsTo(CellData[] exitArray)
        {
            exitsTo = exitArray;
        }


        Direction zoneFacing;

        public Direction getZoneFacing()
        {
            return zoneFacing;
        }

        public void setZoneFacing(Direction zoneFacing)
        {
            this.zoneFacing = zoneFacing;
        }

    }
}
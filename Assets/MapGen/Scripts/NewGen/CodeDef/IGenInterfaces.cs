using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALOB.Map
{
    public interface IGenBeforeSpawn
    {
        void OnPrepareZones(Zone[,] zoneGrid);
    }

    public interface IGenRoomSpawn
    {
        bool PerZoneModule(ref Zone curZone);
    }

}

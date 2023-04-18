using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;


namespace ALOB.Map
{
    public class MapStreamingMGR : MonoBehaviour
    {

        public NewGen mapGen;
        int cullDistance;
        int streamDistance;

        /// <summary>
        /// Updates Player position for the streaming and occlusion
        /// </summary>
        /// <param name="cD"></param>
        public void SetPlayerPosition(CellData cD)
        {
            cD.getAssignedZone();
            
        }

        public void RecalculateOcclusion()
        {
            
        }


        public CellData[] traverseDepth(CellData cD, int range)
        {
            return null;
        }


    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using UnityEngine.AddressableAssets;
using NaughtyAttributes;
using System;

namespace ALOB.Map
{
    /// <summary>
	/// Contains all properties to define a room preset.
	/// </summary>
    [CreateAssetMenu(fileName = "RoomData", menuName = "SCPEditor/RoomData")]
    public class RoomData : ScriptableObject
    {
        [Header("Room Info")]

        public string roomName = "genericRoomName";
        public RoomShapes shape = 0;

        [Tooltip("Lower number means the room is prioritized to form pathways.")]
        public int pathFinderTravelCost = 2;
        
        public bool large = false;
        
        [ShowIf("large")]
        public Vector2Int[] expandRelativeToOrigin = new Vector2Int[]{};

        public bool isExit = false;
        public AssetReferenceGameObject roomAddr;


        [Header("Spawn Chance")]
        public bool mustSpawn = false;

        [Tooltip("Defined in % / spawned cell when deciding on spawning")]
        [Range(1, 99)]
        [HideIf("mustSpawn")]
        public int spawnChance = 10;

        [HideIf("mustSpawn")]        
        public bool spawnOnce = false;

        // Reserved for future fields

        [Button]
        void showRoom()
        {
            #if UNITY_EDITOR
            AssetPreview.GetAssetPreview(roomAddr.Asset);
            EditorGUIUtility.PingObject( this );
            #endif
        }



        [Button]
        void setNameFromAddressable()
        {
            #if UNITY_EDITOR
            if(roomAddr != null && roomAddr.editorAsset != null)
            {
                this.name = roomAddr.editorAsset.name;
                this.roomName = roomAddr.editorAsset.name;
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(this), roomAddr.editorAsset.name);
                AssetDatabase.SaveAssets();
                //AssetDatabase.SaveAssetIfDirty(this);
            }
#endif
        }


        /// <summary>
        /// Get the total area this room occupies in cells
        /// </summary>
        public int getOccupiedCellsSize()
        {
            if(large)
            {
                if(expandRelativeToOrigin == null)
                {
                    
                }

                // + 1 for include self
                return expandRelativeToOrigin.Length + 1;
            } 
            else 
            {
                return 1;
            }
        }

		
	}

}
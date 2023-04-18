using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Array2DEditor;

namespace ALOB.Map
{

    /// <summary>
	/// Contains all settings for map generation.
	/// </summary>
    [CreateAssetMenu(fileName = "Sample Map Settings", menuName = "SCPEditor/MapSettings")]
    public class GeneratorMapPreset : ScriptableObject
    {

            [Header("Map setup")]
            /// <summary>
            /// Generator User setting to define all zones and their contained rooms.
            /// This is modified during runtime during placeZones() based on zoneLayout, then in generateGrid(), both of these are constant and shouldn't be changing.
            /// </summary>
            public List<Zone> zones;

            /// <summary>
            /// Generator User setting to mark which zones should be connected.
            /// </summary>
            public zoneConnector[] connections;

            /// <summary>
            /// Generator User setting to define locations of the zones.
            /// </summary>
            public Array2DString zoneLayout;


            [Header("Per Zone Grid setup")]
            [Range(5, 1024)]
            public int gridSizeX = 10;

            /// <summary>
            /// room size (distance between cells).
            /// </summary>
            public float spacing = 20.5f;

            /// <summary>
            /// distance between zones (default same as room size).
            /// </summary>
            public const float zoneSpacing = 20.5f;

    }
}
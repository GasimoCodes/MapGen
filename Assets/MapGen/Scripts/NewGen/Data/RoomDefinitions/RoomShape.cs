using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines all possible exit variations from a cell (room)
/// </summary>

namespace ALOB.Map
{
    public enum RoomShapes
    {
        ENDROOM,    // ═×
        HALLWAY,    // ══
        CROOM,      // ═╗
        TSHAPE,     // ═╣
        FOURDOORS   // ═╬═
    }
}
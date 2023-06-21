using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ALOB.Map
{
    /// <summary>
    /// Logger method and formatter for MapGen. 
    /// </summary>
    public static class MapGenLogger
    {
        [HideInCallstack]
        public static void Log(object obj, string module = "")
        {
            Debug.Log($"[Gen{module}] " + obj.ToString());
        }

        [HideInCallstack]
        public static void LogError(object obj, string module = "")
        {
            Debug.LogError($"[Gen{module}] " + "<color=red>" + obj.ToString() + "</color>");
        }
        
        [HideInCallstack]
        public static void LogAssert(object obj, string module = "")
        {
            Debug.LogAssertion($"[Gen{module}] " + obj.ToString());
        }

        [HideInCallstack]
        public static void LogSoftError(object obj, string module = "")
        {
            Debug.Log($"[Gen{module}] " + "<color=red>" + obj.ToString() + "</color>");
        }
    }
}
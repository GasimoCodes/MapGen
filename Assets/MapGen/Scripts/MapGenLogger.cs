using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALOB.Map
{
    public static class MapGenLogger
    {
        public static void Log(object obj, string module = "")
        {
            Debug.Log($"[Gen{module}] " + obj.ToString());
        }

        public static void LogError(object obj, string module = "")
        {
            Debug.LogError($"[Gen{module}] " + obj.ToString());
        }

        public static void LogAssert(object obj, string module = "")
        {
            Debug.LogAssertion($"[Gen{module}] " + obj.ToString());
        }
    }
}
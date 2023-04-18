using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ALOB.Map;

namespace ALOB.Editor
{
    [CustomEditor(typeof(GeneratorMapPreset))]
    [CanEditMultipleObjects]
    public class NewGenEditor : UnityEditor.Editor
    {
            public SerializedProperty sp;

            public string[] getZoneIDs()
            {
                string[] x = new string[sp.arraySize];
                for(int i = 0; i < sp.arraySize; i++)
                {
                    x[i] = sp.GetArrayElementAtIndex(i).stringValue;
                }
                return x;
            }

            public void OnEnable()
            {
                sp = serializedObject.FindProperty("zones");
            }   

            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                
                if(sp.arraySize < 1)
                {
                    EditorGUILayout.HelpBox("No zones have been assigned or defined.", MessageType.Info);
                }

                DrawDefaultInspector(); 
            }
    }
/*

    [CustomEditor(typeof(zone))]
    [CanEditMultipleObjects]
    public class ZoneEditor : UnityEditor.Editor
    {
            SerializedProperty sp;

            public void OnEnable()
            {
                sp = serializedObject.FindProperty("connectedZones");
            }   

            
            public override void OnInspectorGUI()
            {
                serializedObject.Update();

                if(sp.arraySize >= 1)
                {
                    if(!(sp.arraySize < 4))
                    EditorGUILayout.HelpBox("No zones have been assigned or defined.", MessageType.Info);
                    else
                    EditorGUILayout.HelpBox("You cannot have more than 4 zones connected at the same time.", MessageType.Warning);
                }

                DrawDefaultInspector(); 
            }
    }

*/
    
}
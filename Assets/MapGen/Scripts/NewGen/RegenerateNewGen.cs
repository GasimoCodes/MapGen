using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System.Reflection;
using UnityEditor;
using System.Threading;
using System;

namespace ALOB.Map
{
    public class RegenerateNewGen : MonoBehaviour
    {
        [Header("0 = Random")]
        public int seed = 0;

#if UNITY_EDITOR

        [Button("Regenerate", enabledMode: EButtonEnableMode.Playmode)]
        public void EnabledInPlaymodeOnly()
        {

            ClearLog();

            NewGen ng = this.GetComponent<NewGen>();
            if (seed == 0)
                ng.generateMap((UnityEngine.Random.Range(10000000, 99999999)));
            else
                ng.generateMap((seed));
        }

        [Button("Regenerate 100 times", enabledMode: EButtonEnableMode.Playmode)]
        public void Generate100()
        {


            int i = 0;
            int seed;
            NewGen ng = this.GetComponent<NewGen>();

            while (i < 100)
            {
                seed = UnityEngine.Random.Range(10000000, 99999999);
                ClearLog();

                try
                {
                    if (!ng.generateMap((seed)))
                    {

                        throw new Exception("Generator execution error noticed.");
                    }

                    i++;
                }
                catch (Exception e)
                {
                    Debug.Log("Error found during attempt " + i + "(Seed: " + seed + ")");
                    throw (e);
                }



            }

            Debug.Log("Finished all 100");
        }



        [Button]
        public void ClearLog()
        {
            var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
        }

#endif

        public void Start()
        {
            Application.targetFrameRate = 60;
            EnabledInPlaymodeOnly();
        }


#if DEVELOPMENT_BUILD

        public void EnabledInPlaymodeOnly()
        {
            try{
            
            NewGen ng = this.GetComponent<NewGen>();
            if (seed == 0)
                ng.generateMap((UnityEngine.Random.Range(10000000, 99999999)));
            else
                ng.generateMap((seed));

            }
            catch()
            {
            
            EnabledInPlaymodeOnly();
            
            }

            
        }

#endif

    }
}

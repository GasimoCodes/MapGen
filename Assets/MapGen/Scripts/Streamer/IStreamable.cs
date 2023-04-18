using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALOB.Map
{
    public interface IStreamable
    {

        void OnOcclude();

        void OnVisible();


        // PS: OnDestroy and OnLoad are already included in Unity
    }
}
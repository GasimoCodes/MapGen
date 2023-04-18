using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ReleaseAddresableOnDestroy : MonoBehaviour
{
    void OnDestroy() {
        // Debug.Log("Asset was destroyed, releasing " + this.name);
        Addressables.ReleaseInstance(gameObject);
    }
}

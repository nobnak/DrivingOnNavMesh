using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace DrivingOnNavMesh {
        
    public class NavMeshUpdator : MonoBehaviour {
        [SerializeField]
        Vector3 size = new Vector3 (100f, 100f, 100f);

        AsyncOperation updateProcess;
        List<NavMeshBuildSource> sources;
        NavMeshData navMesh;
        NavMeshDataInstance navMeshInstance;

        #region Unity
        void OnEnable() {
            sources = new List<NavMeshBuildSource> ();
            navMesh = new NavMeshData ();
            navMeshInstance = NavMesh.AddNavMeshData (navMesh);
            UpdateNavMesh (false);
        }
        IEnumerator Start () {
            while (true) {
                updateProcess = UpdateNavMesh (true);
                yield return updateProcess;
            }
    	}
        void OnDisable() {
            navMeshInstance.Remove ();
        }
        #endregion

        AsyncOperation UpdateNavMesh(bool asyncOperation = false) {
            NavMeshSourceTag.Collect (ref sources);
            var setting = NavMesh.GetSettingsByID (0);
            var bounds = new Bounds (Quantizer.Quantize (transform.position, 0.1f * size), size);

            if (asyncOperation) {
                return NavMeshBuilder.UpdateNavMeshDataAsync (navMesh, setting, sources, bounds);
            } else {
                NavMeshBuilder.UpdateNavMeshData (navMesh, setting, sources, bounds);
                return null;
            }
        }
    }
}

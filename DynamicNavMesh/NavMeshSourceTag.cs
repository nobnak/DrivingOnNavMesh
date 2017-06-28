using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace DrivingOnNavMesh {
        
    public class NavMeshSourceTag : MonoBehaviour {
        [SerializeField]
        int area = 0;

        MeshFilter mf;

        static List<NavMeshSourceTag> tags = new List<NavMeshSourceTag>();

        void OnEnable() {
            mf = GetMeshFilter ();
            tags.Add (this);
        }
        void OnDisable() {
            tags.Remove (this);
        }
        IEnumerable<NavMeshBuildSource> GetSources() {
            if (mf != null) {
                var source = new NavMeshBuildSource ();
                source.shape = NavMeshBuildSourceShape.Mesh;
                source.area = area;
                source.sourceObject = mf.sharedMesh;
                source.transform = transform.localToWorldMatrix;
                yield return source;
            }
        }

        public static void Collect(ref List<NavMeshBuildSource> list) {
            list.Clear ();
            foreach (var tag in tags)
                list.AddRange (tag.GetSources ());
        }

        bool TryGetMeshFilter(out MeshFilter mf) {
            return (mf = GetMeshFilter()) != null;
        }
        MeshFilter GetMeshFilter() {
            return GetComponent<MeshFilter> ();
        }
    }
}

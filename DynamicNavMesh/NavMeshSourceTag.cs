using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace DrivingOnNavMesh {
        
    public class NavMeshSourceTag : MonoBehaviour {
        [SerializeField]
        int area = 0;

        MeshFilter mf;

        static bool tagsChanged = true;
        static List<NavMeshSourceTag> tags = new List<NavMeshSourceTag>();

        #region Unity
        void OnEnable() {
            mf = GetMeshFilter ();
            Add ();
        }
        void OnDisable() {
            Remove ();
        }
        #endregion

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
        void Add() {
            transform.hasChanged = true;
            tagsChanged = true;
            tags.Add (this);
        }
        void Remove() {
            tagsChanged = true;
            tags.Remove (this);
        }

        public static void Collect(ref List<NavMeshBuildSource> list) {
            list.Clear ();
            foreach (var tag in tags)
                list.AddRange (tag.GetSources ());
        }
        public static bool MakeUnchanged() {
            var changed = tagsChanged;
            tagsChanged = false;

            foreach (var t in tags) {
                var tr = t.transform;
                tagsChanged = (tagsChanged ? tagsChanged : tr.hasChanged);
                tr.hasChanged = false;
            }

            return changed;
        }

        bool TryGetMeshFilter(out MeshFilter mf) {
            return (mf = GetMeshFilter()) != null;
        }
        MeshFilter GetMeshFilter() {
            return GetComponent<MeshFilter> ();
        }
    }
}

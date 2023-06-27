using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace DrivingOnNavMesh {

    public class NavMeshPathRouter : AbstractRouter {
        protected NavMeshPath navPath;

        #region implemented abstract members of AbstractRouter
        protected override bool TryToFindPath (float3 pointFrom, float3 pointTo, out float3[] path) {
            return TryToFindPath (pointFrom, pointTo, out path, NavMesh.AllAreas);
        }
        #endregion

        protected virtual bool TryToFindPath(float3 pointFrom, float3 pointTo, out float3[] path, int area) {
            var result = NavMesh.CalculatePath (pointFrom, pointTo, area, navPath = new NavMeshPath());
            path = navPath.corners.Cast<float3>().ToArray();
            return result;
        }
    }
}

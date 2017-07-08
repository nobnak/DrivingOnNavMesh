using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace DrivingOnNavMesh {

    public class NavMeshPathRouter : AbstractRouter {
        protected NavMeshPath navPath;

        #region implemented abstract members of AbstractRouter
        protected override bool TryToFindPath (Vector3 pointFrom, Vector3 pointTo, out Vector3[] path) {
            return TryToFindPath (pointFrom, pointTo, out path, NavMesh.AllAreas);
        }
        #endregion

        protected virtual bool TryToFindPath(Vector3 pointFrom, Vector3 pointTo, out Vector3[] path, int area) {
            var result = NavMesh.CalculatePath (pointFrom, pointTo, area, navPath = new NavMeshPath());
            path = navPath.corners;
            return result;
        }
    }
}

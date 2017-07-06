using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace DrivingOnNavMesh {

    public class NavMeshPathRouter : AbstractRouter {
        protected NavMeshPath path;

        #region implemented abstract members of AbstractRouter
        public override bool TryToStartRoute (Vector3 pointFrom, Vector3 pointTo) {
            return TryToStartRoute (pointFrom, pointTo, NavMesh.AllAreas);
        }
        #endregion

        public virtual bool TryToStartRoute(Vector3 pointFrom, Vector3 pointTo, int area) {
            var result = NavMesh.CalculatePath (pointFrom, pointTo, area, path = new NavMeshPath());
            if (result) {
                SetRouteState (RouteStateEnum.Reacheable);
                Build (corners = path.corners, out activeRange, out tangents, out lengths);
            } else
                SetRouteState (RouteStateEnum.Invalid);
                
            return result;
        }
    }
}

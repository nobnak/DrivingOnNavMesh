using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace DrivingOnNavMesh {

    public class LinearRouter : AbstractRouter {
        #region implemented abstract members of AbstractRouter
        protected override bool TryToFindPath (Vector3 pointFrom, Vector3 pointTo, out Vector3[] path) {
            path = new Vector3[] { pointFrom, pointTo };
            return true;
        }
        #endregion
    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace DrivingOnNavMesh {

    public class LinearRouter : AbstractRouter {
        #region implemented abstract members of AbstractRouter
        protected override bool TryToFindPath (float3 pointFrom, float3 pointTo, out float3[] path) {
            path = new float3[] { pointFrom, pointTo };
            return true;
        }
        #endregion
    }
}

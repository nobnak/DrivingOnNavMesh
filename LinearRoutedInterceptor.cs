using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace DrivingOnNavMesh {

    public class LinearRoutedInterceptor : AbstractRoutetedInterceptor {

        public LinearRoutedInterceptor(RootMotionInterceptor rootMotion)
            : base(rootMotion, new LinearRouter()) {
        }

        #region implemented abstract members of AbstractRoutetedInterceptor
        protected override bool TryToStartNavigationTo(float3 destination) {
            var source = rootMotion.Tr.Position;
            return router.TryToStartRoute (source, destination);
        }
        protected override void UpdateTarget (float3 pointFrom, float t) {
			var heading = CurrentDestination - pointFrom;
			rootMotion.SetHeading(heading);
            //SetDestination (CurrentDestination);
        }
        #endregion
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace DrivingOnNavMesh {

    public class LinearRoutedInterceptor : AbstractRoutetedInterceptor {

        public LinearRoutedInterceptor(RootMotionInterceptor rootMotion)
            : base(rootMotion, new LinearRouter()) {
        }

        #region implemented abstract members of AbstractRoutetedInterceptor
        protected override bool TryToStartNavigationTo(Vector3 destination) {
            var source = rootMotion.Tr.position;
            return router.TryToStartRoute (source, destination);
        }
        protected override void UpdateTarget (Vector3 pointFrom, float t) {
			var heading = CurrentDestination - pointFrom;
			rootMotion.SetHeading(heading);
            //SetDestination (CurrentDestination);
        }
        #endregion
    }
}

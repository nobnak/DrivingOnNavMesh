using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace DrivingOnNavMesh {

    public class LinearRoutedInterceptor : AbstractRoutetedInterceptor {

        public LinearRoutedInterceptor(Animator anim, Transform tr, DrivingSetting drivingSetting)
            : base(anim, tr, drivingSetting, new LinearRouter()) {
        }

        #region implemented abstract members of AbstractRoutetedInterceptor
        protected override bool TryToStartNavigationTo(Vector3 destination) {
            var source = tr.position;
            var result = router.TryToStartRoute (source, destination);
			if (result) SetDestination(destination);
			return result;
        }
        protected override void UpdateTarget (Vector3 pointFrom, float t) {
            //SetDestination (CurrentDestination);
        }
        #endregion
    }
}

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
            this.CurrentDestination = destination;
            return router.TryToStartRoute (source, destination);
        }
        protected override void UpdateTarget (Vector3 pointFrom, float t) {
            SetTarget (CurrentDestination);
        }
        #endregion
    }
}

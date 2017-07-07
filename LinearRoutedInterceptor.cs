using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace DrivingOnNavMesh {

    public class LinearRoutedInterceptor : AbstractRoutetedInterceptor {
        protected Vector3 destination;

        public LinearRoutedInterceptor(Animator anim, Transform tr, DrivingSetting drivingSetting)
            : base(anim, tr, drivingSetting, new LinearRouter()) {
        }

        #region implemented abstract members of AbstractRoutetedInterceptor
        protected override bool TryToStartNavigationTo(Vector3 destination) {
            var source = _tr.position;
            this.destination = destination;
            return router.TryToStartRoute (source, destination);
        }
        protected override void UpdateTarget (Vector3 pointFrom, float t) {
            SetTarget (destination);
        }
        #endregion
    }
}

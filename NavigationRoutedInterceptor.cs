using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace DrivingOnNavMesh {

    public class NavigationRoutedInterceptor : AbstractRoutetedInterceptor {
        protected DrivingAndPathSetting motionData;

        protected bool approximateStartPoint = true;
        protected bool approximateEndPoint = true;

        public NavigationRoutedInterceptor(Animator anim, Transform tr, DrivingAndPathSetting motionData)
            : base(anim, tr, motionData, new NavMeshPathRouter()) {
            this.motionData = motionData;
        }

        #region implemented abstract members of AbstractRoutetedInterceptor
        protected override bool TryToStartNavigationTo(Vector3 destination) {
            var source = tr.position;
            if (approximateStartPoint || approximateEndPoint) {
                NavMeshHit hit;
                var distance = RADIUS * tr.lossyScale.sqrMagnitude;
                if (approximateStartPoint && NavMesh.SamplePosition (source, out hit, distance, NavMesh.AllAreas))
                    source = hit.position;
                if (approximateEndPoint && NavMesh.SamplePosition (destination, out hit, distance, NavMesh.AllAreas))
                    destination = hit.position;
            }
            return router.TryToStartRoute (source, destination);
        }
        protected override void UpdateTarget (Vector3 pointFrom, float t) {
            var pointOn = router.PointAt (t);
            var tangentOn = router.TangentAt (t);
            var toPoint = pointOn - pointFrom;
            var moveBy = motionData.DestinationDistance * tangentOn;
            if (toPoint.sqrMagnitude > Mathf.Epsilon) {
                var distance = toPoint.magnitude;
                var toPointDir = toPoint.normalized;
                var angle = 0.5f * (1f - Vector3.Dot (tr.forward, toPointDir));
                var distantialRatio = motionData.PathFollowingDistanceRatio * distance;
                var angularRatio = motionData.PathFollowingAngleRatio;
                var mixRatio = Mathf.Clamp01 (Mathf.Lerp (distantialRatio, angularRatio, angle));
                moveBy = motionData.DestinationDistance * Vector3.Lerp (tangentOn, toPointDir, mixRatio);
            }
            SetTarget (pointFrom + moveBy);
        }

        #endregion

        [System.Serializable]
        public class DrivingAndPathSetting : RootMotionInterceptor.DrivingSetting {
            public float DestinationDistance { get; protected set; }
            public float PathFollowingAngleRatio { get; protected set; }
            public float PathFollowingDistanceRatio { get; protected set; }

            public DrivingAndPathSetting() : base() {
                this.DestinationDistance = 1f;
                this.PathFollowingAngleRatio = 0.1f;
                this.PathFollowingDistanceRatio = 0.5f;
            }           

            public DrivingAndPathSetting SetDestinationDistance(float v) {
                DestinationDistance = v;
                return this;
            }
            public DrivingAndPathSetting SetPathFollowingAngleRatio(float v) {
                PathFollowingAngleRatio = v;
                return this;
            }
            public DrivingAndPathSetting SetPathFollowingDistanceRatio(float v) {
                PathFollowingDistanceRatio = v;
                return this;
            }
        }
    }
}

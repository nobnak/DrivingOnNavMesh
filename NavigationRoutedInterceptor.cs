using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace DrivingOnNavMesh {

    public class NavigationRoutedInterceptor : AbstractRoutetedInterceptor {
        protected DrivingAndPathSetting motionData;

        protected bool approximateStartPoint = true;
        protected bool approximateEndPoint = true;

        public NavigationRoutedInterceptor(RootMotionInterceptor rootMotion, DrivingAndPathSetting motionData)
            : base(rootMotion, new NavMeshPathRouter()) {
            this.motionData = motionData;
        }

        #region implemented abstract members of AbstractRoutetedInterceptor
        protected override bool TryToStartNavigationTo(float3 destination) {
            var source = rootMotion.Tr.Position;
            if (approximateStartPoint || approximateEndPoint) {
                NavMeshHit hit;
				var distance = RADIUS; // * rootMotion.Tr.lossyScale.sqrMagnitude;
                if (approximateStartPoint && NavMesh.SamplePosition (source, out hit, distance, NavMesh.AllAreas))
                    source = hit.position;
                if (approximateEndPoint && NavMesh.SamplePosition (destination, out hit, distance, NavMesh.AllAreas))
                    destination = hit.position;
            }
            return router.TryToStartRoute (source, destination);
        }
        protected override void UpdateTarget (float3 pointFrom, float t) {
            var pointOn = router.PointAt (t);
            var tangentOn = router.TangentAt (t);
            var toPoint = pointOn - pointFrom;
            var moveBy = motionData.DestinationDistance * tangentOn;
            if (math.lengthsq(toPoint) > 1e-4f) {
                var distance = math.length(toPoint);
                var toPointDir = math.normalize(toPoint);
                var angle = 0.5f * (1f - math.dot(rootMotion.Tr.Forward, toPointDir));
                var distantialRatio = motionData.PathFollowingDistanceRatio * distance;
                var angularRatio = motionData.PathFollowingAngleRatio;
                var mixRatio = math.clamp (math.lerp (distantialRatio, angularRatio, angle), 0f, 1f);
                moveBy = motionData.DestinationDistance * math.lerp(tangentOn, toPointDir, mixRatio);
            }
            SetDestination (pointFrom + moveBy);
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

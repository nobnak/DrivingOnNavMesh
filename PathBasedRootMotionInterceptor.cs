using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace DrivingOnNavMesh {

    public class PathBasedRootMotionInterceptor : RootMotionInterceptor {
        protected DrivingAndPathSetting motionData;
        protected NavMeshPathGeometry path;

        public PathBasedRootMotionInterceptor(Animator anim, Transform tr, DrivingAndPathSetting motionData)
            : base(anim, tr, motionData) {
            this.motionData = motionData;
            this.path = new NavMeshPathGeometry (new NavMeshPath ());
        }

        #region Override
        public override bool SetTarget (Vector3 target) {
            if (!path.CalculatePath (_tr.position, target)) {
                ResetTarget ();
                return false;
            }
            return base.SetTarget (target);
        }
        #endregion

    	public virtual void Update () {
            if (!ActiveAndValid)
                return;
            
            var center = _tr.position;
            var t = path.NearestCorner (center);
            if (t < 0f) {
                ResetTarget ();
                return;
            }

            path.ActiveRange.SetRangeBegin (t);
            if (path.ActiveRange.Length <= Mathf.Epsilon) {
                ResetTarget ();
                return;
            }

            var pointOn = path.PointAt (t);
            var tangentOn = path.TangentAt (t);
            var toPoint = pointOn - center;
            var moveBy = motionData.destinationDistance * tangentOn;
            if (toPoint.sqrMagnitude > Mathf.Epsilon) {
                var distance = toPoint.magnitude;
                var toPointDir = toPoint.normalized;
                var angle = 0.5f * (1f - Vector3.Dot (_tr.forward, toPointDir));
                var distantialRatio = motionData.pathFollowingDistanceRatio * distance;
                var angularRatio = motionData.pathFollowingAngleRatio;
                var mixRatio = Mathf.Clamp01 (Mathf.Lerp (distantialRatio, angularRatio, angle));
                moveBy = motionData.destinationDistance * Vector3.Lerp (tangentOn, toPointDir, mixRatio);
            }
            SetTarget (center + moveBy);
        }

        public class DrivingAndPathSetting : RootMotionInterceptor.DrivingSetting {
            public float destinationDistance = 1f;
            public float pathFollowingAngleRatio = 0.1f;
            public float pathFollowingDistanceRatio = 0.5f;
        }
    }
}

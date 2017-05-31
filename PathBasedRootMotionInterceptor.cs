using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace DrivingOnNavMesh {

    public class PathBasedRootMotionInterceptor : RootMotionInterceptor {
        [SerializeField]
        MotionData motionData;

        bool destinationIsActive;
        NavMeshPathGeometry path;

        public PathBasedRootMotionInterceptor(Animator anim, Transform tr) : base(anim, tr) {
            path = new NavMeshPathGeometry (new NavMeshPath ());
        }

    	public void Update () {
            if (destinationIsActive) {
                var center = _tr.position;
                var t = path.NearestCorner (center);
                if (t < 0f) {
                    destinationIsActive = false;
                } else {
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

                    path.ActiveRange.SetRangeBegin (t);
                    if (path.ActiveRange.Length <= Mathf.Epsilon)
                        destinationIsActive = false;
                }
            }
        }

        [System.Serializable]
        public class MotionData {
            [Header("Corner")]
            public float destinationDistance = 1f;
            public float pathFollowingAngleRatio = 0.1f;
            public float pathFollowingDistanceRatio = 0.5f;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace DrivingOnNavMesh {

    public abstract class AbstractRouter {
        public enum RouteStateEnum { Invalid = 0, Reacheable }

        protected RouteStateEnum state;

        protected Vector3[] corners;
        protected Vector3[] tangents;
        protected float[] lengths;

        protected Range activeRange;

        #region Abstract
        public abstract bool TryToStartRoute (Vector3 pointFrom, Vector3 pointTo);
        #endregion

        #region Public
        public virtual Range ActiveRange { get { return activeRange; } }

        public virtual float ClosestT(Vector3 p) {
            var minSqrDist = float.MaxValue;
            var minT = -1f;
            var indexBegin = activeRange.IndexBegin;
            var indexEnd = activeRange.IndexEnd;

            for (var i = indexBegin; i < indexEnd; i++) {
                var inext = Mathf.Min (i + 1, indexEnd);

                var c = corners [i];
                var dir = p - c;
                var tan = tangents [i + 1];
                var len = lengths [i];
                var t = Vector3.Dot (dir, tan) / len;
                t = activeRange.ClampInActive (t, i);

                if (t <= 0f) {
                    t = 0f;
                } else if (t < 1f) {
                    c = Vector3.Lerp (c, corners [inext], t);
                } else {
                    t = 1f;
                    c = corners [inext];
                }

                var tentativeSqrDist = (p - c).sqrMagnitude;
                if (tentativeSqrDist < minSqrDist) {
                    minSqrDist = tentativeSqrDist;
                    minT = t + i;
                }
            }

            return minT;
        }
        public virtual Vector3 PointAt(float t) {
            float tlerp;
            var tfloor = Floor (t, out tlerp);
            return activeRange.Lerp(corners, tfloor, tlerp);
        }
        public virtual Vector3 TangentAt(float t) {
            float tlerp;
            var tfloor = Floor (t + 0.5f, out tlerp);
            return activeRange.Lerp (tangents, tfloor, tlerp);
        }
        #endregion

        #region Gizmo
        public virtual void DrawGizmos() {
            if (state == RouteStateEnum.Invalid)
                return;
            
            var pfrom = PointAt (activeRange.min);
            for (var i = activeRange.min; i < activeRange.max; i++) {
                var pto = PointAt (i + 1);
                Gizmos.DrawLine (pfrom, pto);
                pfrom = pto;
            }
        }
        #endregion

        #region Protected
        protected virtual void SetRouteState(RouteStateEnum nextState) {
            state = nextState;
        }
        #endregion

        #region Static
        protected static void Build(Vector3[] corners, 
            out Range activeRange, out Vector3[] tangents, out float[] lengths) {
            var cornerCount = corners.Length;

            activeRange = new Range(cornerCount - 1);
            tangents = new Vector3[cornerCount + 1];
            lengths = new float[cornerCount - 1];

            for (var i = 0; i < cornerCount - 1; i++) {
                var ca = corners [i];
                var cb = corners [i + 1];
                var v = cb - ca;
                lengths [i] = v.magnitude;
                tangents [i + 1] = v.normalized;
            }
            tangents [0] = tangents [1];
            tangents [cornerCount] = tangents [cornerCount - 1];
        }
        protected static int Floor (float t, out float tlerp) {
            var tfloor = Mathf.FloorToInt (t);
            tlerp = t - tfloor;
            return tfloor;
        }
        #endregion

        #region Classes
        public class Range {
            public readonly int min;
            public readonly int max;

            public float ActiveRangeBegin { get; protected set; }
            public float ActiveRangeEnd { get; protected set; }

            public int IndexBegin { get; protected set; }
            public int IndexEnd { get; protected set; }

            public Range(int min, int max) {
                this.min = min;
                this.max = max;

                SetActiveRange(min, max);
            }
            public Range(int max) : this(0, max) {}

            public float Length { get { return ActiveRangeEnd - ActiveRangeBegin; } }

            public void SetActiveRange(float rangeBegin, float rangeEnd) {
                ActiveRangeBegin = Mathf.Clamp(rangeBegin, min, max);
                ActiveRangeEnd = Mathf.Clamp(rangeEnd, rangeBegin, max);

                IndexBegin = Mathf.FloorToInt (ActiveRangeBegin);
                IndexEnd = Mathf.CeilToInt (ActiveRangeEnd);
            }
            public void SetRangeBegin(float begin) {
                SetActiveRange (begin, ActiveRangeEnd);
            }
            public void SetRangeEnd(float end) {
                SetActiveRange(ActiveRangeBegin, end);
            }

            public float ClampInActive (float t) {
                return Mathf.Clamp (t, ActiveRangeBegin, ActiveRangeEnd);
            }
            public float ClampInActive(float t, int index) {
                return ClampInActive (t + index) - index;
            }

            public float Clamp(float t) {
                return Mathf.Clamp (t, min, max);
            }
            public int Clamp(int index) {
                return Mathf.Clamp(index, min, max);
            }
            public Vector3 Lerp(IList<Vector3> array, int index, float tlerp) {
                if (index < 0)
                    return array [0];
                else if (index < max)
                    return Vector3.LerpUnclamped (array [index], array [index + 1], tlerp);
                else
                    return array [max];
            }
        }
        #endregion
    }
}

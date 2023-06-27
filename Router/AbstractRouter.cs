using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace DrivingOnNavMesh {

    public abstract class AbstractRouter {
        public enum RouteStateEnum { Invalid = 0, Reacheable }

        protected RouteStateEnum state;

        protected float3[] path;
        protected float3[] tangents;
        protected float[] lengths;

        protected Range activeRange;

        #region Abstract
        protected abstract bool TryToFindPath(float3 pointFrom, float3 pointTo, out float3[] path);
        #endregion

        public virtual bool TryToStartRoute (Vector3 pointFrom, float3 pointTo) {
            var result = TryToFindPath (pointFrom, pointTo, out path);
            if (result) {
                SetRouteState (RouteStateEnum.Reacheable);
                Build (path, out activeRange, out tangents, out lengths);
            } else
                SetRouteState (RouteStateEnum.Invalid);
            return result;
        }

        #region Public
        public virtual Range ActiveRange { get { return activeRange; } }

        public virtual float ClosestT(float3 p) {
            var minSqrDist = float.MaxValue;
            var minT = -1f;
            var indexBegin = activeRange.IndexBegin;
            var indexEnd = activeRange.IndexEnd;

            for (var i = indexBegin; i < indexEnd; i++) {
                var inext = math.min (i + 1, indexEnd);

                var c = path [i];
                var dir = p - c;
                var tan = tangents [i + 1];
                var len = lengths [i];
                var t = Vector3.Dot (dir, tan) / len;
                t = activeRange.ClampInActive (t, i);

                if (t <= 0f) {
                    t = 0f;
                } else if (t < 1f) {
                    c = math.lerp (c, path [inext], t);
                } else {
                    t = 1f;
                    c = path [inext];
                }

                var tentativeSqrDist = math.lengthsq(p - c);
                if (tentativeSqrDist < minSqrDist) {
                    minSqrDist = tentativeSqrDist;
                    minT = t + i;
                }
            }

            return minT;
        }
        public virtual float3 PointAt(float t) {
            float tlerp;
            var tfloor = Floor (t, out tlerp);
            return activeRange.Lerp(path, tfloor, tlerp);
        }
        public virtual float3 TangentAt(float t) {
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
        protected static void Build(float3[] corners, 
            out Range activeRange, out float3[] tangents, out float[] lengths) {
            var cornerCount = corners.Length;

            activeRange = new Range(cornerCount - 1);
            tangents = new float3[cornerCount + 1];
            lengths = new float[cornerCount - 1];

            for (var i = 0; i < cornerCount - 1; i++) {
                var ca = corners [i];
                var cb = corners [i + 1];
                var v = cb - ca;
                lengths [i] = math.length(v);
                tangents [i + 1] = math.normalize(v);
            }
            tangents [0] = tangents [1];
            tangents [cornerCount] = tangents [cornerCount - 1];
        }
        protected static int Floor (float t, out float tlerp) {
            var tfloor = (int)math.floor(t);
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
                ActiveRangeBegin = math.clamp(rangeBegin, min, max);
                ActiveRangeEnd = math.clamp(rangeEnd, rangeBegin, max);

                IndexBegin = (int)math.floor(ActiveRangeBegin);
                IndexEnd = (int)math.ceil(ActiveRangeEnd);
            }
            public void SetRangeBegin(float begin) {
                SetActiveRange (begin, ActiveRangeEnd);
            }
            public void SetRangeEnd(float end) {
                SetActiveRange(ActiveRangeBegin, end);
            }

            public float ClampInActive (float t) {
                return math.clamp(t, ActiveRangeBegin, ActiveRangeEnd);
            }
            public float ClampInActive(float t, int index) {
                return ClampInActive (t + index) - index;
            }

            public float Clamp(float t) {
                return math.clamp (t, min, max);
            }
            public int Clamp(int index) {
                return math.clamp(index, min, max);
            }
            public float3 Lerp(IList<float3> array, int index, float tlerp) {
                if (index < 0)
                    return array [0];
                else if (index < max)
                    return math.lerp(array [index], array [index + 1], tlerp);
                else
                    return array [max];
            }
        }
        #endregion
    }
}

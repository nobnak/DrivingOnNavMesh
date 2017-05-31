using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace DrivingOnNavMesh {

    public class NavMeshPathGeometry {
        protected NavMeshPath path;

        protected Vector3[] tangents;
        protected float[] lengths;

        protected Range activeRange;

        public NavMeshPathGeometry(NavMeshPath path) {
            this.path = path;
        }

        #region Public
        public NavMeshPath Path { get { return path; } }
        public Range ActiveRange { get { return activeRange; } }

        #region Calculate Path
        public void Build() {
            Debug.Log ("Build NavMesh Geomatry");
            var cornerCount = path.corners.Length;

            activeRange = new Range(cornerCount - 1);

            tangents = new Vector3[cornerCount + 1];
            lengths = new float[cornerCount - 1];
            for (var i = 0; i < cornerCount - 1; i++) {
                var ca = path.corners [i];
                var cb = path.corners [i + 1];
                var v = cb - ca;
                lengths [i] = v.magnitude;
                tangents [i + 1] = v.normalized;
            }
            tangents [0] = tangents [1];
            tangents [cornerCount] = tangents [cornerCount - 1];
        }
        public bool CalculatePath(Vector3 pointFrom, Vector3 pointTo, int area = NavMesh.AllAreas) {
            var result = NavMesh.CalculatePath (pointFrom, pointTo, area, path);
            if (result)
                Build ();
            return result;
        }
        #endregion

        public float NearestCorner(Vector3 p) {
            var minSqrDist = float.MaxValue;
            var minCorner = -1f;
            var indexBegin = activeRange.IndexBegin;
            var indexEnd = activeRange.IndexEnd;

            for (var i = indexBegin; i < indexEnd; i++) {
                var inext = Mathf.Min (i + 1, indexEnd);

                var c = path.corners [i];
                var dir = p - c;
                var tan = tangents [i + 1];
                var len = lengths [i];
                var t = Vector3.Dot (dir, tan) / len;
                t = activeRange.ClampInRange (t, i);

                if (t <= 0f) {
                    t = 0f;
                } else if (t < 1f) {
                    c = Vector3.Lerp (c, path.corners [inext], t);
                } else {
                    t = 1f;
                    c = path.corners [inext];
                }

                var tentativeSqrDist = (p - c).sqrMagnitude;
                if (tentativeSqrDist < minSqrDist) {
                    minSqrDist = tentativeSqrDist;
                    minCorner = t + i;
                }
            }

            return minCorner;
        }
        public Vector3 PointAt(float t) {
            float tlerp;
            var tfloor = Floor (t, out tlerp);
            return Lerp(path.corners, tfloor, tlerp);
        }
        public Vector3 TangentAt(float t) {
            float tlerp;
            var tfloor = Floor (t + 0.5f, out tlerp);
            return Lerp (tangents, tfloor, tlerp);
        }
        #endregion

        #region Gizmo
        public void DrawGizmos() {
            if (path.status == NavMeshPathStatus.PathInvalid)
                return;
            
            var indexBegin = activeRange.IndexBegin;
            var indexEnd = activeRange.IndexEnd;

            var pfrom = PointAt (activeRange.ClampInRange(indexBegin));
            for (var i = indexBegin; i < indexEnd; i++) {
                var pto = PointAt (activeRange.ClampInRange(i + i));
                Gizmos.DrawLine (pfrom, pto);
                pfrom = pto;
            }
        }
        #endregion

        #region Protected
        protected static int Floor (float t, out float tlerp) {
            var tfloor = Mathf.FloorToInt (t);
            tlerp = t - tfloor;
            return tfloor;
        }

        protected Vector3 Lerp(IList<Vector3> array, int index, float tlerp) {
            var count = array.Count;
            if (index < 0)
                return array [0];
            else if (index <= (count - 2))
                return Vector3.LerpUnclamped (array [index], array [index + 1], tlerp);
            else
                return array [count - 1];
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

                SetRange(min, max);
            }
            public Range(int max) : this(0, max) {}

            public float Length { get { return ActiveRangeEnd - ActiveRangeBegin; } }

            public void SetRange(float rangeBegin, float rangeEnd) {
                ActiveRangeBegin = Mathf.Clamp(rangeBegin, min, max);
                ActiveRangeEnd = Mathf.Clamp(rangeEnd, rangeBegin, max);

                IndexBegin = Mathf.FloorToInt (ActiveRangeBegin);
                IndexEnd = Mathf.CeilToInt (ActiveRangeEnd);
            }
            public void SetRangeBegin(float begin) {
                SetRange (begin, ActiveRangeEnd);
            }
            public void SetRangeEnd(float end) {
                SetRange(ActiveRangeBegin, end);
            }

            public float ClampInRange (float t) {
                return Mathf.Clamp (t, ActiveRangeBegin, ActiveRangeEnd);
            }
            public float ClampInRange(float t, int index) {
                return ClampInRange (t + index) - index;
            }
        }
        #endregion
    }
}

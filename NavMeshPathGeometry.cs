using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshPathGeometry {
    protected NavMeshPath path;

    protected int indexBegin;
    protected int indexEnd;

    protected float rangeBegin;
    protected float rangeEnd;

    protected Vector3[] tangents;
    protected float[] lengths;

    public NavMeshPathGeometry(NavMeshPath path) {
        this.path = path;

        Reset ();
    }

    public NavMeshPath Path { get { return path; } }

    public void Reset() {
        var cornerCount = path.corners.Length;

        indexBegin = 0;
        indexEnd = cornerCount - 1;

        rangeBegin = 0f;
        rangeEnd = indexEnd;

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

    public float NearestCorner(Vector3 p) {
        var minSqrDist = float.MaxValue;
        var minCorner = -1f;

        var pathCount = path.corners.Length - 1;
        for (var i = 0; i < pathCount; i++) {
            var c = path.corners [i];
            var dir = p - c;
            var tan = tangents [i + 1];
            var len = lengths [i];
            var t = Vector3.Dot (dir, tan) / len;

            if (t <= 0f) {
                t = 0f;
            } else if (t < 1f) {
                c = Vector3.Lerp (c, path.corners [i + 1], t);
            } else {
                t = 1f;
                c = path.corners [i + 1];
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
        var tfloor = Mathf.FloorToInt (t);
        var tlerp = t - tfloor;
        return Vector3.Lerp (path.corners [tfloor], path.corners [tfloor + 1], tlerp);
    }
    public Vector3 NearestPoint(Vector3 p) {
        return PointAt(NearestCorner(p));
    }

    #region Gizmo
    public void DrawGizmos() {
        for (var i = 1; i < path.corners.Length; i++)
            Gizmos.DrawLine (path.corners [i - 1], path.corners [i]);
    }
    #endregion
}

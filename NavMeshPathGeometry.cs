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

    public NavMeshPathGeometry(NavMeshPath path) {
        this.path = path;

        Reset ();
    }

    public void Reset() {
        indexBegin = 0;
        indexEnd = path.corners.Length - 1;

        rangeBegin = 0f;
        rangeEnd = indexEnd;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DrivingOnNavMesh {
        
    public static class Quantizer {
        public static Vector3 Quantize(Vector3 v, Vector3 quant) {
            return new Vector3 (
                quant.x * Mathf.Floor (v.x / quant.x),
                quant.y * Mathf.Floor (v.y / quant.y),
                quant.z * Mathf.Floor (v.z / quant.z));
        }
    }
}

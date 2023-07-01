using System;
using Unity.Mathematics;
using UnityEngine;

namespace DrivingOnNavMesh.Poses {

	public delegate void PositionFunc(Transform tr, float3 position);
	public delegate void RotationFunc(Transform tr, quaternion rotation);

	public interface IPose {

		float3 Position {
			get; set;
		}
		quaternion Rotation {
			get; set;
		}

		float3 Forward {
			get;
		}
		PositionFunc PositionUpdator { get; set; }
		RotationFunc RotationUpdator { get; set; }

		UnityEngine.Transform GetTransform();
	}
}
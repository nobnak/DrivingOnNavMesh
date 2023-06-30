using Unity.Mathematics;
using UnityEngine;

namespace DrivingOnNavMesh.Poses {

	public class Pose : IPose {

		protected Transform root;

		public Pose(Transform root) {
			this.root = root;
		}

		public float3 Position {
			get => root.position;
			set => root.position = value;
		}
		public quaternion Rotation {
			get => root.rotation;
			set => root.rotation = value;
		}
		public float3 Forward => root.forward;
		public Transform GetTransform() => root;

		#region static
		public static implicit operator Pose(Transform tr) {
			return new Pose(tr);
		}

		#endregion
	}
}
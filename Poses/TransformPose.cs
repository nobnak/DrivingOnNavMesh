using Unity.Mathematics;
using UnityEngine;

namespace DrivingOnNavMesh.Poses {

	public class TransformPose : IPose {

		protected Transform root;

		public TransformPose(Transform root) {
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

		#region static
		public static implicit operator TransformPose(Transform tr) {
			return new TransformPose(tr);
		}

		#endregion
	}
}
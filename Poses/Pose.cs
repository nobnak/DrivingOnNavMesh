using Unity.Mathematics;
using UnityEngine;

namespace DrivingOnNavMesh.Poses {

	public class Pose : IPose {

		protected Transform root;

		public virtual PositionFunc PositionUpdator { get; set; }
		public virtual RotationFunc RotationUpdator { get; set; }

		public Pose(Transform root, PositionFunc positionUp = null, RotationFunc rotationUp = null) {
			this.root = root;
			this.PositionUpdator = positionUp;
			this.RotationUpdator = rotationUp;
		}

		public virtual float3 Position {
			get => root.position;
			set {
				if (PositionUpdator != null) 
					PositionUpdator(root, value);
				else
					root.position = value;
			}
		}
		public virtual quaternion Rotation {
			get => root.rotation;
			set {
				if (RotationUpdator != null) 
					RotationUpdator(root, value);
				else
					root.rotation = value;
			}
		}
		public virtual float3 Forward => root.forward;
		public virtual Transform GetTransform() => root;

		#region static
		public static implicit operator Pose(Transform tr) {
			return new Pose(tr);
		}

		#endregion
	}
}
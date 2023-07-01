using Unity.Mathematics;
using UnityEngine;

namespace DrivingOnNavMesh.Poses {

	public class JointPose : Pose {

		protected Transform[] arms;

		public JointPose(Transform root, 
			PositionFunc positionUp = null, RotationFunc rotationUp = null,
			params Transform[] arms)
			: base(root, positionUp, rotationUp) {
			this.arms = arms;
		}

		#region interface
#if false
		public void SetPosition(int index, float3 position) {
			float3 rootPos_wc = Position;

			float3 armPos_wc;
			if (0 <= index && index < arms.Length)
				armPos_wc = arms[index].position;
			else {
				Debug.LogWarning($"Index out of range: {index}");
				armPos_wc = rootPos_wc;
			}

			rootPos_wc = (position - armPos_wc) + rootPos_wc;
			Position = rootPos_wc;
		}
		public float3 GetPosition(int index) {
			float3 armPos_wc;
			if (0 <= index && index < arms.Length)
				armPos_wc = arms[index].position;
			else {
				Debug.LogWarning($"Index out of range: {index}");
				armPos_wc = Position;
			}
			return armPos_wc;
		}

		public void SetRotation(int index, quaternion rotation) {
			quaternion rootRot_wc = Rotation;
			quaternion armRot_wc;
			if (0 <= index && index < arms.Length)
				armRot_wc = arms[index].rotation;
			else {
				Debug.LogWarning($"Index out of range: {index}");
				armRot_wc = rootRot_wc;
			}
			rootRot_wc = math.mul(math.mul(math.inverse(armRot_wc), rotation), rootRot_wc);
			Rotation = rootRot_wc;
		}
		public quaternion GetRotation(int index) {
			quaternion armRot_wc;
			if (0 <= index && index < arms.Length)
				armRot_wc = arms[index].rotation;
			else {
				Debug.LogWarning($"Index out of range: {index}");
				armRot_wc = Rotation;
			}
			return armRot_wc;
		}
#endif

		public float3 GetForward(int index) {
			float3 armForward_wc;
			if (0 <= index && index < arms.Length)
				armForward_wc = arms[index].forward;
			else {
				Debug.LogWarning($"Index out of range: {index}");
				armForward_wc = root.forward;
			}
			return armForward_wc;
		}

		public IPose GetArm(int index) {
			return new Arm(index, this);
		}

		public Transform GetTransform(int index) {
			Transform result;
			if (0 <= index && index < arms.Length)
				result = arms[index].transform;
			else {
				Debug.LogWarning($"Index out of range: {index}");
				result = root.transform;
			}
			return result;
		}
#endregion

		public class Arm : Pose {
			protected int index;
			protected JointPose parent;

			public Arm(int index, JointPose parent) : base(parent.GetTransform(index)) {
				this.index = index;
				this.parent = parent;
			}

			public override float3 Position { 
				get => base.Position; 
				set {
					float3 rootPos_wc = parent.Position;
					float3 armPos_wc = Position;

					rootPos_wc = (value - armPos_wc) + rootPos_wc;
					parent.Position = rootPos_wc;
				}
			}
			public override quaternion Rotation {
				get => base.Rotation;
				set {
					quaternion rootRot_wc = parent.Rotation;
					quaternion armRot_wc = Rotation;

					rootRot_wc = math.mul(math.mul(math.inverse(armRot_wc), value), rootRot_wc);
					parent.Rotation = rootRot_wc;
				}
			}
		}
	}
}
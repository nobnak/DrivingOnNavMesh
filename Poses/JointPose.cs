using Unity.Mathematics;
using UnityEngine;

namespace DrivingOnNavMesh.Poses {

	public class JointPose : IPose {

		protected Transform root;
		protected Transform[] arms;

		public JointPose(Transform root, params Transform[] arms) {
			this.root = root;
			this.arms = arms;
		}

		#region IPose
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
		#endregion

		#region interface
		public void SetPosition(int index, float3 position) {
			float3 rootPos_wc = root.position;

			float3 armPos_wc;
			if (0 <= index && index < arms.Length)
				armPos_wc = arms[index].position;
			else {
				Debug.LogWarning($"Index out of range: {index}");
				armPos_wc = rootPos_wc;
			}

			rootPos_wc = (position - armPos_wc) + rootPos_wc;
			root.position = rootPos_wc;
		}
		public float3 GetPosition(int index) {
			float3 armPos_wc;
			if (0 <= index && index < arms.Length)
				armPos_wc = arms[index].position;
			else {
				Debug.LogWarning($"Index out of range: {index}");
				armPos_wc = root.position;
			}
			return armPos_wc;
		}

		public void SetRotation(int index, quaternion rotation) {
			quaternion rootRot_wc = root.rotation;
			quaternion armRot_wc;
			if (0 <= index && index < arms.Length)
				armRot_wc = arms[index].rotation;
			else {
				Debug.LogWarning($"Index out of range: {index}");
				armRot_wc = rootRot_wc;
			}
			rootRot_wc = math.mul(rootRot_wc, math.mul(rotation, math.inverse(armRot_wc)));
			root.rotation = rootRot_wc;
		}
		public quaternion GetRotation(int index) {
			quaternion armRot_wc;
			if (0 <= index && index < arms.Length)
				armRot_wc = arms[index].rotation;
			else {
				Debug.LogWarning($"Index out of range: {index}");
				armRot_wc = root.rotation;
			}
			return armRot_wc;
		}

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

		public class Arm : IPose {
			protected int index;
			protected JointPose parent;

			public Arm(int index, JointPose parent) {
				this.index = index;
				this.parent = parent;
			}

			public float3 Position {
				get => parent.GetPosition(index);
				set => parent.SetPosition(index, value);
			}
			public quaternion Rotation {
				get => parent.GetRotation(index);
				set => parent.SetRotation(index, value);
			}
			public float3 Forward => parent.GetForward(index);
			public Transform GetTransform() => parent.GetTransform(index);
		}
	}
}
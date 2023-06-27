using Unity.Mathematics;

namespace DrivingOnNavMesh.Poses {

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
	}
}
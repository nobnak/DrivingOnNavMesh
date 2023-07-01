using UnityEngine;
using System.Collections;
using DrivingOnNavMesh.Poses;
using Unity.Mathematics;

namespace DrivingOnNavMesh {

    public class RootMotionInterceptor {
        public enum TargetStateEnum { None = 0, OnTheWay }

        public const float E = 1e-2f;
        public const float SINGULAR_DOT = 1f - E;

        protected Animator anim;

        protected bool activity;
        protected TargetStateEnum targetState;
		protected float3 heading;

		protected IPose rootPose, animationPose;
        protected DrivingSetting settings;

        public RootMotionInterceptor(Animator anim, IPose rootPose, IPose animationPose, DrivingSetting settings) {
            this.rootPose = rootPose;
			this.animationPose = animationPose;
            this.anim = anim;
            this.settings = settings;

            this.activity = true;
        }

		#region properties
		public IPose RootPose => rootPose;
		public IPose AnimPose => animationPose;
		#endregion

		public virtual void SetHeading(float3 heading) {
			heading.y = 0f;
			heading = math.normalizesafe(heading, RootPose.Forward);
			this.heading = heading;
		}
        public virtual void SetActive(bool active) {
            this.activity = active;
        }

		public bool IsActive { get { return activity; } }

        public virtual float3 Heading() { return heading; }

        public virtual void CallbackOnAnimatorMove() {
            float3 nextPos = anim.rootPosition;
            quaternion nextRot = anim.rootRotation;

            if (IsActive) {
                var dt = Time.deltaTime * anim.speed;
                var view = Heading ();

                var forward = RootPose.Forward;
                forward.y = 0f;

                if (settings.MasterPositionalPower > 0f) {
                    var dpos = settings.ForwardPositionalPower * forward + settings.TargetPositionalPower * view;
                    nextPos += settings.MasterPositionalPower * dt * dpos;
                }

                if (settings.MasterRotationalPower > 0f && !IsSingularWithUp (view)) {
                    var targetRotation = Quaternion.LookRotation (view, Vector3.up);
                    var t = Mathf.Lerp (settings.ForwardRotationalPower, settings.BackwardRotationalPower, 
                                0.5f * (1f - Vector3.Dot (Vector3.forward, view)));
                    var s = settings.MasterPositionalPower * settings.CrossRotationalPower;
                    var r = settings.MasterRotationalPower * dt * (t + s);
                    nextRot = Quaternion.Slerp (nextRot, targetRotation, r);
                }
            }

            animationPose.Position = nextPos;
			animationPose.Rotation = nextRot;
        }

        protected static bool IsSingularWithUp (Vector3 view) {
            var dotUp = Vector3.Dot (Vector3.up, view);
            var singular = dotUp < -SINGULAR_DOT || SINGULAR_DOT < dotUp || view.sqrMagnitude < E;
            return singular;
        }

        protected virtual void SetTargetState(TargetStateEnum nextState) {
            targetState = nextState;
        }

        [System.Serializable]
        public class DrivingSetting {
            public float MasterPositionalPower { get; protected set; }
            public float ForwardPositionalPower { get; protected set; }
            public float TargetPositionalPower { get; protected set; }

            public float MasterRotationalPower { get; protected set; }
            public float ForwardRotationalPower { get; protected set; }
            public float BackwardRotationalPower { get; protected set; }

            public float CrossRotationalPower { get; protected set; }

            public DrivingSetting() {
                this.MasterRotationalPower = 1f;
                this.MasterPositionalPower = 0f;
                this.CrossRotationalPower = 0f;                
            }

            public DrivingSetting SetPositionalPower(Vector2 power) { return SetPositionalPower (power.x, power.y); }
            public DrivingSetting SetPositionalPower(float forwardPower, float targetPower) {
                ForwardPositionalPower = forwardPower;
                TargetPositionalPower = targetPower;
                return this;
            }
            public DrivingSetting SetMasterPositionalPower(float power) {
                MasterPositionalPower = power;
                return this;
            }

            public DrivingSetting SetRotationalPower(Vector2 power) { return SetRotationalPower (power.x, power.y); }
            public DrivingSetting SetRotationalPower(float forwardPower, float backwardPower) {
                ForwardRotationalPower = Mathf.Max(0f, forwardPower);
                BackwardRotationalPower = Mathf.Max(ForwardRotationalPower, backwardPower);
                return this;
            }
            public DrivingSetting SetMasterRotationalPower(float power) {
                MasterRotationalPower = Mathf.Max(0f, power);
                return this;
            }

            public DrivingSetting SetCrossRotationalPower(float rotationalPower) {
                CrossRotationalPower = rotationalPower;
                return this;
            }            
        }
    }
}

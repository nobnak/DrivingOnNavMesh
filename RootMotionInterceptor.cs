using UnityEngine;
using System.Collections;

namespace DrivingOnNavMesh {

    public class RootMotionInterceptor {
        public enum TargetStateEnum { None = 0, OnTheWay }

        public const float E = 1e-2f;
        public const float SINGULAR_DOT = 1f - E;

        protected Transform _tr;
        protected Animator _anim;

        protected bool _active;
        protected TargetStateEnum targetState;
        protected Vector3 _destination;

        protected DrivingSetting settings;

        public RootMotionInterceptor(Animator anim, Transform tr, DrivingSetting settings) {
            this._tr = tr;
            this._anim = anim;
            this.settings = settings;

            this._active = true;
        }

        public virtual bool SetTarget(Vector3 target) {
            SetTargetState (TargetStateEnum.OnTheWay);
            _destination = target;
            return true;
        }
        public virtual void ResetTarget() {
            SetTargetState (TargetStateEnum.None);
        }
        public virtual void SetActive(bool active) {
            _active = active;
        }

        public bool IsActive { get { return _active; } }
        public bool ActiveAndValid { get { return _active && targetState == TargetStateEnum.OnTheWay; } }

        public float SqrDistance { get { return View().sqrMagnitude; } }
        public float SqrDistanceLocal { get { return ViewLocal().sqrMagnitude; } }

        public virtual Vector3 Destination { get { return _destination; } }
        public virtual Vector3 View() { return _destination - _tr.position; }
        public virtual Vector3 ViewLocal() { return _tr.InverseTransformPoint(_destination); }

        public virtual void CallbackOnAnimatorMove() {
            var nextPos = _anim.rootPosition;
            var nextRot = _anim.rootRotation;

            if (ActiveAndValid) {
                var dt = Time.deltaTime;
                var view = View ();
                view.y = 0f;

                var forward = _tr.forward;
                forward.y = 0f;

                if (settings.MasterPositionalPower > 0f) {
                    var dpos = settings.ForwardPositionalPower * forward + settings.TargetPositionalPower * view.normalized;
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

            _tr.position = nextPos;
            _tr.rotation = nextRot;
        }

        protected static bool IsSingularWithUp (Vector3 view) {
            var dotUp = Vector3.Dot (Vector3.up, view);
            var singular = dotUp < -SINGULAR_DOT || SINGULAR_DOT < dotUp || view.sqrMagnitude < E;
            return singular;
        }

        protected virtual void SetTargetState(TargetStateEnum nextState) {
            targetState = nextState;
        }

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

using UnityEngine;
using System.Collections;

namespace DrivingOnNavMesh {

    public class RootMotionInterceptor {
        public const float E = 1e-2f;
        public const float SINGULAR_DOT = 1f - E;

        protected Transform _tr;
        protected Animator _anim;

        protected bool _active;
        protected Vector3 _destination;

        protected float _masterPositionalPower;
        protected float _forwardPositionalPower;
        protected float _targetPositionalPower;

        protected float _masterRotationalPower;
        protected float _forwardRotationalPower;
        protected float _backwardRotationalPower;

        protected float _crossRotationalPower;

        public RootMotionInterceptor(Animator anim, Transform tr) {
            this._tr = tr;
            this._anim = anim;
            this._masterRotationalPower = 1f;
            this._masterPositionalPower = 0f;
            this._crossRotationalPower = 0f;
        }

        public virtual bool SetTarget(Vector3 target) {
            _active = true;
            _destination = target;
            return true;
        }
        public virtual bool SetActive(bool active) {
            _active = active;
            return true;
        }

        public RootMotionInterceptor SetPositionalPower(Vector2 power) { return SetPositionalPower (power.x, power.y); }
        public RootMotionInterceptor SetPositionalPower(float forwardPower, float targetPower) {
            _forwardPositionalPower = forwardPower;
            _targetPositionalPower = targetPower;
            return this;
        }
        public RootMotionInterceptor SetMasterPositionalPower(float power) {
            _masterPositionalPower = power;
            return this;
        }

        public RootMotionInterceptor SetRotationalPower(Vector2 power) { return SetRotationalPower (power.x, power.y); }
        public RootMotionInterceptor SetRotationalPower(float forwardPower, float backwardPower) {
            _forwardRotationalPower = Mathf.Max(0f, forwardPower);
            _backwardRotationalPower = Mathf.Max(_forwardRotationalPower, backwardPower);
            return this;
        }
        public RootMotionInterceptor SetMasterRotationalPower(float power) {
            _masterRotationalPower = Mathf.Max(0f, power);
            return this;
        }

        public RootMotionInterceptor SetCrossRotationalPower(float rotationalPower) {
            _crossRotationalPower = rotationalPower;
            return this;
        }

        public bool IsActive { get { return _active; } }
        public float SqrDistance { get { return View().sqrMagnitude; } }
        public float SqrDistanceLocal { get { return ViewLocal().sqrMagnitude; } }

        public virtual Vector3 Destination { get { return _destination; } }
        public virtual Vector3 View() { return _destination - _tr.position; }
        public virtual Vector3 ViewLocal() { return _tr.InverseTransformPoint(_destination); }

        public virtual void CallbackOnAnimatorMove() {
            var nextPos = _anim.rootPosition;
            var nextRot = _anim.rootRotation;

            if (_active) {
                var dt = Time.deltaTime;
                var view = View ();
                view.y = 0f;

                var forward = _tr.forward;
                forward.y = 0f;

                if (_masterPositionalPower > 0f) {
                    var dpos = _forwardPositionalPower * forward + _targetPositionalPower * view.normalized;
                    nextPos += _masterPositionalPower * dt * dpos;
                }

                if (_masterRotationalPower > 0f && !IsSingularWithUp(view)) {
                    var targetRotation = Quaternion.LookRotation (view, Vector3.up);
                    var t = Mathf.Lerp (_forwardRotationalPower, _backwardRotationalPower, 
                        0.5f * (1f - Vector3.Dot (Vector3.forward, view)));
                    var s = _masterPositionalPower * _crossRotationalPower;
                    var r = _masterRotationalPower * dt * (t + s);
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
    }
}

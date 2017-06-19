using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace DrivingOnNavMesh {

    public class NavigatedRootMotionInterceptor : RootMotionInterceptor {
        public enum StateEnum { None = 0, DuringNavigaion }

        public event System.Action<NavigatedRootMotionInterceptor> NavigationStart;
        public event System.Action<NavigatedRootMotionInterceptor> NavigationComplete;
        public event System.Action<NavigatedRootMotionInterceptor> NavigationAbort;
        public event System.Action<NavigatedRootMotionInterceptor> NavigationExit;

        protected DrivingAndPathSetting motionData;
        protected NavMeshPathGeometry path;
        protected StateEnum state;

        public NavigatedRootMotionInterceptor(Animator anim, Transform tr, DrivingAndPathSetting motionData)
            : base(anim, tr, motionData) {
            this.motionData = motionData;
            this.path = new NavMeshPathGeometry (new NavMeshPath ());
        }

        public virtual bool NavigateTo(Vector3 destination) {
            var result = path.CalculatePath (_tr.position, destination);
            if (result)
                StartNavigation ();
            return result;
        }
    	public virtual void UpdateNavigation () {
            if (state != StateEnum.DuringNavigaion)
                return;
            
            var center = _tr.position;
            var t = path.NearestCorner (center);
            if (t < 0f) {
                AbortNavigation ();
                return;
            }

            path.ActiveRange.SetRangeBegin (t);
            if (path.ActiveRange.Length <= Mathf.Epsilon) {
                CompleteNavigation ();
                return;
            }

            var pointOn = path.PointAt (t);
            var tangentOn = path.TangentAt (t);
            var toPoint = pointOn - center;
            var moveBy = motionData.DestinationDistance * tangentOn;
            if (toPoint.sqrMagnitude > Mathf.Epsilon) {
                var distance = toPoint.magnitude;
                var toPointDir = toPoint.normalized;
                var angle = 0.5f * (1f - Vector3.Dot (_tr.forward, toPointDir));
                var distantialRatio = motionData.PathFollowingDistanceRatio * distance;
                var angularRatio = motionData.PathFollowingAngleRatio;
                var mixRatio = Mathf.Clamp01 (Mathf.Lerp (distantialRatio, angularRatio, angle));
                moveBy = motionData.DestinationDistance * Vector3.Lerp (tangentOn, toPointDir, mixRatio);
            }
            SetTarget (center + moveBy);
        }
        public void DrawPath() {
            if (path != null || state != StateEnum.DuringNavigaion)
                path.DrawGizmos();
        }
        public void DrawTarget() {
            if (ActiveAndValid)
                Gizmos.DrawLine (_tr.position, Destination);
        }

        protected virtual void GotoNavigationState(StateEnum nextState) {
            state = nextState;
        }

        #region Action
        protected virtual void StartNavigation() {
            if (state != StateEnum.None)
                AbortNavigation ();
            
            GotoNavigationState(StateEnum.DuringNavigaion);
            NotifyNavigationStart ();
        }
        protected virtual void CompleteNavigation() {
            if (state == StateEnum.DuringNavigaion) {
                GotoNavigationState(StateEnum.None);
                ResetTarget ();
                NotifyNavigationComplete ();
                NotifyNavigationExit ();
            }
        }
        protected virtual void AbortNavigation() {
            if (state == StateEnum.DuringNavigaion) {
                GotoNavigationState(StateEnum.None);
                ResetTarget ();
                NotifyNavigationAbort ();
                NotifyNavigationExit ();
            }
        }
        #endregion

        #region Event
        protected void NotifyNavigationStart() {
            if (NavigationStart != null)
                NavigationStart (this);
        }
        protected void NotifyNavigationAbort() {
            if (NavigationAbort != null)
                NavigationAbort (this);
        }
        protected void NotifyNavigationComplete() {
            if (NavigationComplete != null)
                NavigationComplete (this);
        }
        protected void NotifyNavigationExit() {
            if (NavigationExit != null)
                NavigationExit (this);
        }
        #endregion

        [System.Serializable]
        public class DrivingAndPathSetting : RootMotionInterceptor.DrivingSetting {
            public float DestinationDistance { get; protected set; }
            public float PathFollowingAngleRatio { get; protected set; }
            public float PathFollowingDistanceRatio { get; protected set; }

            public DrivingAndPathSetting() : base() {
                this.DestinationDistance = 1f;
                this.PathFollowingAngleRatio = 0.1f;
                this.PathFollowingDistanceRatio = 0.5f;
            }           

            public DrivingAndPathSetting SetDestinationDistance(float v) {
                DestinationDistance = v;
                return this;
            }
            public DrivingAndPathSetting SetPathFollowingAngleRatio(float v) {
                PathFollowingAngleRatio = v;
                return this;
            }
            public DrivingAndPathSetting SetPathFollowingDistanceRatio(float v) {
                PathFollowingDistanceRatio = v;
                return this;
            }
        }
    }
}

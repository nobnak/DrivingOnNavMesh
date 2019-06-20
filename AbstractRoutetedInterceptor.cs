using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace DrivingOnNavMesh {

    public abstract class AbstractRoutetedInterceptor : RootMotionInterceptor {
        public const float RADIUS = 10f;
        public enum StateEnum { Idol = 0, Navigation }

        public event System.Action<AbstractRoutetedInterceptor> NavigationStart;
        public event System.Action<AbstractRoutetedInterceptor> NavigationComplete;
        public event System.Action<AbstractRoutetedInterceptor> NavigationAbort;
        public event System.Action<AbstractRoutetedInterceptor> NavigationExit;

        protected StateEnum state;
        protected AbstractRouter router;

		protected float acceptableRemainingRatio = 0.05f;

        public AbstractRoutetedInterceptor(
            Animator anim, Transform tr, DrivingSetting drivingSetting, AbstractRouter router)
             : base(anim, tr, drivingSetting) {
            this.router = router;
        }

        #region Abstract
        protected abstract bool TryToStartNavigationTo (Vector3 destination);
        protected abstract void UpdateTarget (Vector3 pointFrom, float t);
        #endregion

		public virtual Vector3 CurrentDestination { get; protected set; }
		public virtual float AcceptableRemainingRatio {
			get { return acceptableRemainingRatio; }
			set {
				if (0f <= value && value <= 1f)
					acceptableRemainingRatio = value;
			}
		}
        public virtual void SetActive(bool active, bool clear = true) {
            if (clear)
                AbortNavigation();
            base.SetActive(active);
        }

        public virtual bool NavigateTo(Vector3 destination) {
            AbortNavigation ();
            var result = TryToStartNavigationTo (destination);
			if (result) {
				CurrentDestination = destination;
				StartNavigation();
			}
            return result;
        }
        public virtual bool Update () {
            if (state != StateEnum.Navigation)
                return false;

            var pointFrom = tr.position;
			var t = router.ClosestT(pointFrom);
			t = Mathf.Max(t, router.ActiveRange.ActiveRangeBegin);
            if (t < 0f) {
                AbortNavigation ();
                return false;
            }

            router.ActiveRange.SetRangeBegin (t);
            if (router.ActiveRange.Length <= Mathf.Epsilon || (t + acceptableRemainingRatio) >= 1f) {
                CompleteNavigation ();
                return false;
            }

            UpdateTarget (pointFrom, t);
            return true;
        }
        public virtual void Abort() {
            AbortNavigation();
        }

        public void DrawPath() {
            if (router != null || state == StateEnum.Navigation)
                router.DrawGizmos();
        }
        public void DrawTarget() {
            if (ActiveAndValid)
                Gizmos.DrawLine (tr.position, CurrentTarget);
        }
        public StateEnum CurrentState { get { return state; } }

        protected virtual void GotoNavigationState(StateEnum nextState) {
            state = nextState;
        }

        #region Action
        protected virtual void StartNavigation() {
            if (state == StateEnum.Idol) {
                GotoNavigationState (StateEnum.Navigation);
                NotifyNavigationStart ();
            }
        }
        protected virtual void CompleteNavigation() {
            if (state == StateEnum.Navigation) {
                GotoNavigationState(StateEnum.Idol);
                ResetTarget ();
                NotifyNavigationComplete ();
                NotifyNavigationExit ();
            }
        }
        protected virtual void AbortNavigation() {
            if (state == StateEnum.Navigation) {
                GotoNavigationState(StateEnum.Idol);
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

    }
}

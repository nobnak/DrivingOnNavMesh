using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace DrivingOnNavMesh {

    public abstract class AbstractRoutetedInterceptor {
        public const float RADIUS = 10f;
        public enum StateEnum { Idol = 0, Navigation }

        public event System.Action<AbstractRoutetedInterceptor> NavigationStart;
        public event System.Action<AbstractRoutetedInterceptor> NavigationComplete;
        public event System.Action<AbstractRoutetedInterceptor> NavigationAbort;
        public event System.Action<AbstractRoutetedInterceptor> NavigationExit;

		protected RootMotionInterceptor rootMotion;

		protected StateEnum state;
        protected AbstractRouter router;
		protected float3 destination;

		protected float acceptableRemainingRatio = 0.05f;

        public AbstractRoutetedInterceptor(RootMotionInterceptor rootMotion, AbstractRouter router) {
			this.rootMotion = rootMotion;
            this.router = router;
        }

        #region Abstract
        protected abstract bool TryToStartNavigationTo (float3 destination);
        protected abstract void UpdateTarget (float3 pointFrom, float t);
		#endregion

		public virtual float3 CurrentDestination => destination;
		public virtual bool SetDestination(float3 destination) {
			this.destination = destination;
			var heading = destination - rootMotion.Tr.Position;
			rootMotion.SetHeading(heading);
			return true;
		}

		public virtual float AcceptableRemainingRatio {
			get { return acceptableRemainingRatio; }
			set {
				if (0f <= value && value <= 1f)
					acceptableRemainingRatio = value;
			}
		}

		public virtual bool IsActive => rootMotion.IsActive;
        public virtual void SetActive(bool active, bool clear = true) {
            if (clear)
                AbortNavigation();
			rootMotion.SetActive(active);
        }

        public virtual bool NavigateTo(Vector3 destination) {
            var result = TryToStartNavigationTo (destination);
			if (result) {
				SetDestination(destination);
				StartNavigation();
			}
            return result;
        }
        public virtual bool Update () {
            if (state != StateEnum.Navigation)
                return false;

            var pointFrom = rootMotion.Tr.Position;
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
            if (rootMotion.IsActive)
                Gizmos.DrawLine (rootMotion.Tr.Position, CurrentDestination);
        }
        public StateEnum CurrentState { get { return state; } }

        protected virtual void GotoNavigationState(StateEnum nextState) {
            state = nextState;
        }

        #region Action
        protected virtual void StartNavigation() {
			GotoNavigationState (StateEnum.Navigation);
			rootMotion.SetActive(true);
			NotifyNavigationStart ();
        }
        protected virtual void CompleteNavigation() {
            if (state == StateEnum.Navigation) {
                GotoNavigationState(StateEnum.Idol);
                rootMotion.SetActive (false);
                NotifyNavigationComplete ();
                NotifyNavigationExit ();
            }
        }
        protected virtual void AbortNavigation() {
            if (state == StateEnum.Navigation) {
                GotoNavigationState(StateEnum.Idol);
				rootMotion.SetActive(false);
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

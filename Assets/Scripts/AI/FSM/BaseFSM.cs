using System;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

namespace FSM
{

    public abstract class BaseFSM<EState> : MonoBehaviour where EState : Enum
    {
        [SerializeField]
        protected Dictionary<EState, BaseState<EState>> States = new();
        [SerializeField]
        protected BaseState<EState> CurrentState;
        [SerializeField]
        protected float UpdateDeltaTime = 0.1f;
        private float NextUpdateTime;

        protected void Awake()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                BaseState<EState> state;
                if (transform.GetChild(i).TryGetComponent<BaseState<EState>>(out state))
                    States.Add(state.StateKey, state);
            }
        }
        // Start is called before the first frame update
        void Start()
        {
            CurrentState.EnterState();
            NextUpdateTime =Time.time + UpdateDeltaTime;
        }

        // Update is called once per frame
        void Update()
        {

            if(Time.time < NextUpdateTime)
                return;
            NextUpdateTime += UpdateDeltaTime;

            EState nextStateKey = CurrentState.GetNextSate();
            if (nextStateKey.Equals(CurrentState.StateKey))
            {
                CurrentState.UpdateState();
            }
            else
            {
                TransitionToState(nextStateKey);
            }
        }

        private void TransitionToState(EState nextStateKey)
        {
            CurrentState.ExitState();
            CurrentState = States[nextStateKey];
            CurrentState.EnterState();
        }

        private void OnTriggerEnter(Collider other)
        {
            CurrentState.OnTriggerEnter(other);
        }
        private void OnTriggerExit(Collider other)
        {
            CurrentState.OnTriggerExit(other);
        }
        private void OnTriggerStay(Collider other)
        {
            CurrentState.OnTriggerStay(other);
        }
    }
}

using System;
using UnityEngine;

namespace FSM
{
    public abstract class BaseState<EState> : MonoBehaviour where EState : Enum
    {
        protected bool IsActive = false;
        public BaseState(EState key)
        {
            StateKey = key;
        }
        public EState StateKey { get; private set; }
        public virtual void EnterState() 
        { 
            IsActive = true; 
        }
        public virtual void ExitState()
        {
            IsActive = false;
        }
        public abstract void UpdateState();
        public abstract EState GetNextSate();
        public abstract void OnTriggerEnter(Collider other);
        public abstract void OnTriggerExit(Collider other);
        public abstract void OnTriggerStay(Collider other);

    }
}
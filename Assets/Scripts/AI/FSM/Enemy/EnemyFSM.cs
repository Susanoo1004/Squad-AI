using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FSM
{

    public class EnemyFSM : BaseFSM<EnemyFSM.EnemyState>
    {
        public enum EnemyState
        {
            IDLE,
            CHASE,
            PATROL,
            SHOOT,
            FLEE
        }
        protected void Awake()
        {
            //base.Awake();
            for (int i = 0; i < transform.childCount; i++)
            {
                BaseState<EnemyState> state;
                if (transform.GetChild(i).TryGetComponent(out state))
                    States.Add(state.StateKey, state);
            }
            CurrentState = States[EnemyState.IDLE];
        }
    }
}
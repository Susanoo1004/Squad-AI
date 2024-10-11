using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FSM
{

    public class AIAgentFSM : BaseFSM<AIAgentFSM.AIState>
    {
        public enum AIState
        {
            IDLE,
            FOLLOW,
            SUPPORT,
            COVER,
            PROTECT,
            HEAL
        }
        protected void Awake()
        {
            //base.Awake();
            for (int i = 0; i < transform.childCount; i++)
            {
                BaseState<AIAgentFSM.AIState> state;
                if (transform.GetChild(i).TryGetComponent<BaseState<AIAgentFSM.AIState>>(out state))
                    States.Add(state.StateKey, state);
            }
            CurrentState = States[AIState.IDLE];
        }
    }

}
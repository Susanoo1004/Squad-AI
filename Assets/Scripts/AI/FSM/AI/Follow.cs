using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using FSMMono;
using UnityEngine.Windows;

namespace FSM
{

    namespace AI
    {
        using static AIAgentFSM.AIState;

        public class Follow : BaseState<AIAgentFSM.AIState>
        {
            AIAgentFSM.AIState NextState = FOLLOW;
            AIAgent AIAgent;
            SimpleController Inputs;

            public Follow() : base(FOLLOW)
            { }
            private void Awake()
            {
                AIAgent = transform.parent.parent.GetComponent<AIAgent>();
                Inputs = FindAnyObjectByType<SimpleController>();
                Inputs.OnMouseRightClicked += HandleBarrageFireInput;
            }

            private void OnDestroy()
            {
                Inputs.OnMouseRightClicked -= HandleBarrageFireInput;
            }
            public override void EnterState()
            {
                NextState = FOLLOW; 
                AIAgent.MoveToSquadTarget();
            }

            public override void ExitState()
            {
                AIAgent.StopMove();
            }
            public override AIAgentFSM.AIState GetNextSate()
            {
                return NextState;
            }

            public override void OnTriggerEnter(Collider other)
            {
            }

            public override void OnTriggerExit(Collider other)
            {
                if (other.gameObject.tag == "Player")
                    NextState = IDLE;
            }

            public override void OnTriggerStay(Collider other)
            {
            }

            public override void UpdateState()
            {
                if (AIAgent.HasReachedPos())
                {
                    NextState = IDLE;
                }

            }
            void HandleBarrageFireInput(Vector3 target)
            {
                if (!Inputs.IsBarrageMode) //Important
                    return;
                NextState = BARRAGE;
                AIAgent.ShootingTarget = target;
            }
        }
    }
}
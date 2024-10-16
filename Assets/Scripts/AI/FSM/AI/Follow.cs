using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using FSMMono;
using UnityEngine;

namespace FSM
{

    namespace AI
    {
        using static AIAgentFSM.AIState;

        public class Follow : BaseState<AIAgentFSM.AIState>
        {
            AIAgentFSM.AIState NextState = FOLLOW;
            AIAgent AIAgent;
            bool IsWaitingNewTarget = false;
            Vector3 oldTarget;

            public Follow() : base(FOLLOW)
            { }
            private void Awake()
            {
                AIAgent = transform.parent.parent.GetComponent<AIAgent>();
            }
            public override void EnterState()
            {
                NextState = FOLLOW;
                IsWaitingNewTarget = true;
                StartCoroutine(CoroutineNextTargetMove()); 
            }

            public override void ExitState()
            {
                oldTarget = AIAgent.Target;
                AIAgent.StopMove();
            }
            IEnumerator CoroutineNextTargetMove()
            {
                yield return new WaitUntil(() => oldTarget != AIAgent.Target);
                IsWaitingNewTarget = false;
            }
            public override AIAgentFSM.AIState GetNextSate()
            {
                return NextState;
            }

            public override void OnTriggerEnter(Collider other)
            {
                if (other.gameObject.layer == LayerMask.NameToLayer("Enemies"))
                {
                    NextState = SUPPORT;
                    AIAgent.RegisteredEnemy = other.gameObject.transform;
                }
            }

            public override void OnTriggerExit(Collider other)
            {
                if (other.gameObject.tag == "Player")
                    NextState = FOLLOW;
            }

            public override void OnTriggerStay(Collider other)
            {
            }

            public override void UpdateState()
            {
                AIAgent.MoveToTarget();


                if (!IsWaitingNewTarget && AIAgent.HasReachedPos())
                {
                    NextState = IDLE;
                }

            }
        }
    }
}
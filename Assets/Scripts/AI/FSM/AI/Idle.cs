
using FSMMono;
using UnityEngine;

namespace FSM
{

    namespace AI
    {
        using static AIAgentFSM.AIState;
        public class Idle : BaseState<AIAgentFSM.AIState>
        {
            AIAgentFSM.AIState NextState = IDLE;
            AIAgent AIAgent;
            public Idle() : base(IDLE)
            { }
            private void Awake()
            {
                AIAgent = transform.parent.parent.GetComponent<AIAgent>();
            }
            public override void EnterState()
            {
                NextState = IDLE;
                AIAgent.StopMove();
            }

            public override void ExitState()
            {

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

            }
        }
    }
}
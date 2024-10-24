
using FSMMono;
using UnityEngine;

namespace FSM
{

    namespace AI
    {
        using static AIAgentFSM.AIState;
        public class Support : BaseState<AIAgentFSM.AIState>
        {
            AIAgentFSM.AIState NextState = SUPPORT;
            AIAgent AIAgent;
            public Support() : base(SUPPORT)
            { }
            private void Awake()
            {
                AIAgent = transform.parent.parent.GetComponent<AIAgent>();
            }
            public override void EnterState()
            {
                NextState = IDLE;
                AIAgent.ShootToPosition(AIAgent.ShootingTarget);
                AIAgent.SetMaterial(Color.black);
            }

            public override void ExitState()
            {
                AIAgent.SetDefaultMaterial();
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
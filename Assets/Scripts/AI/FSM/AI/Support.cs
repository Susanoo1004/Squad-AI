
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
            int EnemiesInRange = 1;
            public Support() : base(SUPPORT)
            { }
            private void Awake()
            {
                AIAgent = transform.parent.parent.GetComponent<AIAgent>();
            }
            public override void EnterState()
            {
                NextState = SUPPORT;
                AIAgent.StopMove();
                AIAgent.ShootRegisteredEnemy();
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
                    EnemiesInRange++;
            }

            public override void OnTriggerExit(Collider other)
            {
                if (other.gameObject.layer == LayerMask.NameToLayer("Enemies"))
                {
                    EnemiesInRange--;
                    if (EnemiesInRange == 0)
                        NextState = IDLE;
                }
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
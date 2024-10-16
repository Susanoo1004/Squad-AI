using UnityEngine;

using FSMMono;

namespace FSM
{

    namespace AI
    {
        using static AIAgentFSM.AIState;

        public class Protect : BaseState<AIAgentFSM.AIState>
        {
            AIAgentFSM.AIState NextState = PROTECT;
            AIAgent AIAgent;
            [SerializeField]
            float Distance = 1f;
            Transform Enemy;
            Transform Player;

            public Protect() : base(PROTECT)
            { }
            private void Awake()
            {
                AIAgent = transform.parent.parent.GetComponent<AIAgent>();
                Player = FindAnyObjectByType<PlayerAgent>().transform;
            }
            public override void EnterState()
            {
                Enemy = AIAgent.RegisteredEnemy;
                if (Enemy == null || (Player.position - Enemy.position).magnitude < Distance)
                    NextState = IDLE;
                else
                    NextState = PROTECT;

                AIAgent.MoveTo(Player.position + (Player.position - Enemy.position).normalized * Distance);
            }

            public override void ExitState()
            {
                AIAgent.RegisteredEnemy = null;
                AIAgent.StopMove();
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
                    NextState = PROTECT;
            }

            public override void OnTriggerStay(Collider other)
            {
            }

            public override void UpdateState()
            {
                if (!Enemy)
                {
                    NextState = IDLE;
                    return;
                }
                if (AIAgent.HasReachedPos())
                {
                    AIAgent.ShootRegisteredEnemy();
                }
                else
                {
                    AIAgent.MoveTo(Player.position + (Player.position - Enemy.position).normalized * Distance);
                }
            }

        }
    }
}

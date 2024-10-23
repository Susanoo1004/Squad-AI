using UnityEngine;

using FSMMono;
using UnityEngine.Windows;
using Squad;

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
            SimpleController Inputs;
            SquadController SquadController;

            public Protect() : base(PROTECT)
            { }
            private void Awake()
            {
                AIAgent = transform.parent.parent.GetComponent<AIAgent>();
                Player = FindAnyObjectByType<PlayerAgent>().transform;
                Inputs = FindAnyObjectByType<SimpleController>();
                Inputs.OnMouseRightClicked += HandleBarrageFireInput;
                AIAgent.OnDeath += HandleDeath;
            }
            private void Start()
            {
                SquadController = AIAgent.transform.parent.GetComponent<SquadController>();
            }
            private void OnDestroy()
            {
                Inputs.OnMouseRightClicked -= HandleBarrageFireInput;
                AIAgent.OnDeath -= HandleDeath;
            }
            public override void EnterState()
            {
                Enemy = AIAgent.RegisteredEnemy;
                if (Enemy == null || (Player.position - Enemy.position).magnitude < Distance)
                {
                    NextState = IDLE;
                    return;
                }
                else
                    NextState = PROTECT;
                //AddProtector();
                AIAgent.MoveTo(Player.position + (Enemy.position - Player.position).normalized * Distance);
            }

            public override void ExitState()
            {
                if (SquadController.Protector == AIAgent)
                    RemoveProtector();

                AIAgent.RegisteredEnemy = null;
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
                if (!Enemy)
                {
                    //RemoveProtector();
                    NextState = IDLE;
                    return;
                }
                if (AIAgent.HasReachedPos())
                {
                    AIAgent.StopMove();
                    AIAgent.ShootRegisteredEnemy();
                }
                else
                {
                    AIAgent.MoveTo(Player.position + (Enemy.position - Player.position).normalized * Distance);
                }
            }
            void HandleBarrageFireInput(Vector3 target)
            {
                if (!Inputs.IsBarrageMode) //Important
                    return;
                RemoveProtector();
                NextState = BARRAGE;
                AIAgent.ShootingTarget = target;
            }
            void AddProtector()
            {
                SquadController.Protector = AIAgent;
            }
            void RemoveProtector()
            {
                SquadController.Protector = null;
            }
            void HandleDeath()
            {
                RemoveProtector();
            }
        }
    }
}

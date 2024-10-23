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
            bool IsPlayerDetected = false; //Debug purposes

            public Protect() : base(PROTECT)
            { }
            private void Awake()
            {
                AIAgent = transform.parent.parent.GetComponent<AIAgent>();
                Player = FindAnyObjectByType<PlayerAgent>().transform;
                Inputs = FindAnyObjectByType<SimpleController>();
                AIAgent.OnDeath += HandleDeath;
            }
            private void Start()
            {
                SquadController = AIAgent.transform.parent.GetComponent<SquadController>();
            }
            private void OnDestroy()
            {
                AIAgent.OnDeath -= HandleDeath;
            }
            public override void EnterState()
            {
                Enemy = AIAgent.RegisteredEnemy;
                if (Enemy == null || (Player.position - Enemy.position).sqrMagnitude < Distance * Distance)
                {
                    NextState = IDLE;
                    return;
                }
                else
                    NextState = PROTECT;
                AddProtector(); //Not necessary, safety purpose
                AIAgent.MoveTo(Player.position + (Enemy.position - Player.position).normalized * Distance);
            }

            public override void ExitState()
            {
                if (SquadController.Protector == AIAgent) //Should always be the case, however safety first
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
                if (other.gameObject.tag == "Player")
                    IsPlayerDetected = true;
            }

            public override void OnTriggerExit(Collider other)
            {
                if (other.gameObject.tag == "Player")
                {
                    IsPlayerDetected = false;
                    NextState = IDLE;
                }
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

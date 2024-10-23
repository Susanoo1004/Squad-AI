using UnityEngine;

using FSMMono;
using UnityEngine.Windows;
using Squad;

namespace FSM
{

    namespace AI
    {
        using static AIAgentFSM.AIState;

        public class Heal : BaseState<AIAgentFSM.AIState>
        {
            AIAgentFSM.AIState NextState = HEAL;
            AIAgent AIAgent;
            [SerializeField]
            float Distance = 1f;
            Transform Enemy;
            Transform Player;
            SquadController SquadController;

            private float mHealTimer;
            private float mHealDelay = 0.2f;
            private float mHealRadius = 1f;

            public Heal() : base(HEAL)
            { }
            private void Awake()
            {
                AIAgent = transform.parent.parent.GetComponent<AIAgent>();
                Player = FindAnyObjectByType<PlayerAgent>().transform;
                mHealTimer = mHealDelay;
            }
            private void Start()
            {
                SquadController = AIAgent.transform.parent.GetComponent<SquadController>();
            }
            private void OnDestroy()
            {

            }
            public override void EnterState()
            {
                NextState = HEAL;
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
            }

            public override void OnTriggerExit(Collider other)
            {

            }

            public override void OnTriggerStay(Collider other)
            {
            }

            public override void UpdateState()
            {
                Vector3 v = AIAgent.transform.position - Player.position;

                if (v.magnitude > mHealRadius)
                {
                    AIAgent.MoveTo(Player.position + v.normalized * mHealRadius * 0.8f);
                }
                else
                {
                    mHealTimer -= Time.deltaTime;

                    if (mHealTimer < 0)
                    {
                        mHealTimer = mHealDelay;

                        if (!Player.GetComponent<PlayerAgent>().Heal(1))
                            NextState = IDLE;
                    }
                }
            }
        }
    }
}

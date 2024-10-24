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

            [SerializeField]
            private float HealTimer;
            [SerializeField]
            private uint HealAmount = 1;
            [SerializeField]
            private float HealDelay = 0.2f;
            [SerializeField]
            private float HealRadius = 2.5f;
            [SerializeField]
            private float RadiusToleranceRatio = 0.8f;

            [SerializeField] GameObject HealParticle;

            public Heal() : base(HEAL)
            { }
            private void Awake()
            {
                AIAgent = transform.parent.parent.GetComponent<AIAgent>();
                Player = FindAnyObjectByType<PlayerAgent>().transform;
                HealTimer = HealDelay;
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
                AIAgent.SetMaterial(Color.green);
            }

            public override void ExitState()
            {
                SquadController.Healer = null;
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
                Vector3 v = AIAgent.transform.position - Player.position;
                if (v.magnitude > HealRadius)
                {
                    AIAgent.MoveTo(Player.position + v.normalized * HealRadius * RadiusToleranceRatio);
                }
                else
                {

                    HealTimer -= AIAgentFSM.AIStateUpdateDeltaTime;// UpdateState DeltaTime 

                    if (HealTimer < 0)
                    {
                        HealTimer = HealDelay;

                        GameObject finalExplo = Instantiate(HealParticle);
                        finalExplo.transform.position = transform.position;
                        Destroy(finalExplo, 0.5f);

                        if (!Player.GetComponent<PlayerAgent>().Heal((int)HealAmount))
                            NextState = IDLE;
                    }
                }
            }
        }
    }
}

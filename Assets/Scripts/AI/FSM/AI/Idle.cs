
using FSMMono;
using Squad;
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
            SquadController SquadController;

            PlayerAgent Player;
            SimpleController Inputs;

            public Idle() : base(IDLE)
            { }
            bool playerDetected = false;

            private void Awake()
            {
                Inputs = FindAnyObjectByType<SimpleController>();
                AIAgent = transform.parent.parent.GetComponent<AIAgent>();
                SquadController = AIAgent.transform.parent.GetComponent<SquadController>();
                Player = FindAnyObjectByType(typeof(PlayerAgent)) as PlayerAgent;
                #region Events //Can be better
                //Player.OnDamageTaken += HandlePlayerDamaged;
                //Inputs.OnMouseLeftClicked += HandleSupportFireInput;
                //Inputs.OnMouseLeftHold += HandleSupportFireInput;
                //Inputs.OnMouseRightClicked += HandleBarrageFireInput;
                SquadController.OnMoving += HandleSquadMoving;
                #endregion //Events Can be better
            }

            private void OnDestroy()
            {
                #region Events //Can be better
                //Player.OnDamageTaken -= HandlePlayerDamaged;
                //Inputs.OnMouseLeftClicked -= HandleSupportFireInput;
                //Inputs.OnMouseLeftHold -= HandleSupportFireInput;
                //Inputs.OnMouseRightClicked -= HandleBarrageFireInput;
                SquadController.OnMoving -= HandleSquadMoving;

                #endregion //Events Can be better
            }
            public override void EnterState()
            {
                //if (playerDetected)
                    NextState = IDLE;
                //else
                    //NextState = FOLLOW;

                AIAgent.StopMove();
            }

            public override void ExitState()
            {

            }

            public override AIAgentFSM.AIState GetNextSate()
            {
                return NextState;
            }

            public void HandleSquadMoving()
            {
                NextState = FOLLOW;
            }
            public void HandlePlayerDamaged(GameObject source)
            {
                NextState = PROTECT;
                AIAgent.RegisteredEnemy = source.transform;
            }

            void HandleSupportFireInput(Vector3 target)
            {
                NextState = SUPPORT;
                AIAgent.ShootingTarget = target;
            }
            void HandleBarrageFireInput(Vector3 target)
            {
                if (!Inputs.IsBarrageMode) //Important
                    return;
                NextState = BARRAGE;
                AIAgent.ShootingTarget = target;
            }
            public override void OnTriggerEnter(Collider other)
            {
                //if (other.gameObject.layer == LayerMask.NameToLayer("Enemies"))
                //{
                //    NextState = SUPPORT;
                //    AIAgent.RegisteredEnemy = other.gameObject.transform;
                //}
                if (other.gameObject.tag == "Player")
                {
                    playerDetected = true;
                }
            }

            public override void OnTriggerExit(Collider other)
            {
                if (other.gameObject.tag == "Player")
                {
                    playerDetected = false;
                    //NextState = FOLLOW;
                }
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
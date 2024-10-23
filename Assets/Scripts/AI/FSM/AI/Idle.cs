
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
                Player.OnShooting += HandleSupportFireInput;
                //Inputs.OnMouseRightClicked += HandleBarrageFireInput;
                SquadController.OnMoving += HandleSquadMoving;
                #endregion //Events Can be better
            }

            private void OnDestroy()
            {
                #region Events //Can be better
                //Player.OnDamageTaken -= HandlePlayerDamaged;
                Player.OnShooting -= HandleSupportFireInput;
                //Inputs.OnMouseRightClicked -= HandleBarrageFireInput;
                SquadController.OnMoving -= HandleSquadMoving;

                #endregion //Events Can be better
            }
            public override void EnterState()
            {
                base.EnterState();
                AIAgent.StopMove();
            }

            public override void ExitState()
            {
                base.ExitState();
                NextState = IDLE;
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
                if (!IsActive)
                    return;
                NextState = PROTECT;
                AIAgent.RegisteredEnemy = source.transform;
            }

            void HandleSupportFireInput(Vector3 target)
            {
                if (!IsActive)
                    return;
                AIAgent.ShootingTarget = target;
                NextState = SUPPORT;
            }
            void HandleBarrageFireInput(Vector3 target)
            {
                if (!IsActive || !Inputs.IsBarrageMode) //Important
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
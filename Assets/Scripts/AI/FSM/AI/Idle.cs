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

            ISquadLeader Leader;
            SimpleController Inputs;

            public Idle() : base(IDLE)
            { }
            bool playerDetected = false; //Debug purposes

            private void Awake()
            {
                Inputs = FindAnyObjectByType<SimpleController>();
                AIAgent = transform.parent.parent.GetComponent<AIAgent>();
                SquadController = AIAgent.transform.parent.GetComponent<SquadController>();
                Leader = SquadController.LeaderComp;
                #region Events //Can be better
                Leader.OnShooting += HandleSupportFireInput;
                SquadController.OnMoving += HandleSquadMoving;
                #endregion //Events Can be better
            }

            private void OnDestroy()
            {
                #region Events //Can be better
                Leader.OnShooting -= HandleSupportFireInput;
                SquadController.OnMoving -= HandleSquadMoving;

                #endregion //Events Can be better
            }
            public override void EnterState()
            {
                base.EnterState();
                AIAgent.StopMove();
                if (!AIAgent.HasReachedPos())
                {
                    NextState = FOLLOW;
                }
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
            public void HandleLeaderDamaged(GameObject source)
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
                if (!IsActive || !SquadController.IsBarrageMode) //Important
                    return;
                NextState = BARRAGE;
                AIAgent.ShootingTarget = target;
            }
            public override void OnTriggerEnter(Collider other)
            {
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
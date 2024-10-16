
using FSMMono;
using UnityEngine;
using UnityEngine.Windows;

namespace FSM
{

    namespace AI
    {
        using static AIAgentFSM.AIState;
        public class Barrage : BaseState<AIAgentFSM.AIState>
        {
            AIAgentFSM.AIState NextState = BARRAGE;
            AIAgent AIAgent;
            SimpleController Inputs;
            public Barrage() : base(BARRAGE)
            { }
            private void Awake()
            {
                Inputs = FindAnyObjectByType<SimpleController>();
                AIAgent = transform.parent.parent.GetComponent<AIAgent>();
                #region Events //Can be better
                Inputs.OnMouseRightClicked += HandleBarrageFireInput;
                Inputs.OnMouseRightHold += HandleBarrageFireInput;
                #endregion //Events Can be better
            }

            private void OnDestroy()
            {
                #region Events //Can be better
                Inputs.OnMouseRightClicked -= HandleBarrageFireInput;
                Inputs.OnMouseRightHold -= HandleBarrageFireInput;
            }
            #endregion //Event
            public override void EnterState()
            {
                NextState = BARRAGE;
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
                if (!AIAgent.IsRecharging)
                    AIAgent.ShootToPosition(AIAgent.ShootingTarget);
            }
            public void HandleBarrageFireInput(Vector3 Target)
            {
                NextState = IDLE;
            }
        }
    }
}
using FSMMono;
using UnityEngine;

namespace FSM
{

    namespace AI
    {
        using static EnemyFSM.EnemyState;
        public class EnemyIdle : BaseState<EnemyFSM.EnemyState>
        {
            EnemyFSM.EnemyState NextState = IDLE;
            TurretAgent TurretAgent;

            private float mPatrolTimer;

            [SerializeField]
            private float mPatrolDelay;

            public EnemyIdle() : base(IDLE)
            { }
            private void Awake()
            {
                TurretAgent = transform.parent.parent.GetComponent<TurretAgent>();
                mPatrolTimer = mPatrolDelay;
            }
            public override void EnterState()
            {
                NextState = IDLE;
                TurretAgent.StopMove();
            }

            public override void ExitState()
            {
                mPatrolTimer = mPatrolDelay;
            }

            public override EnemyFSM.EnemyState GetNextSate()
            {
                return NextState;
            }

            public override void OnTriggerEnter(Collider other)
            {
                if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
                {
                    NextState = CHASE;
                }
            }

            public override void OnTriggerExit(Collider other)
            {
            }

            public override void OnTriggerStay(Collider other)
            {
            }

            public override void UpdateState()
            {
                mPatrolTimer -= Time.deltaTime;

                if (mPatrolTimer < 0)
                    NextState = PATROL;
            }
        }
    }
}
using FSMMono;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace FSM
{

    namespace AI
    {
        using static EnemyFSM.EnemyState;
        public class EnemyPatrol : BaseState<EnemyFSM.EnemyState>
        {
            EnemyFSM.EnemyState NextState = PATROL;
            TurretAgent TurretAgent;

            [SerializeField]
            private List<Transform> mPatrolPoint = new List<Transform>();

            private int mIndex = 0;

            public EnemyPatrol() : base(PATROL)
            { }
            private void Awake()
            {
                TurretAgent = transform.parent.parent.GetComponent<TurretAgent>();
                TurretAgent.MoveTo(mPatrolPoint[mIndex % 3].position);
            }
            public override void EnterState()
            {
                NextState = PATROL;
                TurretAgent.MoveTo(mPatrolPoint[mIndex % 3].position);
            }

            public override void ExitState()
            {
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
                if (TurretAgent.HasReachedPos())
                {
                    mIndex++;
                    TurretAgent.MoveTo(mPatrolPoint[mIndex % 3].position);
                }
            }
        }
    }
}

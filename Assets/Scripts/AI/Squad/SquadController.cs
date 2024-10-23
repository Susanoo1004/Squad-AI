using FSM;
using FSMMono;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Squad
{
    public class SquadController : MonoBehaviour
    {
        [SerializeField]
        private GameObject AIAgentPrefab = null;
        [SerializeField]
        private uint numberOfAIAgents = 3;
        public uint NumberOfUnits { get { return numberOfAIAgents; } }
        [SerializeField]
        private Transform PlayerStart = null;
        [SerializeField]
        private ISquadFormation Formation =
            null;
        private List<AIAgent> Agents = new();
        private Vector3 Target;
        [SerializeField]
        private Transform SquadLeader;
        private ISquadLeader SquadLeaderComp;

        [SerializeField]
        private float DistanceToMove = 5f;
        [SerializeField]
        private float DistanceFromTarget = 2f;
        Vector3 Barycenter;

        public AIAgent Protector;
        public AIAgent Healer;

        private void Awake()
        {
            if (!SquadLeader)
                SquadLeader = FindAnyObjectByType<PlayerAgent>().transform;
            SquadLeaderComp = SquadLeader.GetComponent<ISquadLeader>();
            Target = SquadLeader.position;
        }
        // Start is called before the first frame update
        void Start()
        {
            SquadLeaderComp.OnMoving += HandleMovingLeader;
            SquadLeaderComp.OnShooting += HandleShootingLeader;
            SquadLeaderComp.OnDamageTaken += HandleDamageTakenLeader;
            SquadLeaderComp.OnCriticalHP += HandleCriticalHPLeader;

            Target = PlayerStart.transform.position;
            for (uint i = 0; i < numberOfAIAgents; i++)
            {
                GameObject unitInst = InstantiateAAIAgent();
                unitInst.transform.position += Formation.GetOffset(i);
            }
            Barycenter = ComputeBarycenter();
        }
        private void OnDestroy()
        {
            SquadLeaderComp.OnMoving -= HandleMovingLeader;
            SquadLeaderComp.OnShooting -= HandleShootingLeader;
            SquadLeaderComp.OnDamageTaken -= HandleDamageTakenLeader;
            SquadLeaderComp.OnCriticalHP -= HandleCriticalHPLeader;
        }

        GameObject InstantiateAAIAgent()
        {
            GameObject unitInst = Instantiate(AIAgentPrefab, PlayerStart, false);
            unitInst.transform.parent = transform;
            AIAgent unit = unitInst.GetComponent<AIAgent>();
            Agents.Add(unitInst.GetComponent<AIAgent>());

            RaycastHit raycastInfo;
            Ray ray = new Ray(unitInst.transform.position, Vector3.down);
            if (Physics.Raycast(ray, out raycastInfo, 10f, 1 << LayerMask.NameToLayer("Floor")))
            {
                unitInst.transform.position = raycastInfo.point;
            }

            return unitInst;
        }
        //Update the squad size as well
        public void SpawnAAIAgent()
        {
            numberOfAIAgents++;
            Formation.UpdateUnitCount();
            InstantiateAAIAgent();
        }
        public void SetTargetPos(Vector3 newTarget)
        {
            Barycenter = ComputeBarycenter();
            Vector3 direction = SquadLeader.forward;
            float angle = Mathf.Atan2(direction.x, direction.z);
            float cosA = Mathf.Cos(angle);
            float sinA = Mathf.Sin(angle);
            Vector3 TargetWithDistance = newTarget - DistanceFromTarget * direction;
            for (uint i = 0; i < Agents.Count; i++)
                Agents[(int)i].SquadTarget = TargetWithDistance + Formation.GetOffset(i, cosA, sinA);
        }


        public void OrderToShoot(Vector3 newTarget)
        {
            foreach (AIAgent agent in Agents)
                agent.ShootToPosition(newTarget);
        }
        public void FollowPath(List<Vector3> path)
        {
            foreach (var agent in Agents)
            {
                agent.FollowPath(path);
            }
        }

        void FixedUpdate()
        {
        }

        void HandleMovingLeader(Vector3 destination)
        {
            if (Vector3.SqrMagnitude(Target - destination) > DistanceToMove * DistanceToMove)
                SetTargetPos(destination);
        }
        void HandleShootingLeader(Vector3 target)
        {
            foreach (AIAgent agent in Agents)
            {
                ChangeState(agent, AIAgentFSM.AIState.SUPPORT);
                agent.ShootingTarget = target;
            }

        }
        void HandleDamageTakenLeader(GameObject instigator)
        {
            if (Protector != null)
                return;
            else
            {
                AIAgent bestProtector = null;
                int maxPriority = 0;
                foreach (AIAgent agent in Agents)
                {
                    if (Vector3.Distance(agent.transform.position, SquadLeader.position) > 5f /*MinDistanceToProtect*/)
                        break;
                    // if(maxPriority < agent. /*agent.ProtectPriority*/)
                    //    {
                    //    maxPriority = agent. /*agent.ProtectPriority*/
                    bestProtector = agent;
                    //    }
                }

                Protector = bestProtector;
                if (Protector != null)
                {
                    Protector.RegisteredEnemy = instigator.transform;
                    ChangeState(Protector, AIAgentFSM.AIState.PROTECT);
                    StartCoroutine(EndingStateCoroutine(Protector.gameObject.GetComponentInChildren<AIAgentFSM>(), AIAgentFSM.AIState.PROTECT, () => Protector = null));
                }
            }
        }
        void HandleCriticalHPLeader(int currentHP)
        {
            //TO ADD When Heal State is completed

            //if (Healer != null)
            //    return;
            //else
            //{
            //    AIAgent bestHealer = null;
            //    int maxPriority = 0;
            //    foreach (AIAgent agent in Agents)
            //    {
            //        if (Vector3.Distance(agent.transform.position, SquadLeader.position) > 5f /*MinDistanceToHeal*/)
            //            break;
            //        // if(maxPriority < agent. /*agent.HealPriority*/)
            //        //    {
            //        //    maxPriority = agent. /*agent.HealPriority*/
            //        bestHealer = agent;
            //        //    }
            //    }

            //    Healer = bestHealer;
            //    if (Healer != null)
            //    {
            //        ChangeState(Healer, AIAgentFSM.AIState.PROTECT);
            //        StartCoroutine(EndingStateCoroutine(Healer.gameObject.GetComponentInChildren<AIAgentFSM>(), AIAgentFSM.AIState.HEAL, () => Healer = null));
            //    }
            //}
        }
        void OrderBarrageFire(Vector3 target)
        {
            foreach (AIAgent agent in Agents)
            {
                ChangeState(agent, AIAgentFSM.AIState.BARRAGE);
                agent.ShootingTarget = target;
            }
        }

        private void ChangeState(AIAgent agent, AIAgentFSM.AIState newState)
        {
            agent.gameObject.GetComponentInChildren<AIAgentFSM>().ChangeState(newState);
        }
        IEnumerator EndingStateCoroutine(AIAgentFSM aiAgentFSM, AIAgentFSM.AIState state, Action func)
        {
            yield return new WaitUntil(() => aiAgentFSM.CurrentState.StateKey != state);
            func.Invoke();
        }
        Vector3 ComputeBarycenter()
        {
            Vector3 result = Vector3.zero;
            foreach (AIAgent agent in Agents)
                result += agent.transform.position;
            result /= Agents.Count;
            return result;
        }
    }
}
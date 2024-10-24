using FSM;
using FSM.AI;
using FSMMono;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


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
        private float MinDistanceToProtect = 7f;
        [SerializeField]
        private float MinDistanceToHeal = 10f;

        [SerializeField]
        private ISquadFormation Formation =
            null;
        private List<AIAgent> Agents = new();
        private Vector3 Target;
        [SerializeField]
        private Transform Leader;
        public ISquadLeader LeaderComp { get; private set; }

        [SerializeField]
        private float DistanceToMove = 5f;
        public event Action OnMoving;

        [SerializeField]
        private float DistanceFromTarget = 2f;
        Vector3 Barycenter;

        public AIAgent Protector;
        public AIAgent Healer;
        private bool IsBarrageMode = false;

        private void Awake()
        {
            if (!Leader)
                Leader = FindAnyObjectByType<PlayerAgent>().transform;
            LeaderComp = Leader.GetComponent<ISquadLeader>();
            Target = Leader.position;
        }
        // Start is called before the first frame update
        void Start()
        {
            transform.position = Leader.position;
            InitLeaderEvents();

            Target = Leader.transform.position;
            for (uint i = 0; i < numberOfAIAgents; i++)
            {
                GameObject unitInst = InstantiateAAIAgent();
                unitInst.transform.position += Formation.GetOffset(i);
            }
            Barycenter = ComputeBarycenter();
        }
        private void OnDestroy()
        {
            UnInitLeaderEvents();
            Leader = null;
            LeaderComp = null;
        }

        GameObject InstantiateAAIAgent()
        {
            if (!AIAgentPrefab)
            {
                print("NO AGENT PREFAB");
                return null;
            }
            GameObject unitInst = Instantiate(AIAgentPrefab, transform, false);
            AIAgent unit = unitInst.GetComponent<AIAgent>();
            Agents.Add(unitInst.GetComponent<AIAgent>());
            unit.OnAIDeath += RemoveAIAgent;

            if (Agents.Count < 2 || Agents.Count % 5 == 0)
                unit.Role = AIAgent.AgentRole.Medic;
            else if (Agents.Count % 3 == 0)
                unit.Role = AIAgent.AgentRole.Tank;
            //Default is Attacker

            RaycastHit raycastInfo;
            Ray ray = new Ray(unitInst.transform.position, Vector3.down);
            if (Physics.Raycast(ray, out raycastInfo, 10f, 1 << LayerMask.NameToLayer("Floor")))
            {
                unitInst.transform.position = raycastInfo.point;
            }

            return unitInst as GameObject;
        }
        public void RemoveAIAgent(AIAgent agent)
        {
            agent.OnAIDeath -= RemoveAIAgent;
            Agents.Remove(agent);
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
            Vector3 direction = Leader.forward;
            Target = newTarget;
            Barycenter = ComputeBarycenter();
            float angle = Mathf.Atan2(direction.x, direction.z);
            float cosA = Mathf.Cos(angle);
            float sinA = Mathf.Sin(angle);
            Vector3 TargetWithDistance = newTarget - DistanceFromTarget * new Vector3(direction.x, 0f, direction.z);
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
            {
                SetTargetPos(destination);
                OnMoving?.Invoke();
            }
        }
        void HandleShootingLeader(Vector3 target)
        {
        }
        void HandleDamageTakenLeader(GameObject instigator)
        {
            if (Protector != null || IsBarrageMode)
                return;
            else
            {
                AIAgent bestProtector = null;
                float maxPriority = 0f;
                foreach (AIAgent agent in Agents)
                {
                    if (Vector3.SqrMagnitude(agent.transform.position - Leader.position) > MinDistanceToProtect * MinDistanceToProtect
                        || agent == Healer)
                        break;
                    float priority = Vector3.Magnitude(agent.transform.position - Leader.position) - MinDistanceToProtect;
                    //ratio
                    priority /= MinDistanceToProtect;
                    if (agent.Role == AIAgent.AgentRole.Tank)
                        priority += 1f;

                    if (maxPriority < priority)
                    {
                        maxPriority = priority;
                        bestProtector = agent;
                    }
                }

                Protector = bestProtector;
                if (!Protector)
                    return;
                //ELSE
                {
                    Protector.RegisteredEnemy = instigator.transform;
                    ChangeState(Protector, AIAgentFSM.AIState.PROTECT);
                    StartCoroutine(EndingStateCoroutine(Protector.gameObject.GetComponentInChildren<AIAgentFSM>(), AIAgentFSM.AIState.PROTECT, () => Protector = null));
                }
            }
        }
        void HandleCriticalHPLeader(int currentHP)
        {

            if (Healer != null)
                return;
            else
            {
                AIAgent bestHealer = null;
                float maxPriority = 0f;

                foreach (AIAgent agent in Agents)
                {
                    if (Vector3.SqrMagnitude(agent.transform.position - Leader.position) > MinDistanceToHeal * MinDistanceToHeal
                        || agent == Protector)
                        break;

                    float priority = Vector3.Magnitude(agent.transform.position - Leader.position) - MinDistanceToHeal;
                    //ratio
                    priority /= MinDistanceToHeal;
                    if (agent.Role == AIAgent.AgentRole.Medic)
                        priority += 1f;

                    if (maxPriority < priority)
                    {
                        maxPriority = priority;
                    }
                    bestHealer = agent;
                }

                Healer = bestHealer;
                if (!Healer || IsBarrageMode)
                    return;
                //ELSE
                {
                    ChangeState(Healer, AIAgentFSM.AIState.HEAL);
                    StartCoroutine(EndingStateCoroutine(Healer.gameObject.GetComponentInChildren<AIAgentFSM>(), AIAgentFSM.AIState.HEAL, () => Healer = null));
                }
            }
        }
        void HandleDeathLeader()
        {
            UnInitLeaderEvents();
            int i = 0;
            while (LeaderComp == null || LeaderComp.CheckDeath() && i < Agents.Count)
            {
                Leader = Agents[i].transform;
                LeaderComp = Leader.GetComponent<ISquadLeader>();
                if (!LeaderComp.CheckDeath())
                    break;
                i++;
            }
            foreach (AIAgent agent in Agents)
            {
                ChangeState(agent, AIAgentFSM.AIState.IDLE);
            }
            InitLeaderEvents();
        }
        public void OrderBarrageFire(Vector3 target)
        {
            foreach (AIAgent agent in Agents)
            {
                agent.ShootingTarget = target;
                ChangeState(agent, AIAgentFSM.AIState.BARRAGE);
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
        void InitLeaderEvents()
        {
            LeaderComp.OnMoving += HandleMovingLeader;
            LeaderComp.OnShooting += HandleShootingLeader;
            LeaderComp.OnDamageTaken += HandleDamageTakenLeader;
            LeaderComp.OnCriticalHP += HandleCriticalHPLeader;
            LeaderComp.OnDeath += HandleDeathLeader;
        }
        void UnInitLeaderEvents()
        {
            LeaderComp.OnMoving -= HandleMovingLeader;
            LeaderComp.OnShooting -= HandleShootingLeader;
            LeaderComp.OnDamageTaken -= HandleDamageTakenLeader;
            LeaderComp.OnCriticalHP -= HandleCriticalHPLeader;
            LeaderComp.OnDeath -= HandleDeathLeader;
        }
    }
}
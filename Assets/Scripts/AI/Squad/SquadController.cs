using FSMMono;
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
        [SerializeField]
        private float DistanceToMove = 5f;
        [SerializeField]
        private float DistanceFromTarget = 2f;
        private void Awake()
        {
            if (!SquadLeader)
                SquadLeader = FindAnyObjectByType<PlayerAgent>().transform;
            Target = SquadLeader.position;

        }
        // Start is called before the first frame update
        void Start()
        {
            Target = PlayerStart.transform.position;
            for (uint i = 0; i < numberOfAIAgents; i++)
            {
                GameObject unitInst = InstantiateAAIAgent();
                unitInst.transform.position += Formation.GetOffset(i);
            }
            transform.position = ComputeBarycenter();
        }

        GameObject InstantiateAAIAgent()
        {
            GameObject unitInst = Instantiate(AIAgentPrefab, PlayerStart, false);
            unitInst.transform.parent = null;
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
            Vector3 direction = SquadLeader.forward;
            float angle = Mathf.Atan2(direction.x, direction.z);
            float cosA = Mathf.Cos(angle);
            float sinA = Mathf.Sin(angle);
           Vector3 TargetWithDistance = newTarget - DistanceFromTarget * direction;
            for (uint i = 0; i < Agents.Count; i++)
                Agents[(int)i].Target = TargetWithDistance + Formation.GetOffset(i, cosA, sinA);
        }

        public void FollowPath(List<Vector3> path)
        {
            foreach (var agent in Agents)
            {
                agent.FollowPath(path);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Vector3.SqrMagnitude(transform.position - SquadLeader.position) > DistanceToMove)
                SetTargetPos(SquadLeader.position);
        }

        Vector3 ComputeBarycenter()
        {
            Vector3 result = new Vector3();
            foreach (AIAgent agent in Agents)
                result += agent.transform.position;
            result /= Agents.Count;
            return result;
        }
    }
}
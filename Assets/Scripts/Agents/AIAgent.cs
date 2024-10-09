using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace FSMMono
{
    public class AIAgent : MonoBehaviour, IDamageable
    {

        [SerializeField]
        int MaxHP = 100;
        [SerializeField]
        float BulletPower = 1000f;
        [SerializeField]
        GameObject BulletPrefab;

        [SerializeField]
        Slider HPSlider = null;


        Transform GunTransform;
        NavMeshAgent NavMeshAgentInst;
        Material MaterialInst;

        bool IsDead = false;
        int CurrentHP;

        private void SetMaterial(Color col)
        {
            MaterialInst.color = col;
        }
        public void SetWhiteMaterial() { SetMaterial(Color.white); }
        public void SetRedMaterial() { SetMaterial(Color.red); }
        public void SetBlueMaterial() { SetMaterial(Color.blue); }
        public void SetYellowMaterial() { SetMaterial(Color.yellow); }

        public Vector3 Target;


        #region MonoBehaviour

        private void Awake()
        {
            CurrentHP = MaxHP;

            NavMeshAgentInst = GetComponent<NavMeshAgent>();

            Renderer rend = transform.Find("Body").GetComponent<Renderer>();
            MaterialInst = rend.material;

            GunTransform = transform.Find("Body/Gun");
            if (GunTransform == null)
                Debug.Log("could not find gun transform");

            if (HPSlider != null)
            {
                HPSlider.maxValue = MaxHP;
                HPSlider.value = CurrentHP;
            }


            //NavMeshAgentInst.updatePosition = false;
        }

        private void Update()
        {
            HPSlider.transform.parent.rotation = Quaternion.identity;
        }
        private void Start()
        {
        }
        private void OnTriggerEnter(Collider other)
        {
        }
        private void OnTriggerExit(Collider other)
        {
        }
        private void OnDrawGizmos()
        {
        }

        #endregion

        #region Perception methods

        #endregion

        #region MoveMethods
        public void StopMove()
        {
            NavMeshAgentInst.isStopped = true;
        }
        public void MoveTo(Vector3 dest)
        {
            NavMeshAgentInst.isStopped = false;
            NavMeshAgentInst.SetDestination(dest);
        }
        public bool HasReachedPos()
        {
            return NavMeshAgentInst.remainingDistance - NavMeshAgentInst.stoppingDistance <= 0f;
        }
        public void FollowPath(List<Vector3> path)
        {
            StartCoroutine(FollowPathCoroutine(path));
        }

        public IEnumerator FollowPathCoroutine(List<Vector3> path)
        {
            int pathId = 0;

            // Loop through the path waypoints
            while (path.Count > pathId + 1)
            {
                // If the current waypoint is null, stop the coroutine
                if (path[pathId] == null)
                    yield break;

                // Move towards the next waypoint in the path
                while ((path[pathId] - transform.position).sqrMagnitude > 10f /* Waypoint Tolerance */)
                {
                    // Move the object towards the target position (you can adjust speed as needed)
                    Target = path[pathId];

                    // Wait for the next frame before continuing the loop
                    yield return null;
                }

                // Set the target position to the next waypoint
                Target = path[pathId];

                // Move to the next waypoint in the path
                pathId++;
            }
    }

        #endregion

        #region ActionMethods

        public void AddDamage(int amount)
        {
            CurrentHP -= amount;
            if (CurrentHP <= 0)
            {
                IsDead = true;
                CurrentHP = 0;
            }

            if (HPSlider != null)
            {
                HPSlider.value = CurrentHP;
            }
        }
        void ShootToPosition(Vector3 pos)
        {
            // look at target position
            transform.LookAt(pos + Vector3.up * transform.position.y);

            // instantiate bullet
            if (BulletPrefab)
            {
                GameObject bullet = Instantiate<GameObject>(BulletPrefab, GunTransform.position + transform.forward * 0.5f, Quaternion.identity);
                bullet.layer = gameObject.layer;
                Rigidbody rb = bullet.GetComponent<Rigidbody>();
                rb.AddForce(transform.forward * BulletPower);
            }
        }

        Vector3 velocity = Vector3.zero;

        public void FixedUpdate()
        {
            // ugly hard coded position next to the player
            NavMeshAgentInst.SetDestination(Target);
        }
        #endregion
    }
}
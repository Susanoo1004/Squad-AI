using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace FSMMono
{
    public class AIAgent : MonoBehaviour, ISquadLeader
    {

        [SerializeField]
        int MaxHP = 100;
        [SerializeField]
        int CriticalHP = 20;
        [SerializeField]
        float BulletPower = 1000f;
        [SerializeField]
        GameObject BulletPrefab;

        [SerializeField]
        Slider HPSlider = null;

        public enum AgentRole
        {
            Attacker,
            Medic,
            Tank
        }
        public AgentRole Role = AgentRole.Attacker;

        Transform GunTransform;
        NavMeshAgent NavMeshAgentInst;
        Material MaterialInst;

        public bool IsRecharging { get; private set; } = false;
        [SerializeField]
        float RechargeDuration = 1f;
        bool IsDead = false; //Debug purposes
        int CurrentHP;

        Color DefaultColor;
        public void SetMaterial(Color col)
        {
            MaterialInst.color = col;
        }
        public Color GetMaterialColor()
        {
            return MaterialInst.color;
        }
        public void SetDefaultMaterial() { SetMaterial(DefaultColor); }
        public void SetWhiteMaterial() { SetMaterial(Color.white); }
        public void SetRedMaterial() { SetMaterial(Color.red); }
        public void SetBlueMaterial() { SetMaterial(Color.blue); }
        public void SetYellowMaterial() { SetMaterial(Color.yellow); }

        public Vector3 MoveTarget;  //Position to move outside of the squad
        public Vector3 SquadTarget; //Position to move with squad offset
        public Vector3 ShootingTarget;

        public Transform RegisteredEnemy;
        [SerializeField]
        private float RangeOfSight = 10f;

        #region MonoBehaviour

        private void Awake()
        {
            CurrentHP = MaxHP;

            NavMeshAgentInst = GetComponent<NavMeshAgent>();

            Renderer rend = transform.Find("Body").GetComponent<Renderer>();
            MaterialInst = rend.material;
            DefaultColor = GetMaterialColor();

            GunTransform = transform.Find("Body/Gun");
            if (GunTransform == null)
                Debug.Log("could not find gun transform");

            if (HPSlider != null)
            {
                HPSlider.maxValue = MaxHP;
                HPSlider.value = CurrentHP;
            }
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
            //Move Target
            Gizmos.color = Color.grey;
            Gizmos.DrawSphere(MoveTarget, 0.5f);
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(SquadTarget, 0.5f);

        }

        #endregion

        #region Perception methods
        public bool IsEnemyInSight()
        {
            //Test the line of sight
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            // Perform the raycast and check if it hits an enemy
            return Physics.Raycast(ray, out hit, RangeOfSight, LayerMask.NameToLayer("Enemies") + LayerMask.NameToLayer("Allies") - gameObject.layer);
        }
        public bool IsEnemyAimable(Vector3 enemyPosition)
        {
            //Test the line of sight
            Ray ray = new Ray(transform.position, (enemyPosition - transform.position).normalized);
            RaycastHit hit;

            // Perform the raycast and check if it hits an enemy
            return Physics.Raycast(ray, out hit, RangeOfSight, LayerMask.NameToLayer("Enemies") + LayerMask.NameToLayer("Allies") - gameObject.layer);
        }

        #endregion

        #region MoveMethods
        public void StopMove()
        {
            NavMeshAgentInst.isStopped = true;
        }
        public void MoveTo(Vector3 dest)
        {
            MoveTarget = dest;
            OnMoving?.Invoke(dest);
            NavMeshAgentInst.isStopped = false;
            NavMeshAgentInst.SetDestination(dest);
        }
        public void MoveToTarget()
        {
            MoveTo(MoveTarget);
        }
        public void MoveToSquadTarget()
        {
            MoveTo(SquadTarget);
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
                    MoveTarget = path[pathId];

                    // Wait for the next frame before continuing the loop
                    yield return null;
                }

                // Set the target position to the next waypoint
                MoveTarget = path[pathId];

                // Move to the next waypoint in the path
                pathId++;
            }
        }

        #endregion

        #region ActionMethods
        public event Action<AIAgent> OnAIDeath;
        public event Action OnDeath;
        public event Action<GameObject> OnDamageTaken;
        public event Action<int> OnCriticalHP;
        public event Action<Vector3> OnMoving;
        public event Action<Vector3> OnShooting;
        public bool CheckDeath() { return IsDead; }


        public void AddDamage(int amount, GameObject source)
        {
            CurrentHP -= amount;
            if (CurrentHP <= 0)
            {
                OnAIDeath?.Invoke(this);
                OnDeath?.Invoke();
                IsDead = true;
                CurrentHP = 0;
            }
            else
            {
                OnDamageTaken?.Invoke(source);
                if (CurrentHP < CriticalHP)
                {
                    OnCriticalHP?.Invoke(CurrentHP);
                }
            }
            if (HPSlider != null)
            {
                HPSlider.value = CurrentHP;
            }
        }
        public void ShootToPosition(Vector3 pos)
        {

            // look at target position

            // instantiate bullet
            if (BulletPrefab && !IsRecharging)
            {
                pos.y = 0f;
                OnShooting?.Invoke(pos);
                transform.LookAt(pos + Vector3.up * transform.position.y);
                StartCoroutine(RechargeCoroutine());
                GameObject bullet = Instantiate(BulletPrefab, GunTransform.position + GunTransform.forward * 0.5f, Quaternion.identity);
                (bullet.GetComponent<Bullet>()).SetShooter(gameObject);
                bullet.layer = gameObject.layer;
                Rigidbody bullet_rb = bullet.GetComponent<Rigidbody>();
                bullet_rb.AddForce(transform.forward * BulletPower);
            }
        }
        IEnumerator RechargeCoroutine()
        {
            IsRecharging = true;
            yield return new WaitForSeconds(RechargeDuration);
            IsRecharging = false;
        }
        IEnumerator TurnAndShootCoroutine(Vector3 pos)
        {
            float duration = 1f;
            float t = 0f;
            Quaternion angle = transform.rotation;
            Quaternion targetAngle = Quaternion.LookRotation(pos + Vector3.up * transform.position.y, Vector3.up);
            // look at target position
            while (t < duration && !Quaternion.Equals(angle, targetAngle))
            {
                yield return new WaitForEndOfFrame();
                t += Time.deltaTime;
                angle = Quaternion.Slerp(transform.rotation, targetAngle, t);
                transform.rotation = angle;
            }
            // instantiate bullet
            if (BulletPrefab)
            {
                GameObject bullet = Instantiate<GameObject>(BulletPrefab, GunTransform.position + transform.forward * 0.5f, Quaternion.identity);
                bullet.layer = gameObject.layer;
                Rigidbody rb = bullet.GetComponent<Rigidbody>();
                rb.AddForce(transform.forward * BulletPower);
            }

        }

        public void ShootRegisteredEnemy()
        {
            if (RegisteredEnemy)
                ShootToPosition(RegisteredEnemy.position);
        }

        Vector3 velocity = Vector3.zero;
        #endregion
    }
}
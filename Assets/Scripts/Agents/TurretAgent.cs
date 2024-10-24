using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class TurretAgent : MonoBehaviour, IDamageable
{
    [SerializeField]
    int MaxHP = 100;
    [SerializeField]
    float BulletPower = 1000f;
    [SerializeField]
    GameObject BulletPrefab;

    [SerializeField]
    float ShootFrequency = 1f;

    float NextShootDate = 0f;

    Transform GunTransform;

    bool IsDead = false; //Debug purposes
    [SerializeField]
    int CurrentHP;
    [SerializeField]
    Slider HPSlider = null;

    GameObject Target = null;

    NavMeshAgent NavMeshAgentInst;

    [SerializeField] private GameObject DeathExplosion;

    public float RangeOfSight { get; private set; } = 15f;

    public void AddDamage(int amount, GameObject source)
    {
        if (IsDead)
            return;

        CurrentHP -= amount;

        if (CurrentHP <= 0)
        {
            IsDead = true;
            CurrentHP = 0;

            GameObject finalExplo = Instantiate(DeathExplosion);
            finalExplo.transform.position = transform.position;
            Destroy(finalExplo, 0.5f);

            gameObject.SetActive(false);
        }

        if (HPSlider != null)
        {
            HPSlider.value = CurrentHP;
        }
    }
    void ShootToPosition(Vector3 pos)
    {
        // look at target position
        transform.LookAt(pos + Vector3.up * (transform.position.y - pos.y));

        // instantiate bullet
        if (BulletPrefab)
        {
            GameObject bullet = Instantiate<GameObject>(BulletPrefab, GunTransform.position + transform.forward * 0.5f, Quaternion.identity);
            (bullet.GetComponent<Bullet>()).SetShooter(gameObject);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * BulletPower);
        }
    }

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
    public void MoveToTarget()
    {
        MoveTo(Target.transform.position);
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
                Target.transform.position = path[pathId];

                // Wait for the next frame before continuing the loop
                yield return null;
            }

            // Set the target position to the next waypoint
            Target.transform.position = path[pathId];

            // Move to the next waypoint in the path
            pathId++;
        }
    }

    #endregion

    void Start()
    {

        NavMeshAgentInst = GetComponent<NavMeshAgent>();

        CurrentHP = MaxHP;
        if (HPSlider != null)
        {
            HPSlider.maxValue = MaxHP;
            HPSlider.value = CurrentHP;
        }
        GunTransform = transform.Find("Body/Gun");
        if (GunTransform == null)
            Debug.Log("could not find gun transform");

        CurrentHP = MaxHP;
    }

    void Update()
    {
        if (Target && Time.time >= NextShootDate)
        {
            NextShootDate = Time.time + ShootFrequency;
            ShootToPosition(Target.transform.position);
        }
        HPSlider.transform.parent.rotation = Quaternion.identity;
    }
    public bool IsEnemyAimable(Vector3 enemyPosition)
    {
        //Test the line of sight
        Ray ray = new Ray(transform.position, (enemyPosition - transform.position).normalized);
        RaycastHit hit;
        Debug.DrawRay(transform.position, (enemyPosition - transform.position).normalized * RangeOfSight,Color.red);
        int layerMask = LayerMask.GetMask("Enemies", "Allies");

        // Exclude the current gameObject's layer
        int gameObjectLayerMask = 1 << gameObject.layer;
        layerMask &= ~gameObjectLayerMask;  // Exclude the current object's layer
        // Perform the raycast and check if it hits an enemy
        return Physics.Raycast(ray, out hit, RangeOfSight, layerMask);
    }
    private void OnTriggerEnter(Collider other)
    {
        LayerMask TargetLayer = LayerMask.NameToLayer("Enemies") + LayerMask.NameToLayer("Allies") - gameObject.layer;
        if (Target == null && other.gameObject.layer == TargetLayer)
        {
            if (IsEnemyAimable(other.transform.position))
                Target = other.gameObject;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == Target)
        {
            Target = null;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (Target)
            return;
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemies") + LayerMask.NameToLayer("Allies") - gameObject.layer)
        {
            if (IsEnemyAimable(other.transform.position))
                Target = other.gameObject;
        }
    }
}

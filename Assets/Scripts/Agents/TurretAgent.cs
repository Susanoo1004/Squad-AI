using System;
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

    bool IsDead = false;
    [SerializeField]
    int CurrentHP;
    [SerializeField]
    Slider HPSlider = null;

    GameObject Target = null;

    public void AddDamage(int amount, GameObject source)
    {
        CurrentHP -= amount;
        if (CurrentHP <= 0)
        {
            IsDead = true;
            CurrentHP = 0;
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
        transform.LookAt(pos + Vector3.up * transform.position.y);

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

    private void OnTriggerEnter(Collider other)
    {
        if (Target == null && other.gameObject.layer == LayerMask.NameToLayer("Allies"))
        {
            Target = other.gameObject;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (Target != null && other.gameObject == Target)
        {
            Target = null;
        }
    }
}

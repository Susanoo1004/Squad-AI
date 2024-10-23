using System;
using System.Collections;
using TreeEditor;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.ShaderData;


public class PlayerAgent : MonoBehaviour, ISquadLeader
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
    GameObject TargetCursorPrefab = null;
    [SerializeField]
    GameObject NPCTargetCursorPrefab = null;

    [SerializeField]
    Slider HPSlider = null;

    Rigidbody rb;
    GameObject TargetCursor = null;
    GameObject NPCTargetCursor = null;
    Transform GunTransform;
    bool IsDead = false;
    [SerializeField]
    float FiringRate = 0.2f;

    bool IsBetweenFireRate = false;
    int CurrentHP;
    
    //Actions as set in the interface
    public event Action<GameObject> OnDamageTaken;
    public event Action<int> OnCriticalHP;
    public event Action<Vector3> OnMoving;
    public event Action<Vector3> OnShooting;

    #region Actions

    #endregion //Actions

    private GameObject GetTargetCursor()
    {
        if (TargetCursor == null)
            TargetCursor = Instantiate(TargetCursorPrefab);
        return TargetCursor;
    }
    private GameObject GetNPCTargetCursor()
    {
        if (NPCTargetCursor == null)
        {
            NPCTargetCursor = Instantiate(NPCTargetCursorPrefab);
        }
        return NPCTargetCursor;
    }
    public void AimAtPosition(Vector3 pos)
    {
        GetTargetCursor().transform.position = pos;
        if (Vector3.Distance(transform.position, pos) > 1.5f)
        {
            transform.LookAt(pos + Vector3.up * transform.position.y);

            GunTransform.LookAt(pos + Vector3.up * transform.position.y);
            GunTransform.Rotate(new Vector3(90, 0, 0));
        }

    }
    public void ShootToPosition(Vector3 pos)
    {
        // instantiate bullet
        if (BulletPrefab && !IsBetweenFireRate)
        {
            OnShooting?.Invoke(pos);

            Vector3 bulletForward = (GetTargetCursor().transform.position - GunTransform.position).normalized;
            bulletForward.y = 0f;
            StartCoroutine(FireRateCoroutine(FiringRate));
            GameObject bullet = Instantiate<GameObject>(BulletPrefab, GunTransform.position + bulletForward * 0.5f, Quaternion.identity);
            (bullet.GetComponent<Bullet>()).SetShooter(gameObject);
            bullet.layer = gameObject.layer;
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            rb.AddForce(bulletForward * BulletPower);

            /*
            rb.excludeLayers += bullet.layer;
            bullet.GetComponent<SphereCollider>().excludeLayers += bullet.layer;
            */
        }
    }
    private IEnumerator FireRateCoroutine(float duration)
    {
        IsBetweenFireRate = true;
        yield return new WaitForSeconds(duration);
        IsBetweenFireRate = false;
    }
    public void NPCShootToPosition(Vector3 pos)
    {
        GetNPCTargetCursor().SetActive(true);
        GetNPCTargetCursor().transform.position = pos;
    }
    public void RemoveNPCTarget()
    {
        GetNPCTargetCursor().SetActive(false);
    }
    public void AddDamage(int amount, GameObject source)
    {
        CurrentHP -= amount;
        if (CurrentHP <= 0)
        {
            IsDead = true;
            CurrentHP = 0;
        }
        else if (CurrentHP < CriticalHP)
        {
            OnCriticalHP?.Invoke(CurrentHP);
        }
        if (HPSlider != null)
        {
            OnDamageTaken?.Invoke(source);
            HPSlider.value = CurrentHP;
        }
    }
    public void MoveToward(Vector3 velocity)
    {
        Vector3 arrival = rb.position + velocity * Time.deltaTime;
        OnMoving?.Invoke(arrival);
        rb.MovePosition(arrival);
    }

    #region MonoBehaviour Methods
    void Start()
    {
        CurrentHP = MaxHP;
        GunTransform = transform.Find("Gun");
        rb = GetComponent<Rigidbody>();



        if (HPSlider != null)
        {
            HPSlider.maxValue = MaxHP;
            HPSlider.value = CurrentHP;
        }
    }
    void Update()
    {

    }

    #endregion


}

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


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
    public event Action OnDeath;
    public bool CheckDeath() { return IsDead; }

    [SerializeField] private GameObject DeathExplosion;

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
            Rigidbody bullet_rb = bullet.GetComponent<Rigidbody>();
            bullet_rb.AddForce(bulletForward * BulletPower);
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
            OnDeath?.Invoke();
            CurrentHP = 0;

            GameObject finalExplo = Instantiate(DeathExplosion);
            finalExplo.transform.position = transform.position;
            Destroy(finalExplo, 0.5f);

            gameObject.SetActive(false);
            return;
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
    public bool Heal(int amount)
    {
        if (CurrentHP >= MaxHP)
            return false;

        CurrentHP += amount;
        HPSlider.value = CurrentHP;
        return true;
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

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 5);
    }

}

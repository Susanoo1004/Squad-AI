using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.ShaderData;

public class PlayerAgent : MonoBehaviour, IDamageable
{
    [SerializeField]
    int MaxHP = 100;
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
        if (Vector3.Distance(transform.position, pos) > 2.5f)
            transform.LookAt(pos + Vector3.up * transform.position.y);
    }
    public void ShootToPosition(Vector3 pos)
    {
        // instantiate bullet
        if (BulletPrefab && !IsBetweenFireRate)
        {
            StartCoroutine(FireRateCoroutine(FiringRate));
            GameObject bullet = Instantiate<GameObject>(BulletPrefab, GunTransform.position + transform.forward * 0.5f, Quaternion.identity);
            bullet.layer = gameObject.layer;
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * BulletPower);
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
        GetNPCTargetCursor().transform.position = pos;
    }
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
    public void MoveToward(Vector3 velocity)
    {
        rb.MovePosition(rb.position + velocity * Time.deltaTime);
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

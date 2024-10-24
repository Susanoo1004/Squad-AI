using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public GameObject Instigator { get;  private set; }

    [SerializeField] private GameObject DamageExplosion;
    public void SetShooter(GameObject instigator)
    {
        Instigator = instigator;
        gameObject.layer = instigator.layer;
    }
    public float Duration = 2f;
    void Start()
    {
        Destroy(gameObject, Duration);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
            return;

        if (other.gameObject.layer == gameObject.layer)
            return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Debug.Log("wall");
            Destroy(gameObject);
            return;
        }

        IDamageable damagedAgent = other.gameObject.GetComponentInParent<IDamageable>();
        if (damagedAgent == null)
            damagedAgent = other.gameObject.GetComponentInParent<IDamageable>();
        damagedAgent?.AddDamage(1,Instigator);
        GameObject finalExplo = Instantiate(DamageExplosion);
        finalExplo.transform.position = transform.position;
        Destroy(finalExplo, 0.5f);

        Destroy(gameObject);
    }
}

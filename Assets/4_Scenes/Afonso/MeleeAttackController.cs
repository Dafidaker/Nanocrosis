using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class MeleeAttackController : MonoBehaviour
{
    [SerializeField] private Transform InitialPos;
    [SerializeField] private Transform TargetPos;
    [SerializeField] private GameObject AmmoPickup;
    [SerializeField] private GameObject EnhancementPickup;
    [SerializeField] private int Damage;

    private void OnEnable()
    {
        transform.position = InitialPos.position;
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, TargetPos.position, 25 * Time.deltaTime);
        if(transform.position == TargetPos.position)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        if(col.GetComponent<RespawningTargetController>() != null)
        {
            RespawningTargetController t = col.GetComponent<RespawningTargetController>();
            if (t.ShieldActive) Debug.Log("Melee cannot damage shield");
            else t.CurrentHealthPoints -= Damage;
            if(t.CurrentHealthPoints <= 0)
            {
                Instantiate(AmmoPickup, t.AmmoPickupSpawnpoint.position, Quaternion.identity);
                //if (t.HasShield) Instantiate(EnhancementPickup, t.EnhancementPickupSpawnpoint.position, Quaternion.identity);
            }
        }
        
        var HitableScript = col.GetComponent<Hittable>();
        if (HitableScript == null)
        {
            HitableScript = col.GetComponentInParent<Hittable>();
        }
        
        if (HitableScript != null)
        {
            HitableScript.GotHit(Damage ,PlayerAttacks.Knife);
        }
        
        
        /*if (col.CompareTag("BossPart"))
        {
            col.GetComponentInParent<Hitable>().GotHit(Damage ,PlayerAttacks.Knife);
        }*/
    }

    private void OnDisable()
    {
        transform.position = InitialPos.position;
    }
}

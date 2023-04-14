using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionController : MonoBehaviour
{
    [SerializeField] private int BlastDamage;
    [SerializeField] private GameObject EnhancementPickup;
    [SerializeField] private LayerMask ItHits;

    private void OnTriggerEnter(Collider other)
    {
        if ((ItHits.value & (1 << other.transform.gameObject.layer)) > 0)
        {

        }
        else
        {
            return;
        }

        if (other.CompareTag("Player")) return;
        if (other.GetComponent<RespawningTargetController>() != null)
        {
            RespawningTargetController d = other.GetComponent<RespawningTargetController>();
            if (!d.ShieldActive) d.CurrentHealthPoints -= BlastDamage;
            else Debug.Log("Blast can't damage shield");

            //if (d.CurrentHealthPoints <= 0 && d.HasShield) Instantiate(EnhancementPickup, d.EnhancementPickupSpawnpoint.position, Quaternion.identity);
        }

        var HitableScript = other.GetComponent<Hitable>();
        if (HitableScript != null)
        {
            HitableScript.GotHit(BlastDamage);
        }

        if (other.CompareTag("BossPart"))
        {
            other.GetComponentInParent<Hitable>().GotHit(BlastDamage);
        }
    }
}

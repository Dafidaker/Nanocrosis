using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionController : MonoBehaviour
{
    [SerializeField] private int BlastDamage;
    [SerializeField] private GameObject EnhancementPickup;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) return;
        if (other.GetComponent<TargetController>() != null)
        {
            TargetController d = other.GetComponent<TargetController>();
            if (!d.ShieldActive) d.CurrentHealthPoints -= BlastDamage;
            else Debug.Log("Blast can't damage shield");

            if (d.CurrentHealthPoints <= 0 && d.HasShield) Instantiate(EnhancementPickup, d.EnhancementPickupSpawnpoint.position, Quaternion.identity);
        }
    }
}

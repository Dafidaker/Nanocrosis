using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEnemiesCollider : MonoBehaviour
{
    [field: SerializeField] private BulletController bulletController;
    
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("bullet") || other.CompareTag("Player")) return;
        if(other.GetComponent<RespawningTargetController>() != null)
        {
            RespawningTargetController d = other.GetComponent<RespawningTargetController>();
            if (d.ShieldActive && bulletController.Enhanced)
            {
                d.CurrentShieldHealthPoints -= bulletController.Damage;
            }
            else if (d.ShieldActive && !bulletController.Enhanced)
            {
                Debug.Log("HAS SHIELD AND AMMO IS NOT ENHANCED");
            }
            else if ((!d.ShieldActive && bulletController.Enhanced) || (!d.ShieldActive && !bulletController.Enhanced))
            {
                d.CurrentHealthPoints -= bulletController.Damage;
            }
        }

        var HitableScript = other.GetComponent<Hitable>();
        if (HitableScript != null)
        {
            //HitableScript.GotHit(bulletController.Damage);
        }

        Destroy(gameObject);
    }
}

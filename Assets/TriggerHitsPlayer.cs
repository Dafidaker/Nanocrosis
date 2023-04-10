using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerHitsPlayer : MonoBehaviour
{
    private int _damage;
    private LayerMask _itHits;

    public void Initiate(int dam , LayerMask layerMask)
    {
        _damage = dam;
        _itHits = layerMask;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerStats>().DamageTaken(_damage);
            Destroy(transform.parent.gameObject);
        }
        
        if ((_itHits.value & (1 << other.transform.gameObject.layer)) > 0) {
            Destroy(transform.parent.gameObject);
        }
    }
}

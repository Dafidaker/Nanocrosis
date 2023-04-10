using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AcidShotController : MonoBehaviour
{
    public float speed;
    public int damage;
    public LayerMask itHits;
    public TriggerHitsPlayer triggerHitsPlayer;
    private Transform _target; 
    private Rigidbody _rb; 
        
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _target = GameManager.Instance.player.transform;
        _rb.velocity = transform.forward * speed;
        triggerHitsPlayer.Initiate(damage, itHits);
    }

    private void Update()
    {
        Debug.DrawRay(transform.position, transform.forward * 100f);
    }

    
}

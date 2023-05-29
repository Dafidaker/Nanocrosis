using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnController : MonoBehaviour
{
    [SerializeField] private Transform respawnPosition;
    [SerializeField] private int damage;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        other.transform.position = respawnPosition.position;
        other.GetComponent<PlayerStats>().DamageTaken(damage);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthController : MonoBehaviour
{
    [field: SerializeField] private int maxHealth;

    private int _currentHealth;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("bullet"))
        {
            _currentHealth -= other.gameObject.GetComponent<BulletController>().Damage;
            if (_currentHealth <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}

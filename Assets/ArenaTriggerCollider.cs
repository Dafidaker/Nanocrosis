    using System;
using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class ArenaTriggerCollider : MonoBehaviour
{
    [field: SerializeField]private Arena arena;
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance.currentArena.arenaType == arena) { return; }
            GameManager.Instance.ChangeArena(arena);
            
        }
    }
}

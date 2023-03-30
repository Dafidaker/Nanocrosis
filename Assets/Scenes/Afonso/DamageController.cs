using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageController : MonoBehaviour
{
    [SerializeField] private float TimeToRespawn;
    [SerializeField] private int MaxHealthPoints;
    public bool IsDead;
    public int CurrentHealthPoints;
    public float CurrentTimeToRespawn;

    private MeshRenderer _mr;
    private BoxCollider _bc;

    private void Start()
    {
        CurrentHealthPoints = MaxHealthPoints;
        CurrentTimeToRespawn = TimeToRespawn;
        _mr = GetComponent<MeshRenderer>();
        _bc = GetComponent<BoxCollider>();
    }
    private void Update()
    {
        if(CurrentHealthPoints <= 0)
        {
            IsDead = true;
            _bc.enabled = false;
            _mr.enabled = false;
            CurrentTimeToRespawn -= Time.deltaTime;
        }

        if (CurrentTimeToRespawn <= 0)
        {
            IsDead = false;
            _mr.enabled = true;
            _bc.enabled = true;
            CurrentHealthPoints = MaxHealthPoints;
            CurrentTimeToRespawn = TimeToRespawn;
        }
    }
}

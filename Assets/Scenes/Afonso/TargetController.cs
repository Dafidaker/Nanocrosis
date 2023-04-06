using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetController : MonoBehaviour
{
    [SerializeField] private float TimeToRespawn;
    [SerializeField] private int MaxHealthPoints;
    [SerializeField] private int MaxShieldHealthPoints;
    [SerializeField] private Material NormalMaterial;
    [SerializeField] private Material ShieldedMaterial;
    [SerializeField] private Material DamageMaterial;

    public Transform AmmoPickupSpawnpoint;
    public Transform EnhancementPickupSpawnpoint;
    public bool HasShield;
    public bool IsDead;
    public bool ShieldActive;
    public int CurrentHealthPoints;
    public int CurrentShieldHealthPoints;
    public float CurrentTimeToRespawn;

    private MeshRenderer _mr;
    private BoxCollider _bc;
    private bool _spawnedPickup;

    private void Start()
    {
        CurrentHealthPoints = MaxHealthPoints;
        CurrentTimeToRespawn = TimeToRespawn;

        _mr = GetComponent<MeshRenderer>();
        _bc = GetComponent<BoxCollider>();

        if (HasShield)
        {
            ShieldActive = true;
            _mr.material = ShieldedMaterial;
            CurrentShieldHealthPoints = MaxShieldHealthPoints;
        }
        else
        {
            ShieldActive = false;
            _mr.material = NormalMaterial;
        }
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
            if (HasShield)
            {
                ShieldActive = true;
                CurrentShieldHealthPoints = MaxShieldHealthPoints;
                _mr.material = ShieldedMaterial;
            }
        }

        if(CurrentShieldHealthPoints <= 0)
        {
            ShieldActive = false;
            _mr.material = NormalMaterial;
        }
    }
}

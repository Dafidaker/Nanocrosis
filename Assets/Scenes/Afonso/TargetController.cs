using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetController : MonoBehaviour
{
    [SerializeField] private int MaxHealthPoints;
    [SerializeField] private int MaxShieldHealthPoints;
    [SerializeField] private float TimeToRespawn;
    [SerializeField] private Material NormalMaterial;
    [SerializeField] private Material ShieldedMaterial;

    public bool HasShield;
    public int CurrentHealthPoints;
    public Transform AmmoPickupSpawnpoint;
    public Transform EnhancementPickupSpawnpoint;
    public bool ShieldActive;
}

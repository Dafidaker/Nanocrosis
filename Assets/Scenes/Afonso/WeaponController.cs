using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Weapon Variables"), Space(10)]
    public string Name;
    public int MagSize;
    public int AmmoReserve;
    public float ReloadTime;
    public float FireRate;
    public bool FullAuto;
    public bool SpreadShot;

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Weapon Variables"), Space(10)]
    public string Name;
    public int MagSize;
    public int CurrentMag;
    public int AmmoReserve;
    public int CurrentAmmoReserve;
    public float ReloadTime;
    public float FireRate;
    public bool FullAuto;
    public bool SpreadShot;
    public int BulletsPerShot;
    public float SpreadAngle;
    public bool MagEmpty;
    public bool OutOfAmmo;
    public bool Reloading;
    public GameObject BulletPrefab;
    public Transform FirePoint;
    public List<Quaternion> Pellets;

    [Header("Colours"), Space(10)]
    public GameObject[] ColoredBits;
    public Material NormalMaterial;
    public Material ReloadMaterial;
    public Material EmptyMagMaterial;

    private void Start()
    {
        CurrentMag = MagSize;
        CurrentAmmoReserve = AmmoReserve;

        if (SpreadShot)
        {
            Pellets = new List<Quaternion>(BulletsPerShot);
            for (int i = 0; i < BulletsPerShot; i++)
            {
                Pellets.Add(Quaternion.Euler(Vector3.zero));
            }
        }
    }

    private void Update()
    {
        if(CurrentMag <= 0) MagEmpty = true;
        else MagEmpty = false;
        if (CurrentAmmoReserve <= 0) OutOfAmmo = true;
        else OutOfAmmo = false;
    }
}

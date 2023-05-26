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
    public bool IsEnhanced;
    public GameObject BulletPrefab;
    public GameObject EnhancedBulletPrefab;
    public GameObject CurrentBulletPrefab;
    public Transform FirePoint;
    public List<Quaternion> Pellets;

    [Header("Colours"), Space(10)]
    public GameObject[] ColoredBits;
    public Material NormalMaterial;
    public Material ReloadMaterial;
    public Material EmptyMagMaterial;
    public Material EnhancedMaterial;

    [SerializeField] private Transform Cam;


    private void Awake()
    {
        CurrentMag = MagSize;
        CurrentAmmoReserve = AmmoReserve;
    }
    private void Start()
    {
        CurrentBulletPrefab = BulletPrefab;

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
        if (PauseManager.paused) { return; }

        //var position = transform.position;
        transform.forward = GameManager.Instance.cinemachineVirtual.transform.forward;
        //transform.position = position;
        
        if(CurrentMag <= 0) MagEmpty = true;
        else MagEmpty = false;
        
        if (CurrentAmmoReserve <= 0) OutOfAmmo = true;
        else OutOfAmmo = false;

        if (IsEnhanced) CurrentBulletPrefab = EnhancedBulletPrefab;
        else CurrentBulletPrefab = BulletPrefab;

        //transform.forward = Cam.forward;
        
    }

    
}

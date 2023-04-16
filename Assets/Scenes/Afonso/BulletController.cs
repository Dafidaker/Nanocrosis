using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    [SerializeField] private float Speed = 100f;
    [SerializeField] private float TimeToDestroy = 5f;
    [SerializeField] private GameObject EnhancementPickup;
    [SerializeField] private LayerMask ItHits;

    public GameObject Gun;

    public int Damage;
    public Vector3 Target { get; set; }
    public bool Hit { get; set; }

    public bool Enhanced;

    private Rigidbody _rb;
    private bool _spawnedWithShotgun;

    [field: SerializeField] private AudioClip clip;
    [field: SerializeField] private float volume;
    

    private void OnEnable()
    {
        Destroy(gameObject, TimeToDestroy);
        //SoundManager.Instance.PlaySound(clip, volume);
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (Gun.GetComponent<WeaponController>().Name == "Rifle") transform.position = Vector3.MoveTowards(transform.position, Target, Speed * Time.deltaTime);
        else if (Gun.GetComponent<WeaponController>().Name == "Shotgun") _rb.AddForce(transform.forward * Speed);
        if(!Hit && Vector3.Distance(transform.position, Target) < .01f)
        {
            Destroy(gameObject);
        }

        _spawnedWithShotgun = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("bullet") || collision.gameObject.CompareTag("Player")) return;
        if(collision.gameObject.GetComponent<RespawningTargetController>() != null)
        {
            RespawningTargetController d = collision.gameObject.GetComponent<RespawningTargetController>();
            if (d.ShieldActive && Enhanced)
            {
                d.CurrentShieldHealthPoints -= Damage;
            }
            else if (d.ShieldActive && !Enhanced)
            {
                Debug.Log("HAS SHIELD AND AMMO IS NOT ENHANCED");
            }
            else if ((!d.ShieldActive && Enhanced) || (!d.ShieldActive && !Enhanced))
            {
                d.CurrentHealthPoints -= Damage;
            }
        }

        var HitableScript = collision.gameObject.GetComponent<Hitable>();
        if (HitableScript != null)
        {
            PlayerAttacks playerAttacks = PlayerAttacks.Bullet;
            if (Enhanced)
            {
                playerAttacks = PlayerAttacks.BulletEnhanced;
            }

            HitableScript.GotHit(Damage, playerAttacks);
        }
        else
        {
            HitableScript = collision.gameObject.GetComponentInParent<Hitable>();
            
            PlayerAttacks playerAttacks = PlayerAttacks.Bullet;
            if (Enhanced)
            {
                playerAttacks = PlayerAttacks.BulletEnhanced;
            }

            HitableScript.GotHit(Damage, playerAttacks);
        }

        /*if (collision.gameObject.CompareTag("BossPart"))
        {
            var hitable = collision.gameObject.GetComponentInParent<Hitable>();
            
            if (Gun.GetComponent<WeaponController>().IsEnhanced)
            {
                hitable.GotHit(Damage, PlayerAttacks.BulletEnhanced);
            }
            hitable.GotHit(Damage,PlayerAttacks.Bullet);
        }*/
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("bullet hit: " + other.name);
        
        if ((ItHits.value & (1 << other.transform.gameObject.layer)) > 0) 
        {
            
        }
        else 
        {
            return;
        }
        
        
        if(other.CompareTag("bullet") || other.CompareTag("Player")) return;
        if(other.GetComponent<RespawningTargetController>() != null)
        {
            RespawningTargetController d = other.GetComponent<RespawningTargetController>();
            if (d.ShieldActive && Enhanced)
            {
                d.CurrentShieldHealthPoints -= Damage;
            }
            else if (d.ShieldActive && !Enhanced)
            {
                Debug.Log("HAS SHIELD AND AMMO IS NOT ENHANCED");
            }
            else if ((!d.ShieldActive && Enhanced) || (!d.ShieldActive && !Enhanced))
            {
                d.CurrentHealthPoints -= Damage;
            }
        }
        
        
        var HitableScript = other.GetComponent<Hitable>();
        var parentHitableScript = other.gameObject.GetComponentInParent<Hitable>();
        if (HitableScript != null)
        {
            if (Enhanced)
            {
                HitableScript.GotHit(Damage, PlayerAttacks.BulletEnhanced);
            }
            HitableScript.GotHit(Damage,PlayerAttacks.Bullet);
        }
        else if(parentHitableScript != null)
        {
            PlayerAttacks playerAttacks = PlayerAttacks.Bullet;
            if (Enhanced)
            {
                playerAttacks = PlayerAttacks.BulletEnhanced;
            }

            parentHitableScript.GotHit(Damage, playerAttacks);
        }
        

        /*HitableScript = other.GetComponentInParent<Hitable>();
        if (other.CompareTag("BossPart"))
        {
            if (Gun.GetComponent<WeaponController>().IsEnhanced)
            {
                HitableScript.GotHit(Damage, PlayerAttacks.BulletEnhanced);
            }
            HitableScript.GotHit(Damage,PlayerAttacks.Bullet);
        }*/
        Destroy(gameObject);
    }
}

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
        if (Gun.GetComponent<WeaponController>().Name == "Rifle") transform.position = Vector3.MoveTowards(transform.position, Target, Speed * Time.fixedDeltaTime);
        else if (Gun.GetComponent<WeaponController>().Name == "Shotgun") _rb.AddForce(transform.forward * Speed);
        
        if(Vector3.Distance(transform.position, Target) < .01f) //if(!Hit && Vector3.Distance(transform.position, Target) < .01f)
        {
            Destroy(gameObject);
        }

        _spawnedWithShotgun = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("collision");
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

        var HitableScript = collision.gameObject.GetComponent<Hittable>();
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
            HitableScript = collision.gameObject.GetComponentInParent<Hittable>();
            
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
        
        if (!((ItHits.value & (1 << other.transform.gameObject.layer)) > 0))
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
        
        
        var hittableScript = other.GetComponent<Hittable>();
        if (hittableScript != null && hittableScript.GetType() == typeof(OxigenNodeHittable))
        {
            var go = Instantiate(GameManager.Instance.vfxHitAnything, transform.position, Quaternion.identity);
            Destroy(go, 1f);
            Destroy(gameObject);
            return;
        }
        
        /*var parentHitableScript = other.gameObject.GetComponentInParent<Hittable>();*/
        
        hittableScript = hittableScript == null ? other.gameObject.GetComponentInParent<Hittable>() : hittableScript;
        
        var playerAttacks = Enhanced ? PlayerAttacks.BulletEnhanced : PlayerAttacks.Bullet;

        if (hittableScript != null)
        {
            /*var go = Instantiate(hittableScript.vfx, transform.position, Quaternion.identity);
            Destroy(go, 1f);*/
            hittableScript.GotHit(Damage,playerAttacks,transform.position );
        }
        
        var a = Instantiate(GameManager.Instance.vfxHitAnything, transform.position, Quaternion.identity);
        Destroy(a, 1f);
        Destroy(gameObject);
    }
}

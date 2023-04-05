using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    [SerializeField] private float Speed = 100f;
    [SerializeField] private float TimeToDestroy = 5f;
    
    public GameObject Gun;

    public int Damage;
    public Vector3 Target { get; set; }
    public bool Hit { get; set; }

    public bool Enhanced;

    private Rigidbody _rb;


    private void OnEnable()
    {
        Destroy(gameObject, TimeToDestroy);
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
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("bullet") || collision.gameObject.CompareTag("Player")) return;
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
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
        Destroy(gameObject);
    }
}

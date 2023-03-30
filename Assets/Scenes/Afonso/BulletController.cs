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
        if(collision.collider.CompareTag("bullet")) return;
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("bullet")) return;
        if(other.GetComponent<DamageController>() != null)
        {
            DamageController d = other.GetComponent<DamageController>();
            d.CurrentHealthPoints -= Damage;
        }
        Destroy(gameObject);
    }
}

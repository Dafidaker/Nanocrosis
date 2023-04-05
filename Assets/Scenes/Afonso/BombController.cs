using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombController : MonoBehaviour
{
    [SerializeField] private float Speed = 100f;
    [SerializeField] private float TimeToDestroy = 5f;

    private Rigidbody _rb;

    public GameObject Explosion;
    public int ImpactDamage;
    public Vector3 Target { get; set; }
    public bool Hit { get; set; }

    private void OnEnable()
    {
        Destroy(gameObject, TimeToDestroy);
    }

    private void Start()
    {
        Explosion.SetActive(false);
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, Target, Speed * Time.deltaTime);
        if (!Hit && Vector3.Distance(transform.position, Target) < .01f)
        {
            StartCoroutine(Explode());
        }
    }
    private IEnumerator Explode()
    {
        Explosion.SetActive(true);
        yield return new WaitForSeconds(.1f);
        Explosion.SetActive(false);
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        StartCoroutine(Explode());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) return;
        if (other.GetComponent<TargetController>() != null)
        {
            TargetController d = other.GetComponent<TargetController>();
            if(!d.ShieldActive) d.CurrentHealthPoints -= ImpactDamage;            
        }
        StartCoroutine(Explode());
    }
}

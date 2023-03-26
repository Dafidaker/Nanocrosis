using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    [SerializeField] private float Speed = 100f;
    [SerializeField] private float TimeToDestroy = 5f;

    public Vector3 Target { get; set; }
    public bool Hit { get; set; }

    private void OnEnable()
    {
        Destroy(gameObject, TimeToDestroy);
    }

    private void FixedUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, Target, Speed * Time.deltaTime);
        if(!Hit && Vector3.Distance(transform.position, Target) < .01f)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }
}

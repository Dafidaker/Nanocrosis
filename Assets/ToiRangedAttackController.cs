using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToiRangedAttackController : MonoBehaviour
{
    [field: SerializeField] private Vector3 finalSize;
    [field: SerializeField] private float bulletForce;
    [field: SerializeField] private LayerMask Ground_Floor_Player;
    private bool _fullSize;

    public bool animateBall;

    public GameObject particalSystem;

    private Rigidbody _rb;

    [field: SerializeField, Header("Distance Traveled"),Space(10)] private float maxDistance;
    private Vector3 _oldPosition;
    private float _distanceTraveled;


    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        
        ResetBall();
        
        _oldPosition = transform.position;
    }
    
    private void ResetBall()
    {
        transform.localScale = new Vector3(0f, 0f, 0f);
        _fullSize = false;
        animateBall = false;
    }

    private void Update()
    {
        if (animateBall && !_fullSize)
        {
            transform.localScale += Vector3.one * 0.1f;
        }

        if (transform.localScale.magnitude >= finalSize.magnitude)
        {
            _fullSize = true;
        }

        CalculateDistance();
    }

    private void CalculateDistance()
    {
        Vector3 distanceVector = transform.position - _oldPosition;
        float distanceThisFrame = distanceVector.magnitude;
        _distanceTraveled += distanceThisFrame;
        _oldPosition = transform.position;

        if (_distanceTraveled >= maxDistance)
        {
            Destroy(gameObject);
        }
    }
    
    public IEnumerator Attack()
    {
        while (!_fullSize)
        {
            transform.localScale += Vector3.one * 0.1f;
            if (transform.localScale.magnitude >= finalSize.magnitude)
            {
                _fullSize = true;
            }

            yield return null;
        }
    }

    public void ShootRangedAttack(Vector3 direction)
    {
        _rb.velocity = direction * bulletForce;
    }


    //collision

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerStats>().DamageTaken(5);
        }
    }
}

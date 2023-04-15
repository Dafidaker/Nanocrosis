using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToiRangedAttackController : MonoBehaviour
{
    public Coroutine attack;
    
    [field: SerializeField] private Vector3 finalSize;
    [field: SerializeField] private float bulletForce;
    [field: SerializeField] private int damage;
    [field: SerializeField] private LayerMask itHits;
    private bool _fullSize;

    [field: HideInInspector] public bool animateBall;
    [field: HideInInspector] public Transform followPosition;
    
    [field: SerializeField, Header("Distance Traveled"),Space(10)] private float maxDistance;
    private Vector3 _oldPosition;
    private float _distanceTraveled;
    
    private bool IsMoving;
    private Rigidbody _rb;
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        
        ResetBall();
        
        _oldPosition = transform.position;
    }

    private void OnDestroy()
    {
        if (attack != null)
        {
            StopCoroutine(attack);
        }
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

        if (!IsMoving)
        {
            transform.position = followPosition.position;   
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
        IsMoving = true;
        _rb.velocity = direction * bulletForce;
    }


    //collision

    private void OnTriggerEnter(Collider other)
    {
        if (!((itHits.value & (1 << other.transform.gameObject.layer)) > 0)) { return; }
        
        if (other.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerStats>().DamageTaken(5);
        }
        
        Destroy(gameObject);
    }
}

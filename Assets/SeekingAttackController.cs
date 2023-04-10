using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

public class SeekingAttackController : MonoBehaviour
{
    public float speed;
    [field: SerializeField] private float animationSpeed;
    [field: SerializeField] private int damage;
    [field: SerializeField] private LayerMask itHits;
    
    [field: HideInInspector] public NavMeshAgent phageAgent;
    [field: HideInInspector] public Transform spawnPosition;
    private Vector3 _finalScale;
    private Transform _target;
    private SphereCollider sphereCollider;
    private bool _followTarget;
    private Tween _shake;
    
    private void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.enabled = false;
        _target = GameManager.Instance.player.transform;
        _finalScale = transform.localScale;
        transform.localScale = Vector3.zero;
        //_shake = transform.DOShakePosition(1, 1, 0, 0).SetLoops(-1);
        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        _followTarget = false;
        while (transform.localScale.x < _finalScale.x)
        {
            transform.localScale += Vector3.one * animationSpeed;
            yield return null;
        }

        yield return new WaitForSeconds(1f);
        sphereCollider.enabled = true;
        phageAgent.isStopped = false;
        _shake.Kill();
        _followTarget = true;
    }
    private void Update()
    {
        if (_followTarget)
            FollowTarget();
        else
            transform.position = spawnPosition.position;
    }

    private void FollowTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, _target.position, speed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerStats>().DamageTaken(damage);
        }
        
        if ((itHits.value & (1 << other.transform.gameObject.layer)) > 0) {
            Destroy(gameObject);
        }
        
    }
}

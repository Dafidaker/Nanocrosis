using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PhageMinionController : MonoBehaviour
{
    [field: SerializeField] private float distanceToExplode;
    [field: SerializeField] private MeshRenderer explosionMeshRenderer;
    [field: SerializeField] private SphereCollider explosionCollider;
    [field: SerializeField] private Color[] colors;
    [field: SerializeField] private TriggerHitsPlayer triggerHitsPlayer;
    [field: SerializeField] private LayerMask itHits;
    [field: SerializeField] private int damage;
    private float _distanceFromTarget;
    
    private FSMNavMeshAgent _fsmNavMeshAgent;
    private NavMeshAgent _agent;
    private Transform _target;

    private bool _isExploding;

    private void Start()
    {
        _fsmNavMeshAgent = GetComponent<FSMNavMeshAgent>();
        _fsmNavMeshAgent.target = GameManager.Instance.player.transform;
        _agent = _fsmNavMeshAgent._agent;
        _target = _fsmNavMeshAgent.target;
        _isExploding = false;
        triggerHitsPlayer.Initiate(damage , itHits);
    }

    private void Update()
    {
        _distanceFromTarget = Vector3.Distance(transform.position, _target.position);

        if (_distanceFromTarget < distanceToExplode)
        {
            //_agent.isStopped = true;
            _agent.SetDestination(transform.position);
            if (!_isExploding) { StartCoroutine(Explode());}
        }
        
        if (_distanceFromTarget > distanceToExplode && !_isExploding)
        {
            _agent.SetDestination(_target.position);
        }
    }

    private IEnumerator Explode()
    {
        _isExploding = true;
        
        //flashing 
        foreach (var t in colors)
        {
            explosionMeshRenderer.material.color = t;
            explosionMeshRenderer.enabled = true;
            yield return new WaitForSeconds(0.1f);
            explosionMeshRenderer.enabled = false;
            yield return new WaitForSeconds(0.5f);
        }
        
        //circle collider gets scales up to its max size 
        explosionMeshRenderer.enabled = true;
        explosionCollider.enabled = true;
        yield return new WaitForSeconds(0.5f);
        //explosion visual effect
        //if activate trigger to damage people

        //destroy object
        Destroy(gameObject);
    }
    
}

using System;
using System.Collections;
using PathCreation.Examples;
using UnityEngine;
using UnityEngine.AI;

public class KuroruAgent : MonoBehaviour
{
    public PathFollower pathFollower;
    public ParticleSystem ParticleSystem;
    
    private FSMNavMeshAgent _fsmNavMeshAgent;
    private NavMeshAgent _agent;

    private bool _attackGoing;
    private bool _attackDone;
    
    
    #region Unity Functions

    private void OnEnable()
    {
        
    }
    
    private void OnDisable()
    {
        
    }

    private void Awake()
    {
        _fsmNavMeshAgent = GetComponent<FSMNavMeshAgent>();
        _agent = _fsmNavMeshAgent._agent;
        ParticleSystem.gameObject.SetActive(false);
        //StartCoroutine(Attack());
    }
    
    private void Start()
    {
        
    }
    
    private void Update()
    {
        var direction =_fsmNavMeshAgent.target.transform.position - transform.position;
        transform.forward = new Vector3(direction.x, transform.forward.y, direction.z);
    }

    private void FixedUpdate()
    {
        
    }

    #endregion

    #region Utilities
    
    
    #endregion
    
    #region Actions Functions

    public void CallAttack()
    {
        if (!_attackGoing)
        {
            _attackGoing = true;
            StopAllCoroutines();
            StartCoroutine(Attack());
        }
        
    }

    
    
    private IEnumerator Attack()
    {
        //resets the particle system to the left of kuroru
        pathFollower.distanceTravelled = 9f;
        ParticleSystem.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.45f);
        ParticleSystem.gameObject.SetActive(false);
        yield return new WaitForSeconds(1f);
        _attackGoing = false;
    }
    
    public void Chase()
    {
        //Debug.Log("KuroruAgent _ Chase");
        _agent.SetDestination(_fsmNavMeshAgent.target.position);
    }

    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerStats>().DamageTaken(5);
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class FSMNavMeshAgent : MonoBehaviour
{
    public Transform[] patrolWaypoints;
    public Transform target;
    
    //different Agents
    [field: HideInInspector] public ChikaiAgent chikaiAgent;
    [field: HideInInspector] public KuroruAgent kuroruAgent;
    [field: HideInInspector] public ToiAgent toiAgent;
    
    public float canSeeDistance;

    public NavMeshAgent _agent;
    private FiniteStateMachine _fsm;

    public float viewDistance = 0;
    public float viewAngle = 0;
    
    #region Unity Functions

    private void OnEnable()
    {
        
    }
    
    private void OnDisable()
    {
        
    }

    private void Awake()
    {
        
    }
    
    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _fsm = GetComponent<FiniteStateMachine>();
        chikaiAgent = GetComponent<ChikaiAgent>();
        kuroruAgent = GetComponent<KuroruAgent>();
        toiAgent = GetComponent<ToiAgent>();
    }
    
    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        
    }

    #endregion

    #region Utilities
    
    public bool IsAtDestination()
    {
        if (!_agent.pathPending)
        {
            if (_agent.remainingDistance <= _agent.stoppingDistance)
            {
                if (!_agent.hasPath || _agent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }

        return false;
    }
    
    #endregion
    
    #region Actions Functions



    #endregion
    
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f,0f,0f,0.2f);
        Gizmos.DrawSphere(transform.position, viewDistance);
        //Gizmos.DrawLine(transform.position, Quaternion.Euler(0, viewAngle/2, 0) * transform.forward);
        //Gizmos.DrawLine(transform.position, Quaternion.Euler(0, -viewAngle/2, 0) * transform.forward);
    }

    
}

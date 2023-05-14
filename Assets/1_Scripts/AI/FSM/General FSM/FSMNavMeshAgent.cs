using System;
using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class FSMNavMeshAgent : MonoBehaviour
{
    public List<Transform> patrolWaypoints;
    public Transform target;
    
    //different Agents
    [field: HideInInspector] public ChikaiAgent chikaiAgent;
    [field: HideInInspector] public KuroruAgent kuroruAgent;
    [field: HideInInspector] public ToiAgent toiAgent;
    private Hittable hittable;
    
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
        target = GameManager.Instance.player.transform;

        if (chikaiAgent != null)
        {
            chikaiAgent.CalledStart();
        }
        else if (kuroruAgent != null)
        {
            kuroruAgent.CalledStart();
        }
        else if (toiAgent != null)
        {
            toiAgent.CalledStart();
        }

        hittable = GetComponent<Hittable>();

        GetWaypoints();

    }
    
    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        
    }

    #endregion

    #region Utilities

    private void GetWaypoints()
    {
        patrolWaypoints = new List<Transform>();
        patrolWaypoints.AddRange(GameManager.Instance.currentArena.waypoints);
    }
    
    public bool IsAtDestination()
    {
        if (!_agent.pathPending)
        {
            if (_agent.remainingDistance <= _agent.stoppingDistance)
            {
                if (!_agent.hasPath || _agent.velocity.sqrMagnitude <= 0.1f)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public Vector3 DiretionToTarget(bool predictFuture = false, float howTimeIntheFuture = 2)
    {
        var targetPosition = Vector3.zero;
        var targetRigidbody = target.GetComponent<Rigidbody>();
        
        if (!predictFuture || targetRigidbody == null)
        {
            targetPosition = target.position;
        }
        else if(targetRigidbody != null)
        {
            targetPosition = TargetFuturePosition(targetRigidbody, howTimeIntheFuture);
        }
        
        return (targetPosition - transform.position).normalized;
       
    }
    
    public Vector3 TargetFuturePosition(Rigidbody targetRigidBody, float howTimeInTheFuture)
    {
        var velocity = targetRigidBody.velocity;
        var targetDirection = velocity.normalized;
        var targetSpeed = velocity.magnitude;
        
        if (targetSpeed == 0 || targetDirection == Vector3.zero)
        {
            return  target.position + target.forward * howTimeInTheFuture;
        }
        return  target.position + targetDirection * (targetSpeed * howTimeInTheFuture);
    }

    public void CheckNode(Component sender, object data)
    {
        if (hittable.attackPlayer) { return; }

        if (!target.GetComponent<OxigenNodeHittable>().targetable)
        {
            hittable.SelectTarget();
        }
    }

    #endregion

    #region Actions Functions



    #endregion


    /*void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f,0f,0f,0.2f);
        Gizmos.DrawSphere(transform.position, viewDistance);
        //Gizmos.DrawLine(transform.position, Quaternion.Euler(0, viewAngle/2, 0) * transform.forward);
        //Gizmos.DrawLine(transform.position, Quaternion.Euler(0, -viewAngle/2, 0) * transform.forward);
    }*/


}

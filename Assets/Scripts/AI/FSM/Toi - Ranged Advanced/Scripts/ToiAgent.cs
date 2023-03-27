using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ToiAgent : MonoBehaviour
{
    private FSMNavMeshAgent _fsmNavMeshAgent;
    private FiniteStateMachine finiteStateMachine;
    private NavMeshAgent _agent;

    public float AttackCooldown;
    public float attackTimer;
    public bool _attackGoing;
    public bool isAttackOver;

    [Header("States"), Space(10)]
    public State ChaseState;
    public State ChaseCarefullyState;
    public State RangeAttackState;
    public State MeleeAttackState;
    public State EvadeState;
    
    public int currentHealth;
    public int maxHealth;
    public bool hasShield;
    public float DistanceToTarget;
    
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
        finiteStateMachine = GetComponent<FiniteStateMachine>();

    }
    
    private void Start()
    {
        
    }
    
    private void Update()
    {
        DistanceToTarget = (_fsmNavMeshAgent.target.position - _agent.transform.position).magnitude;

        if (finiteStateMachine.currentState == ChaseState || finiteStateMachine.currentState == ChaseCarefullyState || finiteStateMachine.currentState == EvadeState)
        {
            attackTimer -= Time.deltaTime;
        }
        else
        {
            Debug.Log(finiteStateMachine.currentState.name);
            attackTimer = 5f;
        }
    }

    private void FixedUpdate()
    {
        
    }

    #endregion

    #region Utilities
    
    private IEnumerator Attack()
    {
        _agent.isStopped = true;
        _attackGoing = true;
        isAttackOver = false;
        //attackTimer = 5f;
        yield return new WaitForSeconds(5f);
        isAttackOver = true;
        _agent.isStopped = false;
        _attackGoing = false;
    }
    
    #endregion
    
    #region Actions Functions

    public void ChaseAction()
    {
        _agent.SetDestination(_fsmNavMeshAgent.target.position);
        Debug.Log("ChaseAction");
    }
    
    public void MeleeAttackAction()
    {
        if (!_attackGoing)
        {
            StopAllCoroutines();
            StartCoroutine(Attack());
        }
        Debug.Log("MeleeAttackAction");
    }
    
    public void RangedAttackAction()
    {
        Debug.Log("RangedAttackAction");
        if (!_attackGoing)
        {
            StopAllCoroutines();
            StartCoroutine(Attack());
        }
    }
    
    public void ChaseCarefullyAction()
    {
        _agent.SetDestination(_fsmNavMeshAgent.target.position);
        Debug.Log("ChaseCarefullyAction");
    }
    
    public void EvadeAction()
    {
        Debug.Log("EvadeAction");
        var patrolWaypoints = _fsmNavMeshAgent.patrolWaypoints;
        var target = _fsmNavMeshAgent.target;
        var selectedWaypoint = patrolWaypoints[0];
        var currentDistance = Vector3.Distance(selectedWaypoint.position, target.transform.position);
        
        foreach (var waypoint in patrolWaypoints)
        {
            if (!(Vector3.Distance(waypoint.position, _fsmNavMeshAgent.target.transform.position) > currentDistance)) continue;
            selectedWaypoint = waypoint;
            currentDistance = Vector3.Distance(waypoint.position, target.transform.position);
        }

        _agent.SetDestination(selectedWaypoint.position);
    }

    #endregion

}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PathCreation;
using PathCreation.Examples;
using UnityEngine;
using UnityEngine.AI;


public class ToiAgent : MonoBehaviour
{
    //Finite State Machine and NavMesh
    private FSMNavMeshAgent _fsmNavMeshAgent;
    private FiniteStateMachine finiteStateMachine;
    private NavMeshAgent _agent;

    [Header("States"), Space(10)]
    [field: SerializeField] private State chaseState;
    [field: SerializeField] private State chaseCarefullyState;
    [field: SerializeField] private State rangeAttackState;
    [field: SerializeField] private State meleeAttackState;
    [field: SerializeField] private State evadeState;
    
    [Header("Stats"), Space(10)]
    public int currentHealth;
    public int maxHealth;
    public bool hasShield;
    public float attackCooldown;
    public float distanceToTarget;
    [field: HideInInspector]public float attackTimer;
    [field: HideInInspector] public bool isAttacking;
    
    [Header("Ranged Attack"),Space(10)]
    [field: SerializeField] private Transform[] rangedAttackPositions;
    private List<ToiRangedAttackController> _rangedAttacks;
    [field: SerializeField] private GameObject rangedAttackPrefab;

    [Header("Melee Attack"),Space(10)] 
    [field: SerializeField] private GameObject meleeAttackPrefab;
    [field: SerializeField] private Transform[] meleeAttackTransforms;
    [field: SerializeField] private float spinDuration;
    [field: SerializeField] private float launchDuration;
    private List<ToiMeleeAttackController> _meleeAttacks;
    private List<Vector3> _meleeAttackPositions;
    
    
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

        currentHealth = maxHealth;
        attackTimer = attackCooldown;
    }
    
    private void Start()
    {
        _rangedAttacks = new List<ToiRangedAttackController>();
        _meleeAttacks = new List<ToiMeleeAttackController>();
        _meleeAttackPositions = new List<Vector3>();
        foreach (var meleeTransform in meleeAttackTransforms)
        {
            _meleeAttackPositions.Add(meleeTransform.position);
        }
    }
    
    private void Update()
    {
        distanceToTarget = (_fsmNavMeshAgent.target.position - _agent.transform.position).magnitude;

        if (finiteStateMachine.currentState == chaseState || finiteStateMachine.currentState == chaseCarefullyState || finiteStateMachine.currentState == evadeState)
        {
            attackTimer -= Time.deltaTime;
        }
        else
        {
            attackTimer = attackCooldown;
        }
    }

    private void FixedUpdate()
    {
        
    }

    #endregion

    #region Utilities
    
    private IEnumerator PerformRangedAttack()
    {
        _agent.isStopped = true;
        isAttacking = true;
        
        foreach (var t in rangedAttackPositions)
        {
            var temp = Instantiate(rangedAttackPrefab, t.position, t.rotation);
            _rangedAttacks.Add(temp.GetComponent<ToiRangedAttackController>());
            StartCoroutine(temp.GetComponent<ToiRangedAttackController>().Attack());
            yield return new WaitForSeconds(1f);
        }

        foreach (var attack in _rangedAttacks.ToArray())
        {
            if (attack == null)
            {
                _rangedAttacks.Remove(attack);
                continue;
            }
            attack.ShootRangedAttack((_fsmNavMeshAgent.target.position - attack.transform.position).normalized);
            yield return new WaitForSeconds(1f);
        }
        
        yield return new WaitForSeconds(2f);
        
        isAttacking = false;
        _agent.isStopped = false;
    }

    private IEnumerator PerformMeleeAttack()
    {
        _agent.isStopped = true;
        isAttacking = true;

        for (int i = 0; i < _meleeAttackPositions.Count; i++)
        {
            //create melee attack and get its script
            var temporaryMeleeAttack = Instantiate(meleeAttackPrefab, meleeAttackTransforms[i].position, meleeAttackTransforms[i].rotation);
            var temporaryMeleeScript = temporaryMeleeAttack.GetComponent<ToiMeleeAttackController>();
            
            //add the positions in order
            
            var count = 0;
            var index = i;
            while (count <= _meleeAttackPositions.Count - 1)
            {
                if (index > _meleeAttackPositions.Count - 1) { index = 0; }
                temporaryMeleeScript.pathNodes.Add(_meleeAttackPositions[index]);
                count++;
                index++;
            }
            
            //add it to the list of all the melee attacks
            _meleeAttacks.Add(temporaryMeleeScript);
            temporaryMeleeScript.toiTransform = gameObject.transform;
            temporaryMeleeScript.timeRotating = spinDuration;
            temporaryMeleeScript.CreateAttack();
            
            yield return null;
        }
        
        yield return new WaitForSeconds(spinDuration + launchDuration);
        
        isAttacking = false;
        _agent.isStopped = false;
    }

    #endregion
    
    #region Actions Functions

    public void ChaseAction()
    {
        _agent.SetDestination(_fsmNavMeshAgent.target.position);
        //Debug.Log("ChaseAction");
    }
    
    public void MeleeAttackAction()
    {
        if (!isAttacking)
        {
            StopAllCoroutines();
            //todo create performed melee attack
            StartCoroutine(PerformMeleeAttack());
        }
        //Debug.Log("MeleeAttackAction");
    }
    
    public void RangedAttackAction()
    {
        //Debug.Log("RangedAttackAction");
        if (!isAttacking)
        {
            StopAllCoroutines();
            StartCoroutine(PerformRangedAttack());
        }
    }
    
    public void ChaseCarefullyAction()
    {
        _agent.SetDestination(_fsmNavMeshAgent.target.position);
        //Debug.Log("ChaseCarefullyAction");
    }
    
    public void EvadeAction()
    {
        //Debug.Log("EvadeAction");
        
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

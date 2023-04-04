using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Versioning;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class ChikaiAgent : MonoBehaviour
{
    private FSMNavMeshAgent _fsmNavMeshAgent;
    private FiniteStateMachine finiteStateMachine;
    private NavMeshAgent _agent;
    private Transform _target;

    [Header("Debug"), Space(10)] 
    [field: SerializeField] private GameObject debugPrefab;
    
    [Header("States"), Space(10)]
    [field: SerializeField] private State chaseState;
    [field: SerializeField] private State chaseCarefullyState;
    [field: SerializeField] private State rangeAttackState;
    [field: SerializeField] private State meleeAttackState;

    [Header("Stats"), Space(10)]
    [field: HideInInspector] public int currentHealth;
    public int maxHealth;
    public bool hasShield;
    public float attackCooldown;
    public float distanceToTarget;
    [field: HideInInspector]public float attackTimer;
    [field: HideInInspector] public bool isAttacking;
    private bool _rotateTowardsTarget;

    [Header("Ranged Headbutt Attack"), Space(10)]
    [field: SerializeField] private float antecipationDuration;
    [field: SerializeField] private float headbuttForce;
    [field: SerializeField] private float headbuttDuration;
    [field: SerializeField] private BoxCollider headbuttTrigger;
    
    [Header("Rotation Attack"), Space(10)]
    [field: SerializeField] private float rotationAttackVelocity;
    [field: SerializeField] private Vector3 rotation;
    [field: SerializeField] private float rotationSpeed;
    [field: SerializeField] private BoxCollider rotationTrigger;
    private bool _rotate;
    
    [Header("Pouce Attack"), Space(10)]
    [field: SerializeField] private float rangeAttackSpeed;
    [field: SerializeField] private float jumpSpeed;
    [field: SerializeField] private AnimationCurve heightCurve;
    [field: SerializeField] private Transform[] bouncePositions;
    [field: HideInInspector] private bool _bouncing;
    
    [Header("Componenets"), Space(10)]
    [field: SerializeField] private TargetController targetController;
    private Rigidbody _rb;
    
    
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
        _rb = GetComponent<Rigidbody>();
        _target = _fsmNavMeshAgent.target;
    }
    
    private void Start()
    {
        currentHealth = maxHealth;

    }
    
    private void Update()
    {
        distanceToTarget = (_fsmNavMeshAgent.target.position - _agent.transform.position).magnitude;

        if (finiteStateMachine.currentState != meleeAttackState && finiteStateMachine.currentState != rangeAttackState)
        {
            attackTimer -= Time.deltaTime;
        }
        else
        {
            attackTimer = attackCooldown;
        }

        if (_rotateTowardsTarget) transform.rotation = Quaternion.Slerp(transform.rotation, 
                    Quaternion.LookRotation(_target.transform.position- transform.position), 0.5f);
        
    }

    private void FixedUpdate()
    {
        if (_rotate)
        {
            transform.Rotate(rotation * (rotationSpeed * Time.fixedDeltaTime),Space.Self);
        }
    }

    #endregion

    #region Utilities
    
    
    #endregion
    
    #region Actions Called
    
    public void ChaseCalled()
    {
        _agent.SetDestination(_fsmNavMeshAgent.target.position);
    }
    
    public void ChaseCarefullyCalled()
    {
        _agent.SetDestination(_fsmNavMeshAgent.target.position);
    }
    
    public void MeleeAttackCalled()
    {
        if (!isAttacking)
        {
            Debug.Log("ChikaiAgent _ Melee Attack");
            StartCoroutine(PerformMeleeAttack());
        }
        _agent.SetDestination(_fsmNavMeshAgent.target.position);
    }
    
    public void RangeAttackCalled()
    {
        if (!isAttacking)
        {
            Debug.Log("ChikaiAgent _ Ranged Attack");
            StartCoroutine(PerformRangeAttack());
        }
    }

    #endregion

    #region Attack Logic

    private IEnumerator PerformMeleeAttack()
    {
        isAttacking = true;
        _agent.updateRotation = false;
        //_agent.isStopped = false;
        var agentVelocity = _agent.velocity;
        var agentDrag = _rb.drag;
        _rotateTowardsTarget = false;
        
        //_agent.velocity = agentVelocity * 2;
        //_rb.drag = 0.5f;

        isAttacking = true;
        _rotate = true;
        yield return new WaitForSeconds(1f);
        rotationTrigger.enabled = true;
        _rb.velocity = _fsmNavMeshAgent.DiretionToTarget(true, 1.1f) * rotationAttackVelocity;
        yield return new WaitForSeconds(1f);
        _rb.velocity = _fsmNavMeshAgent.DiretionToTarget(true, 1.1f)* rotationAttackVelocity;
        yield return new WaitForSeconds(1f);
        _rb.velocity = _fsmNavMeshAgent.DiretionToTarget(true, 1.1f) * rotationAttackVelocity;
        yield return new WaitForSeconds(1f);
        
        
        rotationTrigger.enabled = false;
        _rotate = false;
        _rotateTowardsTarget = true;
        _agent.velocity = agentVelocity;
        _rb.drag = agentDrag;
        
        _agent.updateRotation = true;
        _agent.isStopped = false;
        isAttacking = false;
    }

    private void Bounce(bool endOnGround = false)
    {
        _bouncing = true;
        var sequence = DOTween.Sequence();
        var bouncePaths = new List<Vector3[]>();
        if (bouncePaths == null) throw new ArgumentNullException(nameof(bouncePaths));
        
        for (int i = 0; i < bouncePositions.Length; i++)
        {
            var path = new List<Vector3>();
            if (i == bouncePositions.Length -1 && !endOnGround)
            {
               path.Add(bouncePositions[i].position);
               sequence.Append(transform.DOPath(path.ToArray(), Random.Range(0.5f, 1f), PathType.CatmullRom).SetEase(Ease.InSine));
            }
            else
            {
                path.Add(bouncePositions[i].position); 
                path.Add(transform.position); 
                sequence.Append(transform.DOPath(path.ToArray(), Random.Range(0.5f, 1f), PathType.CatmullRom).SetEase(Ease.OutBounce).SetEase(Ease.InSine));
            }
            
            
        }
        
        sequence.OnComplete(() =>
        {
            //ONCE THE SEQUENCED ENDS 
            _bouncing = false;
        });
        
    }
    private IEnumerator PerformRangeAttack()
    {
        
        isAttacking = true;
        _agent.updateRotation = false;
        _agent.isStopped = true;
        _rotateTowardsTarget = true;

        transform.forward = _fsmNavMeshAgent.DiretionToTarget();
        _rb.angularVelocity = Vector3.zero; //todo do a lerp to slow down angular velocity
        
        var headbutting = true;
        
        var sequence = DOTween.Sequence();
        
        //var initialPosition = transform.position;

        var position = transform.position;
        //var right = transform.right.normalized * 3f;
        var localRight = transform.worldToLocalMatrix.MultiplyVector(transform.right) * 3f;
        
        
        sequence.Append(transform.DOPunchPosition(transform.right.normalized * 2f, antecipationDuration,0,0));
        sequence.Append(transform.DOPunchPosition(-transform.right.normalized * 2f, antecipationDuration,0,0));
        sequence.Append(transform.DOPunchPosition(transform.right.normalized * 2f, antecipationDuration,0,0));
        
        
        sequence.OnComplete(() =>
        {
            headbutting = false;
        });

        while (headbutting)
        {
            yield return null;
        }
        headbuttTrigger.enabled = true;
        _rb.velocity = _fsmNavMeshAgent.DiretionToTarget(true, 1.1f) * headbuttForce;
        
        yield return new WaitForSeconds(2f);
        headbuttTrigger.enabled = false;
        _rotateTowardsTarget = false;
        _agent.updateRotation = true;
        _agent.isStopped = false;
        isAttacking = false;
        
        
        
    }
    
    #endregion

    #region Collisions

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);
        if (other.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerStats>().DamageTaken(5);
        }
    }

    #endregion
    
    //Ranged Attack Pounce
    /*
     * _agent.updateRotation = false;
        _agent.isStopped = true;
        isAttacking = true;
        rotateTowardsTarget = true;
        
        Bounce();
        
        while(_bouncing) { yield return null; }
        
        var startingPosition = transform.position;

        var targetsFuturePosition =
            _fsmNavMeshAgent.TargetFuturePosition(_target.GetComponent<Rigidbody>(), 1.3f);
        for (float time = 0; time < 1; time += Time.deltaTime * jumpSpeed)
        {
            transform.position = Vector3.Lerp(startingPosition, targetsFuturePosition, time) 
                                 + Vector3.up * HeightCurve.Evaluate(time);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_target.position- transform.position), time );
            yield return null;
        }
        
        Bounce(true);
        
        while(_bouncing) { yield return null; }
        
        yield return new WaitForSeconds(2f);
       
        rotateTowardsTarget = false;
        _agent.updateRotation = true;
        _agent.isStopped = false;
        isAttacking = false;
     */
    
    
   /* void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, viewDistance);
        Gizmos.DrawLine(transform.position, Quaternion.Euler(0, viewAngle/2, 0) * transform.forward);
        Gizmos.DrawLine(transform.position, Quaternion.Euler(0, -viewAngle/2, 0) * transform.forward);
    }*/
}



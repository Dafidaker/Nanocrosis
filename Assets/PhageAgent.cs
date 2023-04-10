using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class PhageAgent : MonoBehaviour
{
    private PhageState _phageState;
    private FSMNavMeshAgent _fsmNavMeshAgent;
    private NavMeshAgent _agent;
    private Transform _target;
    private Hitable _hitableScript;
    
    [Header("Stats"), Space(10)]
    [field: SerializeField] private AnimationCurve timeBetweenAttacksMultiplier;
    [field: SerializeField] private GameObject debugPrefab;
    [field: SerializeField] private float timeBetweenAttacks;
    private float _betweenAttacksTimer;
    
    [Header("Explosive Minions Attack"), Space(10)]
    [field: SerializeField] private Transform explosiveMinionsSpawn;
    [field: SerializeField] private AnimationCurve explosiveMinionsAmount;
    [field: SerializeField] private GameObject minionPrefab;
    [field: SerializeField] private float timeBetweenSpawningEachMinion;
    [field: SerializeField] private float explosiveMinionsCooldown;
    private float _explosiveMinionsTimer;
     
     [Header("Seeking Attack"), Space(10)] // amount - speed - health 
     [field: SerializeField] private AnimationCurve seekingModifications;
     [field: SerializeField] private Transform seekingAttackSpawn;
     [field: SerializeField] private GameObject seekingAttackPrefab;

     [Header("Unexpected Bomb"), Space(10)] 
     [field: SerializeField] private Vector3[] bombFinalPositions;
     [field: SerializeField] private float bombDistanceFromPlayer;
     [field: SerializeField] private GameObject unexpectedBombPrefab;
     [field: SerializeField] private Transform unexpectedBombSpawn;
     
     [Header("Acid Shot"), Space(10)]
     [field: SerializeField] private GameObject acidShotPrefab;
     [field: SerializeField] private Transform acidShotSpawn;
     [field: SerializeField] private float AcidShotSpread;
     [field: SerializeField] private AnimationCurve acidShotAmount;
     private Vector3 _direction = Vector3.zero;
    
    private int _currentWaypointIndex;
    
    private enum PhageState
    {
        Attacking,
        Moving
    }
    
    private enum PhageAttack
    {
        //ExplosiveMinion,
        SeekingAttack,
        //UnexpectedBomb,
        AcidShot
    }

    #region Unity Funtions

    private void Start()
    {
        _fsmNavMeshAgent = GetComponent<FSMNavMeshAgent>();
        _agent = _fsmNavMeshAgent._agent;
        _target = _fsmNavMeshAgent.target;
        _currentWaypointIndex = 0;

        _hitableScript = GetComponent<Hitable>();
        _target = _fsmNavMeshAgent.patrolWaypoints[_currentWaypointIndex];

        _betweenAttacksTimer = timeBetweenAttacks;
        //_explosiveMinionsTimer = explosiveMinionsCooldown;
        _phageState = PhageState.Moving;
        DefineWaypoint();
    }

    private void Update()
    {
        //Debug.Log("_agent.isStopped: " + _agent.isStopped);
        
        Debug.DrawRay(acidShotSpawn.position, _direction * 100, Color.magenta);
        
        if (!_agent.isStopped && !_fsmNavMeshAgent.IsAtDestination())
        {
            //_agent.SetDestination(_target.position);
        }

        if (_phageState == PhageState.Attacking)
        {
            return;
        }
        
        if (_fsmNavMeshAgent.IsAtDestination())
        {
            DefineRandomWaypoint();
        }
        
        if (_betweenAttacksTimer >= 0)
        {
            _betweenAttacksTimer -= Time.deltaTime;
        }

        if (_betweenAttacksTimer < 0)
        {
            _phageState = PhageState.Attacking;
            DecideAttack();
        }

        if (_explosiveMinionsTimer >= 0)
        {
            _explosiveMinionsTimer -= Time.deltaTime;
        }
        
        if (_explosiveMinionsTimer < 0)
        {
            _phageState = PhageState.Attacking;
            _explosiveMinionsTimer = explosiveMinionsCooldown;
            StartCoroutine(ExplosiveMinions());
        }
    }

    #endregion

    #region Funtions

    private void DefineWaypoint()
    {
        _currentWaypointIndex++;

        if (_currentWaypointIndex > _fsmNavMeshAgent.patrolWaypoints.Length - 1)
        {
            _currentWaypointIndex = 0;
        }
        
        _agent.SetDestination(_fsmNavMeshAgent.patrolWaypoints[_currentWaypointIndex].position);
    }
    
    private void DefineRandomWaypoint()
    {
        var newWaypointIndex = Random.Range(0, _fsmNavMeshAgent.patrolWaypoints.Length);
        while (newWaypointIndex == _currentWaypointIndex)
        {
            newWaypointIndex = Random.Range(0, _fsmNavMeshAgent.patrolWaypoints.Length);
        }

        _currentWaypointIndex = newWaypointIndex;
        _agent.SetDestination(_fsmNavMeshAgent.patrolWaypoints[_currentWaypointIndex].position);
    }

    private void ResetAttack()
    {
        _betweenAttacksTimer = timeBetweenAttacksMultiplier.Evaluate(_hitableScript.GetLostHealthPercentage()) * timeBetweenAttacks;
        _phageState = PhageState.Moving;
    }

    private void DecideAttack()
    {
        var array = Enum.GetValues(typeof(PhageAttack));
        var index = Random.Range(0, array.Length);

        var attack =  (PhageAttack) array.GetValue(index);

       
        switch (attack)
        {
            case PhageAttack.AcidShot:
                StartCoroutine(AcidShot());
                break;
            case PhageAttack.SeekingAttack:
                StartCoroutine(SeekingAttack());
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    #endregion


    #region Attacks

    private IEnumerator ExplosiveMinions()
    {
        _agent.isStopped = true;
        //Debug.Log(_hitableScript.GetLostHealthPercentage());
        var amountOfMinions = (int)explosiveMinionsAmount.Evaluate(_hitableScript.GetLostHealthPercentage());
        //Debug.Log("ExplosiveMinions");

        for (var i = 1; i <= amountOfMinions; i++)
        {
            Instantiate(minionPrefab , explosiveMinionsSpawn.position, explosiveMinionsSpawn.rotation , transform);
            if (i != amountOfMinions)
            {
                yield return new WaitForSeconds(timeBetweenSpawningEachMinion);
            }
        }
        
        _agent.isStopped = false;
        yield return null;
        ResetAttack();
    }
    
    private IEnumerator SeekingAttack()
    {
        _agent.isStopped = true;
        
        var go = Instantiate(seekingAttackPrefab, seekingAttackSpawn.position, seekingAttackSpawn.rotation);
        var script = go.GetComponent<SeekingAttackController>();
        script.phageAgent = _agent;
        script.spawnPosition = seekingAttackSpawn;
        
        //waits for the seeking attack script to set the agent back to isStopped = false
        //when the attack is finishied with the animation
        while (_agent.isStopped && go != null)
        {
            Debug.Log(go);
            yield return null;
        }

        _agent.isStopped = false;
        
        Debug.Log("SeekingAttack");
        ResetAttack();
    }
    
    private IEnumerator UnexpectedBomb()
    {
        _agent.isStopped = true;

        var playerPosition = GameManager.Instance.player.transform.position;
        
        //create the bombs 

        List<RaycastHit> raycastHits = new List<RaycastHit>();

        foreach (var vector3 in bombFinalPositions)
        {
            Physics.Raycast(playerPosition + (vector3 + Vector3.up) * bombDistanceFromPlayer, Vector3.down, out var raycastHit);
            raycastHits.Add(raycastHit);
            //Instantiate(debugPrefab, playerPosition + vector3 * bombDistanceFromPlayer, Quaternion.identity);
            Instantiate(debugPrefab, raycastHit.point, Quaternion.identity);
            var go = Instantiate(unexpectedBombPrefab, unexpectedBombSpawn.position, Quaternion.identity);
            go.GetComponent<UnexpectedBombController>().finalPosition = raycastHit.point;    
        }
        
       
        Debug.Log("UnexpectedBomb"); 
        yield return null;
        ResetAttack();
        _agent.isStopped = false; // when the attack is fully done the boss goes back to walking 
    }
    
    private IEnumerator AcidShot()
    {
        //Debug.Log("AcidShot");
        
        //create the acid shots with the spread 
        var amountShots = (int)acidShotAmount.Evaluate(_hitableScript.GetLostHealthPercentage() / 100);
        
        //amountShots = 5;
        var diretionToTarget = (GameManager.Instance.player.transform.position - acidShotSpawn.position).normalized ;

        if (amountShots == 1)
        {
            var go = Instantiate(acidShotPrefab, acidShotSpawn.position, acidShotSpawn.rotation);
            go.transform.forward = diretionToTarget;
        }
        else
        {
             var angleBetweenShots = AcidShotSpread / (amountShots - 1);
             
             //sets the direction to the more left part of the spread
             _direction = diretionToTarget;
             _direction = Quaternion.Euler(0, -AcidShotSpread/2, 0) * _direction;
             var go = Instantiate(acidShotPrefab, acidShotSpawn.position, acidShotSpawn.rotation);
             go.transform.forward = _direction;
     
             //for each acid shot 
             for (int i = 1; i < amountShots; i++)
             {
                 _direction = Quaternion.Euler(0, angleBetweenShots, 0) * _direction;
                 go = Instantiate(acidShotPrefab, acidShotSpawn.position, acidShotSpawn.rotation);
                 go.transform.forward = _direction;
             }
        }  
        
        yield return null;
        ResetAttack();
    }
        
        
    
        
    }

    #endregion
    




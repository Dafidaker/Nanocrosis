using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Enums;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Hittable : MonoBehaviour
{
    [Header("Events"), Space(5)] protected GameEvent _enemyDied;
    
    //Components 
    private FSMNavMeshAgent _fsmNavMeshAgent;
    
    [Header("Enemy Stats"), Space(5)] 
    public Enemy enemyType;
    public AttackRange attackRange;
    public Arena location;
    public bool isItEnemy;
    public bool canHaveShield;
    public bool onlyDamagableByExplosion;
    public bool attackPlayer;
    [field: SerializeField] private float alertByPlayerDistance;
    [field: SerializeField] private float alertDistance;
    
    [Header("Health Stats"), Space(5)] 
    public int maxHealth;
    /*[field: HideInInspector]*/ public int currentHealth;
        
    [Header("Shield Stats"), Space(5)] 
    [field: SerializeField] protected GameObject shieldGameObject;
    public int maxShieldHealth;
    /*[field: HideInInspector]*/ public int currentShieldHealth;
    [field: HideInInspector] public bool hasShield;
    [field: HideInInspector] public bool shieldIsActive;
    [field: SerializeField] private float shieldCooldown;
    [field: SerializeField, Range(0f,1f)] protected float haveShieldChance;
    private Coroutine _rechargeShield;
    private bool _rechargingShield;
    
    [Header("Hit Animation"), Space(5)] 
    [field: SerializeField] private bool animate;
    [field: SerializeField] private float duration;
    [field: SerializeField] private float strength;
    [field: SerializeField] private float vibrato;
    [field: SerializeField] private float randomness;
    [field: SerializeField] private bool fadeOut;
    
    [Header("Damage Text Pop up"), Space(5)] 
    public GameObject damageText;
    [field: SerializeField] private Transform textPosition;
    [field: SerializeField] private float textDuration;
    [field: SerializeField] private float textSize = 1;
    protected TextMesh[] _damageTextMesh;
    protected ReduceSize[] _reduceSizes;


    // Death Drops
    protected GameObject _ammo;
    protected GameObject _specialAmmo;
        
        
    [Header("Death Sound"), Space(5)] 
    [field: SerializeField] private bool doSound;
    [field: SerializeField] private AudioClip clip;
    [field: SerializeField] private float volume;
    
    private Tween _shaking;
    private bool isShaking;
    
    private void Start()
    {
        _fsmNavMeshAgent = GetComponent<FSMNavMeshAgent>();
        
        currentHealth = maxHealth;
        _damageTextMesh = damageText.GetComponentsInChildren<TextMesh>();
        _reduceSizes = damageText.GetComponentsInChildren<ReduceSize>();

        _ammo = GameManager.Instance.ammo;
        _specialAmmo = GameManager.Instance.specialAmmo;

        _enemyDied = GameEvents.Instance.enemyDied;

        if (canHaveShield && Random.Range(0f,1f) <= haveShieldChance)
        {
            hasShield = true;
            shieldIsActive = true;
            shieldGameObject.SetActive(true);
            currentShieldHealth = maxShieldHealth;
        }

        SelectTarget();
    }

    private void OnDestroy()
    {
        _shaking?.Kill();

        if (_rechargeShield != null)
        {
            StopCoroutine(_rechargeShield);
        }
    }

    public void SelectTarget()
    {
        attackPlayer = false;
        
        if (location == Arena.Heart)
        {
            attackPlayer = true;
        }
        
        if (Vector3.Distance(GameManager.Instance.player.transform.position , transform.position) <= alertByPlayerDistance)
        {
            attackPlayer = true;
        }

        if (!AreThereAvaliableNode())
        {
            attackPlayer = true;
        }
        
        _fsmNavMeshAgent.target = attackPlayer ? GameManager.Instance.player.transform : GetClosestHittableNode();
        
    }

    public void Disperse()
    {
        attackPlayer = false;
        _fsmNavMeshAgent.target = AreThereAvaliableNode() ? GetClosestHittableNode() 
            : GameManager.Instance.currentArena.waypoints[Random.Range(0,GameManager.Instance.currentArena.waypoints.Length)];
    }

    public void CheckTarget()
    {
        if (!_fsmNavMeshAgent.target.CompareTag("Player") && !_fsmNavMeshAgent.target.CompareTag("OxygenNode"))
        {
            _fsmNavMeshAgent.target = GameManager.Instance.player.transform;
        }
    }

    private bool AreThereAvaliableNode()
    {
        return attackRange switch
        {
            AttackRange.Melee when GameManager.Instance.hittableLowNodes.Count <= 0 => false,
            AttackRange.Ranged when GameManager.Instance.hittableHighNodes.Count <= 0 => false,
            _ => true
        };
    }

    private Transform GetClosestHittableNode()
    {
        List<OxigenNodeHittable> nodeList;
        switch (attackRange)
        {
            default:
            case AttackRange.Melee:
                nodeList = GameManager.Instance.hittableLowNodes;
                break;
            case AttackRange.Ranged:
                nodeList = GameManager.Instance.hittableHighNodes;
                break;
        }
        
        var smallestDistance = float.MaxValue;
        var closestNode = nodeList[0];

        foreach (var oxygenNode in nodeList)
        {
            var distance = Vector3.Distance(transform.position, oxygenNode.transform.position);
            
            if (!(distance < smallestDistance)) continue;
            smallestDistance = distance;
            closestNode = oxygenNode;
        }

        return closestNode.transform;
    }
    
    private IEnumerator ResetShield()
    {
        _rechargingShield = true;
        yield return new WaitForSeconds(shieldCooldown);
        shieldIsActive = true;
        shieldGameObject.SetActive(true);
        currentShieldHealth = maxShieldHealth;
        _rechargingShield = false;
    }

    public void SwichTargetToPlayer()
    {
        attackPlayer = true;
        _fsmNavMeshAgent.target = GameManager.Instance.player.transform;
    }
    
    public virtual void GotHit(int damage, PlayerAttacks attackType)
    {
        if (!attackPlayer)
        {
            attackPlayer = true;
            _fsmNavMeshAgent.target = GameManager.Instance.player.transform;

            foreach (var enemySpawn in GameManager.Instance.currentArena.enemiesSpawners)
            {
                enemySpawn.enemies ??= new List<GameObject>();

                foreach (var enemy in enemySpawn.enemies.ToArray())
                {
                    var enemyHittableScript = enemy.GetComponent<Hittable>();
                    if (Vector3.Distance(transform.position, enemy.transform.position) <= alertDistance && !enemyHittableScript.attackPlayer)
                    {
                        enemyHittableScript.SwichTargetToPlayer();
                    }
                    
                }
            }
        }
        
        if (onlyDamagableByExplosion && attackType != PlayerAttacks.Explositon)
        {
            damage = 0;
        }
        
        if (hasShield && shieldIsActive && attackType != PlayerAttacks.BulletEnhanced)
        {
            damage = 0;
        }
        

        if (hasShield && shieldIsActive)
        {
            currentShieldHealth -= damage;
        }
        else if (!hasShield || (hasShield && !shieldIsActive))
        {
            currentHealth -= damage;
        }

            
        if ( hasShield && currentShieldHealth <= 0 && !_rechargingShield)
        {
            Debug.Log("deativate shield");
            shieldIsActive = false;
            shieldGameObject.SetActive(false);
            _rechargeShield = StartCoroutine(ResetShield());
        }
        
        
        foreach (var textMesh in _damageTextMesh)
        {
            textMesh.text = damage.ToString();
        }

        foreach (var reduce in _reduceSizes)
        {
            reduce.duration = textDuration;
        }
        
        var go =Instantiate(damageText, textPosition.position, Quaternion.identity, transform);
        go.transform.localScale *= textSize;
        
        if (enemyType == Enemy.Phage)
        {
            GameEvents.Instance.bossTookDamage.Ping(this,null);
        }
        
        if (currentHealth <= 0)
        {
            Death(attackType);
        }

        if (animate && !isShaking)
        {
            isShaking = true;
            _shaking = transform.DOShakeScale(duration, strength, 0, 0, fadeOut);
            _shaking.OnComplete(() =>
            {
                isShaking = false;
            });
            
        }
        
    }

    public float GetCurrentHealthPercentage()
    {
        return (float)Math.Round((float)currentHealth / maxHealth, 2);
        //return 900 / 1000;
    }
    
    public float GetLostHealthPercentage()
    {
        return (float)Math.Round((decimal)((maxHealth - currentHealth)*100 / maxHealth), 2);
    }
    
    private void Death(PlayerAttacks attackType)
    {
        var DropAmmoChange = 0.05f;
        if (GameManager.Instance.player.GetComponent<PlayerController>().CurrentWeapon.name == "Shotgun") { DropAmmoChange = 0.025f; }
        if (hasShield) { Instantiate(_specialAmmo, transform.position, Quaternion.identity); }
        
        if (attackType == PlayerAttacks.Knife) { Instantiate(_ammo, transform.position, Quaternion.identity); }
        else if (Random.Range(0f, 1f) < DropAmmoChange) { Instantiate(_ammo, transform.position, Quaternion.identity); }
        
        if (isItEnemy) { _enemyDied.Ping(this, null); }
        
        
        if(doSound) SoundManager.Instance.PlaySound(clip,volume);
        Destroy(gameObject);
    }

    
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using DG.Tweening;
using Enums;
using UnityEngine;
using Random = UnityEngine.Random;

public class Hitable : MonoBehaviour
{
    [Header("Events"), Space(5)] 
    private GameEvent enemyDied;
    
    [Header("Enemy Stats"), Space(5)] 
    public Enemy enemyType;
    public bool isItEnemy;
    public bool canHaveShield;
    
    [Header("Health Stats"), Space(5)] 
    public int maxHealth;
    [field: HideInInspector] public int currentHealth;
        
    [Header("Shield Stats"), Space(5)] 
    public int maxShieldHealth;
    [field: HideInInspector] public int currentShieldHealth;
    [field: HideInInspector] public bool hasShield;
    [field: HideInInspector] public bool shieldIsActive;
    [field: SerializeField] private float shieldCooldown;
    
    
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
    private TextMesh[] _damageTextMesh;
    private ReduceSize[] _reduceSizes;


    // Death Drops
    private GameObject _ammo;
    private GameObject _specialAmmo;
        
        
    [Header("Death Sound"), Space(5)] 
    [field: SerializeField] private bool doSound;
    [field: SerializeField] private AudioClip clip;
    [field: SerializeField] private float volume;
    
    
    private Tween _shaking;
    private void Start()
    {
        currentHealth = maxHealth;
        _damageTextMesh = damageText.GetComponentsInChildren<TextMesh>();
        _reduceSizes = damageText.GetComponentsInChildren<ReduceSize>();

        _ammo = GameManager.Instance.ammo;
        _specialAmmo = GameManager.Instance.specialAmmo;

        enemyDied = GameEvents.Instance.enemyDied;
    }

    private void OnDestroy()
    {
        if (_shaking != null)
        {
            _shaking.Kill();
        }
    }

    public void GotHit(int damage, PlayerAttacks attackType)
    {
        //Debug.Log("the attack type: " + attackType);
        
        //todo add the distinction between bullets / enhanced bullets / knife
        currentHealth -= damage;
        
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
        
        if (currentHealth <= 0)
        {
            Death(attackType);
        }

        if (animate)
        {
            _shaking = transform.DOShakeScale(duration, strength, 0, 0, fadeOut);
        }
        
    }

    public float GetCurrentHealthPercentage()
    {
        return (float)Math.Round((decimal)(currentHealth / maxHealth), 2);
    }
    
    public float GetLostHealthPercentage()
    {
        return (float)Math.Round((decimal)((maxHealth - currentHealth)*100 / maxHealth), 2);
    }
    
    private void Death(PlayerAttacks attackType)
    {
        if (Random.Range(0f, 1f) < 0.05f) { Instantiate(_ammo, transform.position, Quaternion.identity); }
        if (hasShield) { Instantiate(_specialAmmo, transform.position, Quaternion.identity); }
        if (attackType == PlayerAttacks.Knife) { Instantiate(_ammo, transform.position, Quaternion.identity); }
        if (isItEnemy) { enemyDied.Ping(this, null); }
        
        
        if(doSound) SoundManager.Instance.PlaySound(clip,volume);
        Destroy(gameObject);
    }
}

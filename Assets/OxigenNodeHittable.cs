using System;
using System.Collections;
using Enums;
using UnityEngine;

public enum OxygenNodeType
{
    Low,
    High
}

public class OxigenNodeHittable : Hittable
{
    [Header("Oxygen"), Space(5)] 
    [field: SerializeField] public OxygenNodeType oxygenNodeType;
    
    [field: SerializeField]private float minScale;
    [field: SerializeField] private float scaleSpeed;

    private Vector3 _initialPosition;
    private Vector3 _initialSize;
    private Vector3 _currentSize;
    
    public bool targetable;
    private bool _deflating;
    
    private void Start()
    {
        currentHealth = maxHealth;
        _initialPosition = transform.position;
        
        var localScale = transform.localScale;
        
        _initialSize = localScale;
        _currentSize = localScale;
        targetable = true;

        minScale = localScale.x * minScale;
        
        GameEvents.Instance.oxigenNodeIsTargatable.Ping(this,null);
    }
    
    public override void GotHit(int damage, PlayerAttacks attackType, Vector3 hitLocation = default)
    {
        if (!targetable || _deflating) return;
        currentHealth -= damage;    
        
        if (currentHealth <= 0)
        {
            targetable = false;
            GameEvents.Instance.oxigenNodeIsUntargatable.Ping(this,null);
        }
        
        _currentSize = _initialSize * GetCurrentHealthPercentage();
    }
    
    private void Update()
    {
        transform.position = _initialPosition;
        
        _deflating = false;
        
        var localScale = transform.localScale;
        
        
        if (!targetable && Vector3.Distance(transform.localScale ,_initialSize) < 0.01f )
        {
            _currentSize = transform.localScale;    
            currentHealth = maxHealth;
            targetable = true;
            GameEvents.Instance.oxigenNodeIsTargatable.Ping(this,null);
        }
        
        
        if (targetable && transform.localScale.x > _currentSize.x  && _currentSize.x > minScale)
        {
            localScale = transform.localScale - Vector3.one * (scaleSpeed * Time.deltaTime);
            _deflating = true;
        }
        
        if (!targetable && !_deflating)
        {
            localScale = transform.lossyScale + Vector3.one * (scaleSpeed * Time.deltaTime);
        }

        var value = Math.Clamp(localScale.x, minScale, _initialSize.x);
        transform.localScale = new Vector3(value, value, value);
    }
}

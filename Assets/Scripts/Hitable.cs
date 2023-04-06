using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Hitable : MonoBehaviour
{
    public int maxHealth;
    public int currentHealth;
    public GameObject damageText;
    private TextMesh _damageTextMesh;

    private Tween _shaking;

    [Header("Hit Animation"), Space(10)] 
    [field: SerializeField] private float duration;
    [field: SerializeField] private float strenght;
    [field: SerializeField] private float vibrato;
    [field: SerializeField] private float randomness;
    [field: SerializeField] private bool fadeOut;
    
    [Header("Damage Text Pop up"), Space(10)] 
    [field: SerializeField] private Transform textPosition;
    [field: SerializeField] private float textDuration;
    
    [Header("Death Sound"), Space(10)] 
    [field: SerializeField] private AudioClip clip;
    [field: SerializeField] private float volume;
    
    private void Start()
    {
        currentHealth = maxHealth;
        _damageTextMesh = damageText.GetComponent<TextMesh>();
    }

    public void GotHit(int damage)
    {
        currentHealth -= damage;
        _damageTextMesh.text = damage.ToString(); 
        
        var go =Instantiate(damageText, textPosition.position, Quaternion.identity, transform);
        Destroy(go,textDuration);
        
        if (currentHealth <= 0)
        {
            Death();
        }
        
        _shaking = transform.DOShakeScale(duration, strenght, 0, 0, fadeOut);
    }

    private void Death()
    {
        SoundManager.Instance.PlaySound(clip,volume);
        Destroy(gameObject);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using DG.Tweening;
using UnityEngine;

public class Hitable : MonoBehaviour
{
    public int maxHealth;
    public int currentHealth;
    public GameObject damageText;
    private TextMesh[] _damageTextMesh;
    private ReduceSize[] reduceSizes;

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
    [field: SerializeField] private float textSize = 1;
    
    [Header("Death Sound"), Space(10)] 
    [field: SerializeField] private AudioClip clip;
    [field: SerializeField] private float volume;
    
    private void Start()
    {
        currentHealth = maxHealth;
        _damageTextMesh = damageText.GetComponentsInChildren<TextMesh>();
        reduceSizes = damageText.GetComponentsInChildren<ReduceSize>();
        //Debug.Log("_damageTextMesh count: " + _damageTextMesh.Length);
    }

    public void GotHit(int damage)
    {
        currentHealth -= damage;
        foreach (var textMesh in _damageTextMesh)
        {
            textMesh.text = damage.ToString();
        }

        foreach (var reduce in reduceSizes)
        {
            reduce.duration = textDuration;
        }
        
        var go =Instantiate(damageText, textPosition.position, Quaternion.identity, transform);
        go.transform.localScale *= textSize;
        
        if (currentHealth <= 0)
        {
            Death();
        }
        
        _shaking = transform.DOShakeScale(duration, strenght, 0, 0, fadeOut);
    }

    public float GetCurrentHealthPercentage()
    {
        return (float)Math.Round((decimal)(currentHealth / maxHealth), 2);
    }
    
    public float GetLostHealthPercentage()
    {
        return (float)Math.Round((decimal)((maxHealth - currentHealth)*100 / maxHealth), 2);
    }
    
    private void Death()
    {
        SoundManager.Instance.PlaySound(clip,volume);
        Destroy(gameObject);
    }
}

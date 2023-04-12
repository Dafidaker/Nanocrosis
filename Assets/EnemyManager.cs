using System;
using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;
    [field: SerializeField] private float spawnRange;
    [field: SerializeField] private EnemySpawn[] enemySpawns;
    [field: HideInInspector] public float time;
    
    private 
    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        
    }

    
    void Update()
    {
        time += Time.deltaTime;
    }
    
}
[System.Serializable]
public class EnemySpawn
{
    public Enemy enemy;
    public GameObject prefab;
    public AnimationCurve maxAmountHeart;
    public AnimationCurve maxAmountLungs;
    public float minDistanceToPlayer;
    public float maxDistanceToPlayer;
    public float increaseTimer;
    private int lungsAmountEnemy;
    public int HeartAmountEnemy;
    
}

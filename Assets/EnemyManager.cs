using System;
using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;
    [field: SerializeField] private float spawnRange;
    [field: SerializeField] private EnemySpawn_Old[] enemySpawns;
    [field: SerializeField] public ArenaEnemySpawner[] enemySpawnsNew;
    [field: SerializeField] public float spawnAngle;
    [field: SerializeField] public int amountSpawnPositions;
    [field: SerializeField] private LayerMask spawnOn;
    
    private 
    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        
    }


    private void Update()
    {
        //goes through all the enemy spawners of the current arena 
        foreach (var enemySpawn in GameManager.Instance.currentArena.enemiesSpawners)
        {
            //decreases the timers for the the spawners 
            if (enemySpawn.enemySpawnTimer >= 0)
                enemySpawn.enemySpawnTimer -= Time.deltaTime;
                
            //if the timer is smaller than 0 than 
            if (enemySpawn.enemySpawnTimer < 0)
            {
                //it checks if it can spawn the enemy
                if (CanSpawnEnemy(enemySpawn))
                {
                    //and if so it tries to spawn the enemys
                    SpawnEnemy(enemySpawn);
                }

            }
        }
        
    }

    private bool CanSpawnEnemy(ArenaEnemySpawner enemySpawnOld)
    {
        var amountEnemies = enemySpawnOld.amountEnemies;
        var maxAmountEnemies = 0;
        
        var currentCurve = enemySpawnOld.maxAmount;
        
        if (currentCurve != null) 
            maxAmountEnemies = (int)currentCurve.Evaluate(GameManager.Instance.time);

        return amountEnemies < maxAmountEnemies;
    }

    private void SpawnEnemy(ArenaEnemySpawner enemySpawn)
    {
        var playerTransform = GameManager.Instance.player.transform; 
        
        var spawnAmount = 1;
        
        var spawnRange = Random.Range(enemySpawn.minDistanceToPlayer, enemySpawn.maxDistanceToPlayer);
        List<Vector3> spawnOrigins = new List<Vector3>();

        float angleBetweenSpawns = spawnAngle / amountSpawnPositions;
        Vector3 vector = playerTransform.forward * spawnRange + Vector3.up * 1.5f;
        
        spawnOrigins.Add(playerTransform.position + vector);

        for (int i = 1; i <= amountSpawnPositions- 1; i++)
        {
            vector = Quaternion.Euler(0f, angleBetweenSpawns * i, 0f) * vector;
            spawnOrigins.Add(playerTransform.position + vector);
        }

        List<RaycastHit> raycastHits = new List<RaycastHit>();
        
        foreach (var vector3 in spawnOrigins)
        {
            Debug.DrawRay(vector3,Vector3.down, Color.white,100f);
           Physics.Raycast(vector3, Vector3.down, out var hit, spawnOn);
           raycastHits.Add(hit);
        }

        if (raycastHits.Count == 0)
        {
            Debug.Log("no possible position to spawn the Enemy");
            return;
        }
        
        var arenaEnemySpawn = GameManager.Instance.currentArena.enemiesParent;
        
        for (int i = 0; i < spawnAmount; i++)
        {
            var enemySpawnPosition = raycastHits[Random.Range(0, raycastHits.Count)].point;

            Instantiate(enemySpawn.prefab, enemySpawnPosition, Quaternion.identity, arenaEnemySpawn.transform);

            enemySpawn.enemySpawnTimer += enemySpawn.increaseTimer;
        }
        
        enemySpawn.amountEnemies += spawnAmount;
        
        
    }
    
}
[System.Serializable]
public class EnemySpawn_Old
{
    public Enemy enemy;
    public GameObject prefab;
    public AnimationCurve maxAmountHeart;
    public AnimationCurve maxAmountLungs;
    public float minDistanceToPlayer;
    public float maxDistanceToPlayer;
    public float enemySpawnTimer;
    public float increaseTimer;
    public (Arena,int)[] AmountEnemyPerArena;
    
}

/*[System.Serializable]
public class EnemySpawn
{
    public Enemy enemy;
    public GameObject prefab;
    public List<ArenaEnemySpawner> arenaEnemySpawners;

}*/

[System.Serializable]
public class ArenaEnemySpawner 
{
    public Enemy enemy;
    public GameObject prefab;
    public Arena arena;
    public AnimationCurve maxAmount;
    public float minDistanceToPlayer;
    public float maxDistanceToPlayer;
    [HideInInspector] public float enemySpawnTimer;
    public float increaseTimer;
    [HideInInspector] public int amountEnemies;
    
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enums;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    public GameObject debugGameObject;
    
    [field: SerializeField] private float spawnRange;
    [field: SerializeField] public ArenaEnemySpawner[] enemySpawns;
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
        SpawnEnemies();
    }

    public void UpdateEnemyList(Component sender, object data)
    {
        if (sender is Hitable)
        {
            Debug.Log("killed a enemy: " + sender.GetComponent<Hitable>().enemyType);
        }
        else
        {
            Debug.Log("killed a enemy");
        }
        
        
        foreach (var enemySpawn in GameManager.Instance.currentArena.enemiesSpawners)
        {
            enemySpawn.enemies ??= new List<GameObject>();

            foreach (var gameObject in enemySpawn.enemies.ToArray())
            {
                if (gameObject == null)
                {
                    enemySpawn.enemies.Remove(gameObject);
                }
            }
        }
        
    }
    private void SpawnEnemies()
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

        var amountEnemies = enemySpawnOld.enemies.Count;
        var maxAmountEnemies = 0;
        
        var currentCurve = enemySpawnOld.maxAmount;

        var minutes = GameManager.Instance.seconds / 60; 
        
        //if the seconds is higher than the curves max seconds it uses the max seconds of the curve
        if (minutes > currentCurve[currentCurve.length-1].time)
        {
            maxAmountEnemies = (int)currentCurve.Evaluate(currentCurve[currentCurve.length - 1].time);
        }
        else
        {
            maxAmountEnemies = (int)currentCurve.Evaluate(GameManager.Instance.seconds / 60);
        }
        
        return amountEnemies < maxAmountEnemies;
    }

    private void SpawnEnemy(ArenaEnemySpawner enemySpawn)
    {
        var playerTransform = GameManager.Instance.player.transform;
        var camTrans = GameManager.Instance.cinemachineVirtualCamera.transform.forward;
        var spawnAmount = 1;
        
        var spawnRange = Random.Range(enemySpawn.minDistanceToPlayer, enemySpawn.maxDistanceToPlayer);
        List<Vector3> spawnOrigins = new List<Vector3>();

        float angleBetweenSpawns = spawnAngle / amountSpawnPositions;
        Vector3 vector = playerTransform.forward * spawnRange + Vector3.up * 1.5f;
        vector = Quaternion.Euler(0f, -spawnAngle/2, 0f) * vector;
        
        //Debug.DrawRay(playerTransform.position, vector, Color.yellow , 100f);
        
        
        spawnOrigins.Add(playerTransform.position + vector);

        for (int i = 1; i <= amountSpawnPositions- 1; i++)
        {
            vector = Quaternion.Euler(0f, angleBetweenSpawns, 0f) * vector;
            spawnOrigins.Add(playerTransform.position + vector);
            //Debug.DrawRay(playerTransform.position, vector, Color.red , 100f);
        }

        List<RaycastHit> raycastHits = new List<RaycastHit>();
        
        foreach (var vector3 in spawnOrigins)
        { 
            //Debug.DrawRay(vector3,Vector3.down, Color.white,100f);
           Instantiate(debugGameObject, vector3, Quaternion.identity);
           Physics.Raycast(vector3, Vector3.down, out var hit, spawnOn);
           
           if (hit.point.y <= vector3.y)
           {
               raycastHits.Add(hit);
           }
        }

        if (raycastHits.Count == 0)
        {
            Debug.Log("no possible position to spawn the Enemy");
            return;
        }
        
        var arenaEnemySpawn = GameManager.Instance.currentArena.enemiesParent;
        
        for (int i = 0; i < spawnAmount; i++)
        {
            //Debug.Log("Enemy Created: " + enemySpawn.enemy);
            var enemySpawnPosition = raycastHits[Random.Range(0, raycastHits.Count)].point;

            //Instantiate(debugGameObject, enemySpawnPosition, Quaternion.identity);
            
            var go = Instantiate(enemySpawn.prefab, enemySpawnPosition, Quaternion.identity, arenaEnemySpawn.transform);
            
            enemySpawn.enemies.Add(go);
            enemySpawn.enemySpawnTimer += enemySpawn.increaseTimer;
        }
        
        enemySpawn.amountEnemies = enemySpawn.enemies.Count;
        
    }
    
    private void OnGUI()
    {
        var a = GameManager.Instance.currentArena.enemiesSpawners;
        
        GUI.Box(new Rect(Screen.height * 0.7f, 0, Screen.width * 0.2f, Screen.height * 0.05f), "Time:" +
            (GameManager.Instance.seconds / 60));
        
        /*GUI.Box(new Rect(0, Screen.height * 0.1f, Screen.width * 0.2f, Screen.height * 0.05f), "Enemy:" + a[0].enemy);
        GUI.Box(new Rect(0, Screen.height * 0.15f, Screen.width * 0.2f, Screen.height * 0.05f),"Amount: " + a[0].amountEnemies);
        GUI.Box(new Rect(0, Screen.height * 0.2f, Screen.width * 0.2f, Screen.height * 0.05f), "MaxAmount: " + a[0].maxAmount.Evaluate(GameManager.Instance.seconds /60 ));
        GUI.Box(new Rect(0, Screen.height * 0.25f, Screen.width * 0.2f, Screen.height * 0.05f), "Timer: " + a[0].enemySpawnTimer);*/
        
        GUI.Box(new Rect(0, Screen.height * 0.1f, Screen.width * 0.2f, Screen.height * 0.05f), "Lungs State: " + ObjectiveManager.Instance.GetPercentageOfCurrentValue()*100);
        
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
    public List<GameObject> enemies;
    
}

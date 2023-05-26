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
    [field: SerializeField] public ArenaItemSpawner[] itemSpawns;
    [field: SerializeField] public float spawnAngle;
    [field: SerializeField] public int amountSpawnPositions;
    [field: SerializeField] private LayerMask spawnOn;
    
    private void Awake()
    {
        Instance = this;
    }
    
    private void Update()
    {
        if (GameManager.Instance.gamePaused || GameManager.Instance.isGameOver) { return; }
        
        if (GameManager.Instance.spawnEnemies)
        {
            SpawnEnemies();
        }
        
        SpawnItems();
    }

    public void UpdateEnemyList(Component sender, object data)
    {
        if (sender is not Hittable) return;
        
        var hittable = sender.GetComponent<Hittable>();
        if(hittable.enemyType == Enemy.Phage)return;
        var enemies = GameManager.Instance.GetArenaEnemySpawner(GameManager.Instance.currentArena.enemiesSpawners, hittable.enemyType).enemies;
            
        if (!enemies.Contains(hittable.gameObject)) return;
            
        foreach (var enemy in enemies.Where(enemy => enemy == hittable.gameObject))
        {
            enemies.Remove(enemy);
            return; 
        }

        /*foreach (var enemySpawn in GameManager.Instance.currentArena.enemiesSpawners)
        {
            enemySpawn.enemies ??= new List<GameObject>();
            
            foreach (var gameObject in enemySpawn.enemies.ToArray())
            {
                if (gameObject == null)
                {
                    enemySpawn.enemies.Remove(gameObject);
                }
            }
        }*/

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
        var spawnAmount = 1;
        
        var spawnRange = Random.Range(enemySpawn.minDistanceToPlayer, enemySpawn.maxDistanceToPlayer);
        List<Vector3> spawnOrigins = new List<Vector3>();

        float angleBetweenSpawns = spawnAngle / amountSpawnPositions;
        Vector3 vector = playerTransform.forward * spawnRange + Vector3.up * 1.5f;
        vector = Quaternion.Euler(0f, -spawnAngle/2, 0f) * vector;
        
        
        spawnOrigins.Add(playerTransform.position + vector);

        for (int i = 1; i <= amountSpawnPositions- 1; i++)
        {
            vector = Quaternion.Euler(0f, angleBetweenSpawns, 0f) * vector;
            spawnOrigins.Add(playerTransform.position + vector);
        }

        List<RaycastHit> raycastHits = new List<RaycastHit>();
        
        foreach (var vector3 in spawnOrigins)
        { 
           Physics.Raycast(vector3, Vector3.down, out var hit, spawnOn);
           
           if (hit.point.y <= vector3.y)
           {
               raycastHits.Add(hit);
           }
        }

        if (raycastHits.Count == 0)
        {
            return;
        }
        
        var arenaEnemySpawn = GameManager.Instance.currentArena.enemiesParent;
        
        for (int i = 0; i < spawnAmount; i++)
        {
            var enemySpawnPosition = raycastHits[Random.Range(0, raycastHits.Count)].point;
            
            var go = Instantiate(enemySpawn.prefab, enemySpawnPosition, Quaternion.identity, arenaEnemySpawn.transform);
            go.GetComponent<Hittable>().location = GameManager.Instance.currentArena.arenaType;
            
            enemySpawn.enemies.Add(go);
            enemySpawn.enemySpawnTimer += enemySpawn.increaseTimer;
        }
        
        enemySpawn.amountEnemies = enemySpawn.enemies.Count;
        
    }
    
    private void SpawnItems()
    {
        //goes through all the item spawners of the current arena 
        foreach (var itemsSpawner in GameManager.Instance.currentArena.itemsSpawners)
        {
            //decreases the timers for the the spawners 
            if (itemsSpawner.itemSpawnTimer >= 0)
                itemsSpawner.itemSpawnTimer -= Time.deltaTime;
            
            
            //if the timer is smaller than 0 than 
            if (itemsSpawner.itemSpawnTimer < 0 && MaxAmountItems(itemsSpawner) > itemsSpawner.Items.Count)
            {
                //it checks if it can spawn the item
                if (CanSpawnItem(itemsSpawner))
                {
                    //and if so it tries to spawn the item
                    SpawnItem(itemsSpawner);
                }

            }
        }
    }
    private bool CanSpawnItem(ArenaItemSpawner itemSpawner)
    {
        var amountEnemies = itemSpawner.Items.Count;
        int maxAmountEnemies;
        
        var currentCurve = itemSpawner.maxAmount;

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

    public int MaxAmountItems(ArenaItemSpawner itemSpawner)
    {
        int maxAmountEnemies;
        
        var currentCurve = itemSpawner.maxAmount;

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

        return maxAmountEnemies;
    }
    private void SpawnItem(ArenaItemSpawner itemSpawner)
    {
        var tempTree = new List<GameObject>();
        tempTree.AddRange(GameManager.Instance.currentArena.itemTrees);
        

        var selectedTree = tempTree[Random.Range(0, tempTree.Count)];
        var selectedTreeController = selectedTree.GetComponent<ItemTreeController>();
        var itemWaypoint = selectedTreeController.GetEmptyItemBranchWaypoint(); //var itemWaypoint = selectedTreeController.GetEmptyItemWaypoint();
        
        while (itemWaypoint == null && tempTree.Count > 0)
        {
            tempTree.Remove(selectedTree);
            if(tempTree.Count < 0) return;
            Debug.Log("tree" + tempTree.Count);
            selectedTree = tempTree[Random.Range(0, tempTree.Count)];
            selectedTreeController = selectedTree.GetComponent<ItemTreeController>();
            itemWaypoint = selectedTreeController.GetEmptyItemBranchWaypoint(); //itemWaypoint = selectedTreeController.GetEmptyItemWaypoint();
        }

        if (tempTree.Count <= 0)
        {
            Debug.Log("no item spawn avaliable");
            return;
        }

        if (itemWaypoint != null)
        {
            itemSpawner.itemSpawnTimer += itemSpawner.CoolDown; 
            selectedTreeController.AddItemToBranch(itemSpawner.ItemType, itemSpawner.ItemPrefab); //selectedTreeController.AddItem((ItemSpawn)itemWaypoint, itemSpawner.ItemType, itemSpawner.ItemPrefab);
        }
            
    }
    
    private void OnGUI()
    {
        
        
        /*GUI.Box(new Rect(Screen.width * 0.5f - Screen.width * 0.05f, 0, Screen.width * 0.1f, Screen.height * 0.05f), "Time:" +
            TimeSpan.FromSeconds(GameManager.Instance.seconds).ToString(@"mm\:ss"));
        
        GUI.Box(new Rect(0, Screen.height * 0.1f, Screen.width * 0.2f, Screen.height * 0.05f), "Enemy:" + a[0].enemy);
        GUI.Box(new Rect(0, Screen.height * 0.15f, Screen.width * 0.2f, Screen.height * 0.05f),"Amount: " + a[0].amountEnemies);
        GUI.Box(new Rect(0, Screen.height * 0.2f, Screen.width * 0.2f, Screen.height * 0.05f), "MaxAmount: " + a[0].maxAmount.Evaluate(GameManager.Instance.seconds /60 ));
        GUI.Box(new Rect(0, Screen.height * 0.25f, Screen.width * 0.2f, Screen.height * 0.05f), "Timer: " + a[0].enemySpawnTimer);
        
        GUI.Box(new Rect(0, Screen.height * 0.1f, Screen.width * 0.12f, Screen.height * 0.05f), "Lungs State: " +
            Math.Round(ObjectiveManager.Instance.GetPercentageOfCurrentValue() * 100, 2));
        GUI.Box(new Rect(0, Screen.height * 0.2f, Screen.width * 0.12f, Screen.height * 0.05f), "Phage Health: " +
            GameManager.Instance.phage.GetCurrentHealthPercentage()*100 + "%");*/
        
    }
}

[Serializable]
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

[Serializable]
public class ArenaItemSpawner
{
    public ItemType ItemType;
    public GameObject ItemPrefab;
    public Arena arena;
    public AnimationCurve maxAmount;
    [HideInInspector] public float itemSpawnTimer;
    public float CoolDown;
    [HideInInspector]public List<GameObject> Items;
    
}

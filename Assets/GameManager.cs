using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Enums;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[Serializable]
public class Nodes
{
    public List<OxigenNodeHittable> hittableHighNodes;
    public List<OxigenNodeHittable> unhittableHighNodes;
    
    public List<OxigenNodeHittable> hittableLowNodes;
    public List<OxigenNodeHittable> unhittableLowNodes;
}
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool isGameOver;
    public bool gamePaused;
    public bool isInvincible;
    public float teleportActivationTime;
    public bool canTeleport;

    [Header("Prefab"), Space(5)] 
    public GameObject debugObject;
    public GameObject player;
    public GameObject ammo;
    public GameObject specialAmmo;
    public GameObject bomb;
    public GameObject vfxHitEnemy;
    public GameObject vfxHitAnything;
    [field: HideInInspector] public PlayerController playerController;
    
    [Header("Arenas"), Space(5)]
    public ArenaClass[] arenas;
    [field: SerializeField] private Arena initialArena;
    [field: HideInInspector] public ArenaClass currentArena;
    [field: SerializeField] public bool spawnEnemies;
    [field: SerializeField] public Hittable phage;
    
    [Header("Transforms"), Space(5)]
    public Transform lungsPlayerSpawnPosition;
    public Transform heartPlayerSpawnPosition;
    
    [Header("Camera"), Space(5)] 
    public CinemachineVirtualCamera cinemachineVirtual;
    private float _camXSpeed;
    private float _camYSpeed;
    
    
    [Header("Nodes"), Space(5)] 
    //NODES

    public List<OxigenNodeHittable> hittableHighNodes;
    public List<OxigenNodeHittable> unhittableHighNodes;
    
    public List<OxigenNodeHittable> hittableLowNodes;
    public List<OxigenNodeHittable> unhittableLowNodes;
    public float seconds;

    [Header("Tree"), Space(5)]
    [field: SerializeField] private List<Transform> treeSpawns;
    [field: SerializeField] private int itemTrees;
    [field: SerializeField] private int oxygenTrees;
    [field: SerializeField] private int oxygenBushes;
    [field: SerializeField] private GameObject oxygenTreePrefab;
    [field: SerializeField] private GameObject oxygenBushPrefab;
    [field: SerializeField] private GameObject itemTreePrefab;
    [field: SerializeField] public List<GameObject> oxygenTreeGameObjects;
    [field: SerializeField] public List<GameObject> oxygenBushesGameObjects;
    [field: SerializeField] public List<GameObject> itemTreeGameObjects;
    
    private void Awake()
    {
        Instance = this;
        playerController = player.GetComponent<PlayerController>();
        
        player.transform.position = initialArena switch
        {
            Arena.Heart => heartPlayerSpawnPosition.position,
            Arena.Lungs => lungsPlayerSpawnPosition.position,
            _ => lungsPlayerSpawnPosition.position
        };

        oxygenTreeGameObjects = new List<GameObject>();
        oxygenBushesGameObjects = new List<GameObject>();
        itemTreeGameObjects = new List<GameObject>();
        
        foreach (var arenaClass in arenas)
        {
            arenaClass.enemies = new List<GameObject>();
            foreach (Transform child in arenaClass.enemiesParent.transform)
            {
                arenaClass.enemies.Add(child.gameObject);
            }
            
        }
        
        ChangeArena(initialArena);
        
        AudioManager.Instance.PlayMusic("Gameplay");
    }

    private void Start()
    {
        foreach (var enemySpawn in EnemyManager.Instance.enemySpawns)
        {
            GetArena(enemySpawn.arena).enemiesSpawners.Add(enemySpawn);
        }

        foreach (var itemSpawn in EnemyManager.Instance.itemSpawns)
        {
            GetArena(itemSpawn.arena).itemsSpawners.Add(itemSpawn);
        }

        
        hittableHighNodes = new List<OxigenNodeHittable>();
        unhittableHighNodes = new List<OxigenNodeHittable>();
    
        hittableLowNodes = new List<OxigenNodeHittable>();
        unhittableLowNodes = new List<OxigenNodeHittable>();
        
        _camXSpeed = cinemachineVirtual.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_MaxSpeed;
        _camYSpeed = cinemachineVirtual.GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_MaxSpeed;
        
        //PlaceTrees();

        foreach (var arena in arenas)
        {
            PlaceTrees(arena);
        }
        
        SettingsMenu.Instance.UpdateAllSettings();
    }

    void Update()
    {
        var transform1 = cinemachineVirtual.transform;
        Debug.DrawRay(transform1.position, transform1.forward * 10f , Color.green);
        
        
        seconds += Time.deltaTime;

        if (!(seconds > teleportActivationTime) || canTeleport) return;
        canTeleport = true;
        DoctorManager.Instance.AddToQueue("Teleport");
    }


    private void PlaceTrees(ArenaClass arena)
    {
        int amoutItemTrees = arena.amountItemTrees;
        int amountOxygenTree = arena.amountOxygenTrees; 
        int amountOxygenBushes = arena.amountOxygenBushes;
        
        var treeSpawnPositions = new List<Transform>();
        treeSpawnPositions.AddRange(arena.treeSpawnWaypoints); //treeSpawns

        if (amoutItemTrees > treeSpawnPositions.Count) { amoutItemTrees = treeSpawnPositions.Count; } // itemTrees => amoutItemTrees
        
        for (var i = 0; i < amoutItemTrees; i++)
        {
            var index = Random.Range(0, treeSpawnPositions.Count);

            var go = Instantiate(itemTreePrefab, treeSpawnPositions[index]);
            
            itemTreeGameObjects.Add(go);

            treeSpawnPositions.Remove(treeSpawnPositions[index]);
        }
        
        if (amountOxygenTree > treeSpawnPositions.Count) { amountOxygenTree = treeSpawnPositions.Count; } // oxygenTrees  => amountOxygenTree
        
        for (var i = 0; i < amountOxygenTree; i++)
        {
            var index = Random.Range(0, treeSpawnPositions.Count);

            var go = Instantiate(oxygenTreePrefab, treeSpawnPositions[index]);
            
            oxygenTreeGameObjects.Add(go);
            
            treeSpawnPositions.Remove(treeSpawnPositions[index]);
        }
        
        if (amountOxygenBushes > treeSpawnPositions.Count) { amountOxygenBushes = treeSpawnPositions.Count; } // oxygenBushes =>  amountOxygenBushes

        for (var i = 0; i < amountOxygenBushes; i++)
        {
            var index = Random.Range(0, treeSpawnPositions.Count);

            var go = Instantiate(oxygenBushPrefab, treeSpawnPositions[index]);
            
            oxygenBushesGameObjects.Add(go); // todo maybe create new list for bushes if needed 
            
            treeSpawnPositions.Remove(treeSpawnPositions[index]);
        }

        /*var lungs = GetArena(Arena.Lungs);
        lungs.itemTrees.Clear();
        lungs.itemTrees.AddRange(itemTreeGameObjects);
        //lungs.oxigenTrees.Clear();
        lungs.oxygenTrees.AddRange(oxygenTreeGameObjects);
        //lungs.oxigenBushes.Clear();
        lungs.oxygenBushes.AddRange(oxygenBushesGameObjects);*/
        
        arena.itemTrees.AddRange(itemTreeGameObjects);
        arena.oxygenTrees.AddRange(oxygenTreeGameObjects);
        arena.oxygenBushes.AddRange(oxygenBushesGameObjects);
    }
    public void ChangeArena(Arena newArena)
    {
        //if there is a current arena it disables those enemies
        if (currentArena.enemiesParent != null)
        {
            foreach (Transform enemy in currentArena.enemiesParent.transform)
            {
                if (enemy == null)
                {
                    currentArena.enemies.Remove(enemy.gameObject);
                    continue;
                }
                enemy.gameObject.SetActive(false);
            }
            
        }
        
        //gets the arena and it makes the enemies on that arena active
        currentArena = GetArena(newArena);
        
        foreach (Transform enemy in currentArena.enemiesParent.transform)
        {
            if (enemy == null)
            {
                currentArena.enemies.Remove(enemy.gameObject);
                continue;
            }
            enemy.gameObject.SetActive(true);
        }
    }
    
    public ArenaClass GetArena(Arena arenaType)
    {
        return arenas.FirstOrDefault(arenaClass => arenaClass.arenaType == arenaType);
    }

    public void GameEnded(bool win)
    {
        isGameOver = true;
        Time.timeScale = 0;

        if (win)
        {
            PauseMenuController.Instance.WonGame();
        }
        else
        {
            PauseMenuController.Instance.LostGame();
        }
        
        var msm = win ? "you won" : "you lost";
        Debug.Log(msm);
    }

    public ArenaEnemySpawner GetArenaEnemySpawner(List<ArenaEnemySpawner> arenaType,Enemy enemyType )
    {
        return arenaType.FirstOrDefault(enemySpawner => enemySpawner.enemy == enemyType);
    }

    public void UpdateMouseSentivity(Component sender, object data)
    {
        if (data is not Vector2) return;
        Vector2 mouseSentivity = (Vector2)data;
        cinemachineVirtual.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_MaxSpeed = _camXSpeed * mouseSentivity.x;
        cinemachineVirtual.GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_MaxSpeed = _camYSpeed * mouseSentivity.y;
    }
    
    public void BecomeTargetableOxygenNode(Component sender, object data)
    {
        if (sender is not OxigenNodeHittable) return;

        var script = sender.GetComponent<OxigenNodeHittable>();

        switch (script.oxygenNodeType)
        {
            case OxygenNodeType.High:
                unhittableHighNodes.Remove(script);
                hittableHighNodes.Add(script);
                break;
            case OxygenNodeType.Low:
                unhittableLowNodes.Remove(script);
                hittableLowNodes.Add(script);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        ObjectiveManager.Instance.ChangeValue(ObjectiveManager.Instance.nodeHealingValue);
        //ObjectiveManager.Instance.currentValue += ObjectiveManager.Instance.nodeHealingValue;
    }
    
    public void BecomeUntargetableOxygenNode(Component sender, object data)
    {
        if (sender is not OxigenNodeHittable) return;

        var script = sender.GetComponent<OxigenNodeHittable>();

        switch (script.oxygenNodeType)
        {
            case OxygenNodeType.High:
                unhittableHighNodes.Add(script);
                hittableHighNodes.Remove(script);
                break;
            case OxygenNodeType.Low:
                unhittableLowNodes.Add(script);
                hittableLowNodes.Remove(script);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        ObjectiveManager.Instance.ChangeValue(-ObjectiveManager.Instance.nodeDamagingValue);
        //ObjectiveManager.Instance.currentValue -= ObjectiveManager.Instance.nodeDamagingValue;
    }
}

[Serializable]
public class ArenaClass
{
    public Arena arenaType;
    public GameObject enemiesParent;
    public Transform[] waypoints;
    public Transform[] treeSpawnWaypoints;
    public int amountItemTrees;
    public int amountOxygenTrees;
    public int amountOxygenBushes;
    public List<GameObject> itemTrees;
    [FormerlySerializedAs("oxigenTrees")] public List<GameObject> oxygenTrees;
    [FormerlySerializedAs("oxigenBushes")] public List<GameObject> oxygenBushes;
    [HideInInspector] public List<ArenaItemSpawner> itemsSpawners;
    [HideInInspector] public List<GameObject> enemies;
    [HideInInspector] public List<ArenaEnemySpawner> enemiesSpawners;


    public Vector3 GetClosestWaypoint(Vector3 point)
    {
        var closestWaypoint = waypoints[0];
        var closestDistance = float.MaxValue;

        foreach (var waypoint in waypoints)
        {
            var distance = Vector3.Distance(waypoint.position, point);
            
            if (!(distance < closestDistance)) continue;
            
            closestWaypoint = waypoint;
            closestDistance = distance;
        }

        return closestWaypoint.position;
    }
    public Vector3 GetFarthestWaypoint(Vector3 point)
    {
        var farthestWaypoint = waypoints[0];
        var farthestDistance = 0f;

        foreach (var waypoint in waypoints)
        {
            var distance = Vector3.Distance(waypoint.position, point);
            
            if (!(distance > farthestDistance)) continue;
            
            farthestWaypoint = waypoint;
            farthestDistance = distance;
        }

        return farthestWaypoint.position;
    }
}


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Enums;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool isGameOver;
    
    [Header("Prefab"), Space(5)]
    public GameObject player;
    public GameObject ammo;
    public GameObject specialAmmo;
    public GameObject bomb;
    [field: HideInInspector] public PlayerController playerController;
    
    [Header("Arenas"), Space(5)]
    public ArenaClass[] arenas;
    [field: SerializeField] private Arena initialArena;
    [field: HideInInspector] public ArenaClass currentArena;
    [field: SerializeField] public bool spawnEnemies;
    [field: SerializeField] public Hitable phage;
    
    [Header("Transforms"), Space(5)]
    public Transform lungsPlayerSpawnPosition;
    public Transform heartPlayerSpawnPosition;
    
    [Header("Camera"), Space(5)] 
    public CinemachineVirtualCamera cinemachineVirtual;
    private float _camXSpeed;
    private float _camYSpeed;
    
    [field: HideInInspector] public float seconds;

    public bool gamePaused;
    
    private void Awake()
    {
        Instance = this;
        playerController = player.GetComponent<PlayerController>();
        player.transform.position = lungsPlayerSpawnPosition.position;

        foreach (var arenaClass in arenas)
        {
            arenaClass.enemies = new List<GameObject>();
            foreach (Transform child in arenaClass.enemiesParent.transform)
            {
                arenaClass.enemies.Add(child.gameObject);
            }
            
        }
        
        ChangeArena(initialArena);
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
        
        _camXSpeed = cinemachineVirtual.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_MaxSpeed;
        _camYSpeed = cinemachineVirtual.GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_MaxSpeed;
        
        updateMouseSentivity(null, Settings.MouseSentivity);
    }

    void Update()
    {
        seconds += Time.deltaTime;
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
        Debug.Log("Old: " + currentArena.arenaType + " New: " + newArena);
        
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

    public void updateMouseSentivity(Component sender, object data)
    {
        if (data is not Vector2) return;
        Vector2 mouseSentivity = (Vector2)data;
        cinemachineVirtual.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_MaxSpeed = _camXSpeed * mouseSentivity.x;
        cinemachineVirtual.GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_MaxSpeed = _camYSpeed * mouseSentivity.y;
    }
}

[System.Serializable]
public class ArenaClass
{
    public Arena arenaType;
    public GameObject enemiesParent;
    public Transform[] waypoints;
    public List<GameObject> trees;
    [HideInInspector] public List<ArenaItemSpawner> itemsSpawners;
    [HideInInspector] public List<GameObject> enemies;
    [HideInInspector] public List<ArenaEnemySpawner> enemiesSpawners;
    
}


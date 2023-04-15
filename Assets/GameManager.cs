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
    
    [Header("Prefab"), Space(5)] 
    public CinemachineVirtualCamera cinemachineVirtualCamera;
    public GameObject player;
    public GameObject ammo;
    public GameObject specialAmmo;
    [field: HideInInspector] public PlayerController playerController;
    
    [Header("Arenas"), Space(5)]
    public ArenaClass[] arenas;
    [field: SerializeField] private Arena initialArena;
    [field: HideInInspector] public ArenaClass currentArena;
    
    [Header("Transforms"), Space(5)]
    [field: SerializeField] private Transform playerSpawnPosition;
    
    
    [field: HideInInspector] public float seconds;

    public bool GamePaused;
    
    private void Awake()
    {
        Instance = this;
        playerController = player.GetComponent<PlayerController>();
        player.transform.position = playerSpawnPosition.position;

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
        Time.timeScale = 0;
        var msm = win ? "you won" : "you lost";
        Debug.Log(msm);
    }
}

[System.Serializable]
public class ArenaClass
{
    public Arena arenaType;
    public GameObject enemiesParent;
    [HideInInspector] public List<GameObject> enemies;
    [HideInInspector] public List<ArenaEnemySpawner> enemiesSpawners;
    
}


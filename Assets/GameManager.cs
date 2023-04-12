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
    
    public CinemachineVirtualCamera cinemachineVirtualCamera;
    public GameObject player;
    [field: HideInInspector] public PlayerController playerController;

    public ArenaClass[] arenas;
    
    [field: SerializeField] private Transform playerSpawnPosition;
    [field: SerializeField] private Arena initialArena;
    [field: HideInInspector] public ArenaClass currentArena;
    
    [field: HideInInspector] public float time;
    
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
                Debug.Log(child.name);
                arenaClass.enemies.Add(child.gameObject);
            }
            
        }
        
        ChangeArena(initialArena);
    }

    private void Start()
    {
        foreach (var enemySpawn in EnemyManager.Instance.enemySpawnsNew)
        {
            GetArena(enemySpawn.arena).enemiesSpawners.Add(enemySpawn);
        }
        
        foreach (var arenaClass in arenas)
        {
            Debug.Log("Arena: " + arenaClass.arenaType);
            Debug.Log("Enemies: " + arenaClass.enemiesSpawners.Count);
        }
    }

    void Update()
    {
        time += Time.deltaTime;
    }

    public void ChangeArena(Arena newArena)
    {
        //if there is a current arena it disables those enemies
        if (currentArena != null)
        {
            foreach (var enemy in currentArena.enemies)
            {
                if (enemy == null)
                {
                    currentArena.enemies.Remove(enemy);
                    continue;
                }
                enemy.SetActive(false);
            }
            
        }
        
        //gets the arena and it makes the enemies on that arena active
        currentArena = GetArena(newArena);
        foreach (var enemy in currentArena.enemies)
        {
            if (enemy == null)
            {
                currentArena.enemies.Remove(enemy);
                continue;
            }
            enemy.SetActive(true);
        }
    }


    private ArenaClass GetArena(Arena arenaType)
    {
        return arenas.FirstOrDefault(arenaClass => arenaClass.arenaType == arenaType);
    }
}

[System.Serializable]
public class ArenaClass
{
    public Arena arenaType;
    public GameObject enemiesParent;
    [field: HideInInspector] public List<GameObject> enemies;
    public List<ArenaEnemySpawner> enemiesSpawners;
    
}


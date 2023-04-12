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
    [field: HideInInspector] public Afonso_PlayerController playerController;

    public ArenaClass[] arenas;
    
    [field: SerializeField] private Transform playerSpawnPosition;
    [field: SerializeField] private Arena initialArena;
    /*[field: HideInInspector]*/ public ArenaClass currentArena;
    
    private void Awake()
    {
        Instance = this;
        playerController = player.GetComponent<Afonso_PlayerController>();
        player.transform.position = playerSpawnPosition.position;

        foreach (var arenaClass in arenas)
        {
            arenaClass.Enemies = new List<GameObject>();
            foreach (Transform child in arenaClass.EnemiesParent.transform)
            {
                Debug.Log(child.name);
                arenaClass.Enemies.Add(child.gameObject);
            }
            
        }
        
        ChangeArena(initialArena);
    }
    
    
    void Update()
    {
        
    }

    public void ChangeArena(Arena newArena)
    {
        //if there is a current arena it disables those enemies
        if (currentArena != null)
        {
            foreach (var enemy in currentArena.Enemies)
            {
                if (enemy == null)
                {
                    currentArena.Enemies.Remove(enemy);
                    continue;
                }
                enemy.SetActive(false);
            }
            
        }
        
        //gets the arena and it makes the enemies on that arena active
        currentArena = GetArena(newArena);
        foreach (var enemy in currentArena.Enemies)
        {
            if (enemy == null)
            {
                currentArena.Enemies.Remove(enemy);
                continue;
            }
            enemy.SetActive(true);
        }
    }


    private ArenaClass GetArena(Arena arenaType)
    {
        return arenas.FirstOrDefault(arenaClass => arenaClass.ArenaType == arenaType);
    }
}

[System.Serializable]
public class ArenaClass
{
    public Arena ArenaType;
    public GameObject EnemiesParent;
    [field: HideInInspector]public List<GameObject> Enemies;
}

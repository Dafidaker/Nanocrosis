using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public CinemachineVirtualCamera cinemachineVirtualCamera;
    public GameObject player;
    [field: SerializeField] private Transform playerSpawnPosition;
    [field: SerializeField] private Arena initialArena;
    [field: HideInInspector] public Arena currentArena;
    public enum Arena
    {
        Lungs,
        Artery,
        Heart
    }

    private void Start()
    {
        Instance = this;
        //player.transform.position = playerSpawnPosition.position;
        currentArena = initialArena;
    }
    
    void Update()
    {
        
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

[System.Serializable]
public struct EnemyPercentage
{
    public Enemy enemy;
    public float weight;
}
[System.Serializable]
public struct ArenaPercentage
{
    public Arena arena;
    public float multiplier;
}

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance;
    
    public float maxValue;
    public float currentValue;
    private ArenaClass _lungs;
    public EnemyPercentage[] enemyPercentageInspector;
    private Dictionary<Enemy, float> _enemyPercentage;
    
    public ArenaPercentage[] arenaMultiplierInspector;
    private Dictionary<Arena, float> _arenaMultiplier;

    public float passiveHealingValue;
    public float updateUnlimitedObjectiveCooldown;
    private WaitForSeconds _unlimitedObjectiveWaitForSeconds;
    private void Awake()
    {
        Instance = this;
    }
    
    private void Start()
    {
        currentValue = maxValue;
        _lungs = GameManager.Instance.GetArena(Arena.Lungs);

        _unlimitedObjectiveWaitForSeconds = new WaitForSeconds(updateUnlimitedObjectiveCooldown);
        
        _enemyPercentage = new Dictionary<Enemy, float>();
        foreach (var enemyPercentage in enemyPercentageInspector)
        {
            _enemyPercentage.Add(enemyPercentage.enemy , enemyPercentage.weight);
        }
        
        _arenaMultiplier = new Dictionary<Arena, float>();
        foreach (var arenaPercentage in arenaMultiplierInspector)
        {
            _arenaMultiplier.Add(arenaPercentage.arena , arenaPercentage.multiplier);
        }
        
        StartCoroutine(UpdateUnlimitedObjective());
    }

    private IEnumerator UpdateUnlimitedObjective()
    {
        if (currentValue + passiveHealingValue <= maxValue)
        {
            currentValue += passiveHealingValue;
        }
        
        foreach (var enemySpawner in _lungs.enemiesSpawners.ToArray())
        {
            var amountOfEnemies = enemySpawner.enemies.Count;

            var weight = _enemyPercentage[enemySpawner.enemy];

            weight *= _arenaMultiplier[GameManager.Instance.currentArena.arenaType];

            currentValue -= amountOfEnemies * weight;
            
            Debug.Log("Enemy: " + enemySpawner.enemy + "\namountOfEnemies: " + enemySpawner.enemies.Count + "\nAdd to Value: " + amountOfEnemies * weight);
        }

        if (currentValue <= 0 )
        {
            GameManager.Instance.GameEnded(false);
        }
        
        yield return _unlimitedObjectiveWaitForSeconds;
        
        StartCoroutine(UpdateUnlimitedObjective());
    }
    
    public float GetPercentageOfCurrentValue()
    {
        return currentValue / maxValue;
    }

    public void CheckForPhageDeath(Component sender, object data)
    {
        if (sender is Hitable && sender.GetComponent<Hitable>().enemyType == Enemy.Phage)
        {
            Debug.Log("Boss was Killed");
            
            //game won
            GameManager.Instance.GameEnded(true);
            
        }
    }
}

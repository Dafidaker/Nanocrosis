using System;
using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

[System.Serializable]
public struct EnemyHealing
{
    public Enemy enemy;
    public float healing;
}

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


    [Header("Debug Objective"), Space(5)] 
    [field: SerializeField] private float[] minutes;
    [field: SerializeField] private Arena selectedArena;
    
    [Header("Objective Stats"), Space(5)] 
    public float maxValue;
    public float initialValue;
    public float currentValue;
    private ArenaClass _lungs;
    public EnemyPercentage[] enemyPercentageInspector;
    private Dictionary<Enemy, float> _enemyPercentage;
    
    public ArenaPercentage[] arenaMultiplierInspector;
    private Dictionary<Arena, float> _arenaMultiplier;
    
    public EnemyHealing[] enemyHealingInspector;
    private Dictionary<Enemy, float> _enemyHealing;

    public float passiveHealingValue;
    public float updateUnlimitedObjectiveCooldown;
    private WaitForSeconds _unlimitedObjectiveWaitForSeconds;
    
    
    
    private void Awake()
    {
        Instance = this;
    }
    
    private void Start()
    {
        currentValue = initialValue;
        _lungs = GameManager.Instance.GetArena(Arena.Lungs);

        GameEvents.Instance.lungsHealthChanged.Ping(null, null);
        
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
        
        _enemyHealing = new Dictionary<Enemy, float>();
        foreach (var enemyHealing in enemyHealingInspector)
        {
            _enemyHealing.Add(enemyHealing.enemy , enemyHealing.healing);
        }

        GetValueRemoved(minutes, selectedArena);
        
        
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
            
            //Debug.Log("Enemy: " + enemySpawner.enemy + "\namountOfEnemies: " + enemySpawner.enemies.Count + "\nAdd to Value: " + amountOfEnemies * weight);
        }

        if (currentValue <= 0 )
        {
            GameManager.Instance.GameEnded(false);
        }
        
        GameEvents.Instance.lungsHealthChanged.Ping(null, null);
        
        yield return _unlimitedObjectiveWaitForSeconds;
        
        StartCoroutine(UpdateUnlimitedObjective());
    }
    
    public float GetPercentageOfCurrentValue()
    {
        return currentValue / maxValue;
    }

    public void UpdateObjective(Component sender, object data) 
    {
        if (sender is Hitable && GameManager.Instance.currentArena.arenaType == Arena.Lungs)
        {
            var enemyType = sender.GetComponent<Hitable>().enemyType;
            if (enemyType == Enemy.Phage) { return; }
            var healingValue = _enemyHealing[enemyType];
            
            Debug.Log("healed: " + healingValue);
            
            if (currentValue + healingValue <= maxValue)
            {
                currentValue += healingValue;
            }
        }
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

    public void GetValueRemoved(float[] Minutes,Arena arenaMultiplierType  )
    {
        float positiveHealth = passiveHealingValue;
        
        float kurokuWeight = _enemyPercentage[Enemy.Kuroru];
        float chikaiWeight = _enemyPercentage[Enemy.Chikai];
        float toiWeight  = _enemyPercentage[Enemy.Toi];
        
        var arena = GameManager.Instance.GetArena(Arena.Lungs);

        var kuroruAmountCurve = GameManager.Instance.GetArenaEnemySpawner(arena.enemiesSpawners, Enemy.Kuroru).maxAmount;
        var chikaiAmountCurve = GameManager.Instance.GetArenaEnemySpawner(arena.enemiesSpawners, Enemy.Chikai).maxAmount;
        var toiAmountCurve = GameManager.Instance.GetArenaEnemySpawner(arena.enemiesSpawners, Enemy.Toi).maxAmount;

        foreach (var minute in Minutes)
        {
            float maxKuroruAmount;
            if (minute > kuroruAmountCurve[kuroruAmountCurve.length - 1].time)
            {
                maxKuroruAmount = GameManager.Instance.GetArenaEnemySpawner(arena.enemiesSpawners, Enemy.Kuroru).maxAmount.Evaluate(kuroruAmountCurve[kuroruAmountCurve.length - 1].time);
            }
            else
            {
                maxKuroruAmount = GameManager.Instance.GetArenaEnemySpawner(arena.enemiesSpawners, Enemy.Kuroru).maxAmount.Evaluate(minute);
            }
            
            float maxChikaiAmount;
            if (minute > chikaiAmountCurve[chikaiAmountCurve.length - 1].time)
            {
                maxChikaiAmount = GameManager.Instance.GetArenaEnemySpawner(arena.enemiesSpawners, Enemy.Chikai).maxAmount.Evaluate(chikaiAmountCurve[chikaiAmountCurve.length - 1].time);
            }
            else
            {
                maxChikaiAmount = GameManager.Instance.GetArenaEnemySpawner(arena.enemiesSpawners, Enemy.Chikai).maxAmount.Evaluate(minute);
            }
            
            float maxToiAmount;
            if (minute > toiAmountCurve[toiAmountCurve.length - 1].time)
            {
                maxToiAmount = GameManager.Instance.GetArenaEnemySpawner(arena.enemiesSpawners, Enemy.Toi).maxAmount.Evaluate(toiAmountCurve[toiAmountCurve.length - 1].time);
            }
            else
            {
                maxToiAmount = GameManager.Instance.GetArenaEnemySpawner(arena.enemiesSpawners, Enemy.Toi).maxAmount.Evaluate(minute);
            }
            
            float arenaMultiplier = _arenaMultiplier[Arena.Heart];
            float valueLost = positiveHealth - ((maxKuroruAmount * kurokuWeight) * arenaMultiplier) - ((maxChikaiAmount * chikaiWeight) * arenaMultiplier) - ((maxToiAmount * toiWeight) * arenaMultiplier);
            
            arenaMultiplier = _arenaMultiplier[Arena.Lungs];
            float valueLostLungs = positiveHealth - ((maxKuroruAmount * kurokuWeight) * arenaMultiplier) - ((maxChikaiAmount * chikaiWeight) * arenaMultiplier) - ((maxToiAmount * toiWeight) * arenaMultiplier);
            Debug.Log("");
            Debug.Log("MINUTE " + minute + " - MINUTE " + minute + " - MINUTE " + minute);
            //Debug.Log("At minute " + minute + " the value lost every " + updateUnlimitedObjectiveCooldown + " seconds is " + -valueLost);
            Debug.Log("HEART - HEART - HEART - HEART - HEART - HEART ");
            Debug.Log("Each Minute the value lost is " + (60 / updateUnlimitedObjectiveCooldown) * valueLost);
            Debug.Log("Would go from " + maxValue + " to 0 in " + -((maxValue / valueLost) * updateUnlimitedObjectiveCooldown)/ 60 + " mins" );
            Debug.Log("At minute " + minute + " there will be " + maxKuroruAmount + " kurokus, " + maxChikaiAmount + " chikais "+ maxToiAmount +" tois "); 
            
            Debug.Log("LUNGS - LUNGS - LUNGS - LUNGS - LUNGS - LUNGS ");
            Debug.Log("Each Minute the value lost is " + (60 / updateUnlimitedObjectiveCooldown) * valueLostLungs);
            Debug.Log("Would go from " + maxValue + " to 0 in " + -((maxValue / valueLostLungs) * updateUnlimitedObjectiveCooldown)/ 60 + " mins" );
            Debug.Log("At minute " + minute + " there will be " + maxKuroruAmount + " kurokus, " + maxChikaiAmount + " chikais "+ maxToiAmount +" tois "); 
        }
        
        
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

[Serializable]
public struct EnemyHealing
{
    public Enemy enemy;
    public float healing;
}

[Serializable]
public struct EnemyPercentage
{
    public Enemy enemy;
    public float weight;
}
[Serializable]
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

    private float _timeSinceEnemyKilled;
    [field: SerializeField] private float healingDuration;
    public float passiveHealingValue;
    public float updateUnlimitedObjectiveCooldown;
    private WaitForSeconds _unlimitedObjectiveWaitForSeconds;
    
    [Header("Node Stats"), Space(5)] 
    public float nodeHealingValue;
    public float nodeDamagingValue;

    public float lungHealthChange;
    private int _percentagedChanged;
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

        //GetValueRemoved(minutes, selectedArena);
        
        
        StartCoroutine(UpdateUnlimitedObjective());
    }

    private IEnumerator UpdateUnlimitedObjective()
    {
        var lastValue = GetCurrentValue(true, true); // currentValue
        //Debug.Log("currentValue: " + currentValue);
        //Debug.Log("lastValue: " + lastValue);
        var lastLungHealth = lungHealthChange;
        
        if (currentValue + passiveHealingValue <= maxValue && _timeSinceEnemyKilled > 0)
        {
            //currentValue += passiveHealingValue;
            ChangeValue(passiveHealingValue);
        }
        
        foreach (var enemySpawner in _lungs.enemiesSpawners.ToArray())
        {
            var amountOfEnemies = enemySpawner.enemies.Count;

            var weight = _enemyPercentage[enemySpawner.enemy];

            weight *= _arenaMultiplier[GameManager.Instance.currentArena.arenaType];

            //currentValue -= amountOfEnemies * weight;
            ChangeValue(-(amountOfEnemies * weight));
            
            //Debug.Log("Enemy: " + enemySpawner.enemy + "\namountOfEnemies: " + enemySpawner.enemies.Count + "\nAdd to Value: " + amountOfEnemies * weight);
        }

        if (currentValue <= 0 )
        {
            GameManager.Instance.GameEnded(false);
        }
        
        yield return _unlimitedObjectiveWaitForSeconds;
        
        StartCoroutine(UpdateUnlimitedObjective());
    }

    public void ChangeValue(float change)
    {
        var lastValue = GetCurrentValue(true, true);
        var lastLungHealth = lungHealthChange;
        
        //change value
        currentValue += change;
        GameEvents.Instance.lungsHealthChanged.Ping(null, null);
        
        lungHealthChange += (GetCurrentValue(true, true) - lastValue);
        
        var compareOldValue = Convert.ToInt32(Math.Floor(lastLungHealth));
        var compareNewValue = Convert.ToInt32(Math.Floor(lungHealthChange));
        
        if (compareOldValue < compareNewValue) //a percentaged when up
        {
            _percentagedChanged++;
            if (_percentagedChanged >= 10)
            {
                _percentagedChanged = 0;
                DoctorManager.Instance.AddToQueue("LungsGainedHealth");
            }
            //Debug.Log("percentagedChanged: " + _percentagedChanged);
        }
        
        if (compareOldValue > compareNewValue) //a percentaged when down
        {
            _percentagedChanged--;
            if (_percentagedChanged <= -10)
            {
                _percentagedChanged = 0;
                DoctorManager.Instance.AddToQueue("LungsLostHealth");
            }
            //Debug.Log("percentagedChanged: " + _percentagedChanged);
        }
        
        if (lastValue > 10 && GetCurrentValue(true,true) <= 10)
        {
            DoctorManager.Instance.AddToQueue("LungsLowHealth");
        }
    }

    private void Update()
    {
        _timeSinceEnemyKilled -= Time.deltaTime;
    }

    public float GetPercentageOfCurrentValue()
    {
        return currentValue / maxValue;
    }
    
    public float GetCurrentValue(bool baseHundred, bool truncate ,bool round = false,int decimalCases = 0, MidpointRounding mode = MidpointRounding.ToEven)
    {
        var value = currentValue / maxValue;
        
        if (round) { value = (float)Math.Round(value, decimalCases, mode); }
        if (baseHundred) { value *= 100; }
        if (truncate) { return (float)Math.Truncate(value); }
        
        return currentValue / maxValue;
    }

    public void UpdateObjective(Component sender, object data)
    {
        if (sender is not Hittable || GameManager.Instance.currentArena.arenaType != Arena.Lungs) return;
        
        var enemyType = sender.GetComponent<Hittable>().enemyType;
        if (enemyType == Enemy.Phage) { return; }
        var healingValue = _enemyHealing[enemyType];

        _timeSinceEnemyKilled = healingDuration;
            
        if (currentValue + healingValue <= maxValue)
        {
            ChangeValue(healingValue);
            //currentValue += healingValue;
        }
    }
    
    public void CheckForPhageDeath(Component sender, object data) 
    {
        if (sender is Hittable && sender.GetComponent<Hittable>().enemyType == Enemy.Phage)
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

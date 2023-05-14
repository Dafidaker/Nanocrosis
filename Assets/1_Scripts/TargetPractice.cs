using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class TargetPractice : MonoBehaviour
{
    //public float targetHealth;
    public GameObject target;
    public float intervalBetweenSpawns;
    public int maxAmountTargets;
    
    private float intervalTimer;
    private List<GameObject> targets;

    private void Start()
    {
        targets = new List<GameObject>();
    }

    void Update()
    {
        intervalTimer -= Time.deltaTime;
        
        if (intervalTimer <= 0 && targets.Count < maxAmountTargets)
        {
            intervalTimer = intervalBetweenSpawns;
            
            var x = transform.position.x + Random.Range(-(transform.localScale.x / 2), (transform.localScale.x / 2));
            var y = transform.position.y + Random.Range(-(transform.localScale.y / 2), (transform.localScale.y / 2));
            var z = transform.position.z + Random.Range(-(transform.localScale.z / 2), (transform.localScale.z / 2));
            //spawn target 
            var tempGameObject = Instantiate(target, new Vector3(x, y, z), Quaternion.identity);
            targets.Add(tempGameObject);
        }

        if (targets.Count < maxAmountTargets) return;

        foreach (var go in targets.ToArray())
        {
            if (go == null)
            {
                targets.Remove(go);
            }
        }
    }
    
}

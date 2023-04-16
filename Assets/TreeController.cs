using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

[System.Serializable]
public struct ItemSpawn
{
    public Transform Waypoint;
    [field: HideInInspector] public GameObject itemGameObject;
    [field: HideInInspector] public ItemType Item;
}

public class TreeController : MonoBehaviour
{
    public List<ItemSpawn> ItemSpawns;

    public void AddItem(ItemSpawn itemSpawn,ItemType Item, GameObject prefab )
    {
        Debug.Log("Created Item");
        var tempGo = Instantiate(prefab, itemSpawn.Waypoint.position, Quaternion.identity);
        itemSpawn.Item = Item;
        itemSpawn.itemGameObject = tempGo;
    }
    
    public ItemSpawn? GetEmptyItemWaypoint()
    {
        foreach (var itemSpawn in ItemSpawns)
        {
            if (itemSpawn.itemGameObject == null)
            {
                return itemSpawn;
            }  
        }

        return null;
    }
}

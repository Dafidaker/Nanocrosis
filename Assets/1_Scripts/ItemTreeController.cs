using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enums;
using UnityEngine;

[System.Serializable]
public struct ItemSpawn
{
    public Transform waypoint;
    [field: HideInInspector] public GameObject itemGameObject;
    [field: HideInInspector] public ItemType item;
}

public class ItemTreeController : MonoBehaviour
{
    public List<ItemSpawn> itemSpawns;

    public void AddItem(ItemSpawn itemSpawn, ItemType item, GameObject prefab )
    {
        var tempGo = Instantiate(prefab, itemSpawn.waypoint.position, Quaternion.identity);
        itemSpawn.item = item;
        itemSpawn.itemGameObject = tempGo;
    }
    
    public ItemSpawn? GetEmptyItemWaypoint()
    {
        foreach (var itemSpawn in itemSpawns.Where(itemSpawn => itemSpawn.itemGameObject == null))
        {
            return itemSpawn;
        }

        return null;
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enums;
using UnityEngine;

[System.Serializable]
public struct ItemSpawn
{
    public Transform waypoint;
    /*[field: HideInInspector]*/ public GameObject itemGameObject;
    [field: HideInInspector] public ItemType? item;
}

public class ItemTreeController : MonoBehaviour
{
    public List<ItemSpawn> itemSpawns;
    public List<ItemTreeBranchController> itemBranchSpawns;
    
    public void AddItem(ItemSpawn itemSpawn, ItemType item, GameObject prefab )
    {
        var tempGo = Instantiate(prefab, itemSpawn.waypoint.position, Quaternion.identity);
        itemSpawn.item = item;
        itemSpawn.itemGameObject = tempGo;
    }
    
    public void AddItemToBranch(ItemType item, GameObject prefab)
    {
        var branchResult = GetEmptyItemBranchWaypoint();
        if (branchResult == null) return;
        
        var itemSpawn = (ItemTreeBranchController) branchResult;
        itemSpawn.AddItem(item, prefab);
        /*Debug.Log(itemSpawn.waypoint.position);
        var tempGo = Instantiate(prefab, itemSpawn.waypoint.position, Quaternion.identity);
        itemSpawn.item = item;
        itemSpawn.itemGameObject = tempGo;
        tempGo.GetComponent<Collider>().enabled = false;*/
    }
    
    public ItemSpawn? GetEmptyItemWaypoint()
    {
        foreach (var itemSpawn in itemSpawns)
        {
            if (itemSpawn.item == null)
            {
                return itemSpawn;
            }
        }

        return null;
    }
    
    public ItemTreeBranchController? GetEmptyItemBranchWaypoint()
    {
        foreach (var branch in itemBranchSpawns)
        {
            var itemSpawn = branch.itemSpawn;
            if (itemSpawn.item == null)
            {
                return branch;
            }
        }

        return null;
    }
}

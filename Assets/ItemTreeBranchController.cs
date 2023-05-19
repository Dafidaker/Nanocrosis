using Enums;
using UnityEngine;
using UnityEngine.Serialization;

public class ItemTreeBranchController : MonoBehaviour
{
    public ItemSpawn itemSpawn;

    public void RemovedItem()
    {
        Debug.Log("remove item");
        itemSpawn.item = null;
        Destroy(itemSpawn.itemGameObject);
        itemSpawn.itemGameObject = null;
    }

    public void AddItem(ItemType item, GameObject prefab)
    {
        var tempGo = Instantiate(prefab, itemSpawn.waypoint.position, Quaternion.identity);
        itemSpawn.item = item;
        itemSpawn.itemGameObject = tempGo;
        tempGo.GetComponent<Collider>().enabled = false;
    }

    private void Awake()
    {
        itemSpawn = new ItemSpawn
        {
            waypoint = transform
        };
    }

    
}

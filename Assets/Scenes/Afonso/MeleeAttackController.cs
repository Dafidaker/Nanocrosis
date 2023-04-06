using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttackController : MonoBehaviour
{
    [SerializeField] private Transform InitialPos;
    [SerializeField] private Transform TargetPos;
    [SerializeField] private GameObject AmmoPickup;
    [SerializeField] private GameObject EnhancementPickup;
    [SerializeField] private int Damage;

    private void OnEnable()
    {
        transform.position = InitialPos.position;
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, TargetPos.position, 25 * Time.deltaTime);
        if(transform.position == TargetPos.position)
        {
            gameObject.SetActive(false);
        }
        if (!gameObject.activeInHierarchy)
        {
            transform.position = InitialPos.position;
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        if(col.GetComponent<TargetController>() != null)
        {
            TargetController t = col.GetComponent<TargetController>();
            if (t.ShieldActive) Debug.Log("Melee cannot damage shield");
            else t.CurrentHealthPoints -= Damage;
            if(t.CurrentHealthPoints <= 0)
            {
                Instantiate(AmmoPickup, t.AmmoPickupSpawnpoint.position, Quaternion.identity);
                if (t.HasShield) Instantiate(EnhancementPickup, t.EnhancementPickupSpawnpoint.position, Quaternion.identity);
            }
        }
    }

    private void OnDisable()
    {
        transform.position = InitialPos.position;
    }
}

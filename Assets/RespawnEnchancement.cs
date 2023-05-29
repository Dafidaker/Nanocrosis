using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnEnchancement : MonoBehaviour
{
    [SerializeField] private float TimeToRespawn;
    [SerializeField] private GameObject DisableObject;
    [SerializeField] private bool respawn;
    
    private bool _interactable = true;
    public void Interact()
    {
        if (!_interactable) return;
        /*PlayerController.Instance.BombAttached = true;
        PlayerController.Instance.FakeBomb.SetActive(true);*/
        if (respawn)
        {
            StartCoroutine(Respawn());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator Respawn()
    {
        //gameObject.GetComponent<BoxCollider>().enabled = false;
        gameObject.GetComponent<SphereCollider>().enabled = false;
        //gameObject.GetComponent<MeshRenderer>().enabled = false;
        DisableObject.gameObject.SetActive(false);
        _interactable = false;  
        yield return new WaitForSeconds(TimeToRespawn);
        //.GetComponent<BoxCollider>().enabled = true;
        gameObject.GetComponent<SphereCollider>().enabled = true;
        //gameObject.GetComponent<MeshRenderer>().enabled = true;
        DisableObject.gameObject.SetActive(true);
        //Center.GetComponent<MeshRenderer>().enabled = true;
        _interactable = true;
    }
}

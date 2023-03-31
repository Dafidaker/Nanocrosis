using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombPickupController : MonoBehaviour
{
    [SerializeField] private float TimeToRespawn;
    [SerializeField] private GameObject Center;

    private bool _interactable = true;
    public void Interact()
    {
        if (!_interactable) return;
        Afonso_PlayerController.Instance.BombAttached = true;
        Afonso_PlayerController.Instance.FakeBomb.SetActive(true);
        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        gameObject.GetComponent<BoxCollider>().enabled = false;
        gameObject.GetComponent<SphereCollider>().enabled = false;
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        Center.GetComponent<MeshRenderer>().enabled = false;
        _interactable = false;
        yield return new WaitForSeconds(TimeToRespawn);
        gameObject.GetComponent<BoxCollider>().enabled = true;
        gameObject.GetComponent<SphereCollider>().enabled = true;
        gameObject.GetComponent<MeshRenderer>().enabled = true;
        Center.GetComponent<MeshRenderer>().enabled = true;
        _interactable = true;
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class ColorKuroku : MonoBehaviour
{
    [SerializeField] private MeshRenderer[] body;
    [SerializeField] private MeshRenderer[] sphere;
    
    [SerializeField] private Material[] bodyMaterials;
    [SerializeField] private Material[] sphereMaterials;
    // Start is called before the first frame update
    void Start()
    {
        var index = Random.Range(0, bodyMaterials.Length);

        foreach (var meshRenderer in body)
        {
            meshRenderer.material = bodyMaterials[index];
        }
        
        foreach (var meshRenderer in sphere)
        {
            meshRenderer.material = sphereMaterials[index];
        }
    }

    
}

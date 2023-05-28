using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualGunController : MonoBehaviour
{
    [field: SerializeField] private Transform follow;
    
    [field: SerializeField] private Material rifleAmmoMaterial;
    [field: SerializeField] private Material shotgunAmmoMaterial;

    [field: SerializeField] private MeshRenderer meshRenderer;

    private void Start()
    {
        ChangeWeaponVisability(false);
    }

    private void Update()
    {
        var t = transform;
        t.position = follow.position;
        t.rotation = follow.rotation;
    }

    public void ChangeGun(bool isRifle)
    {
        var materials = new Material[meshRenderer.sharedMaterials.Length];
        for (var i = 0; i < meshRenderer.sharedMaterials.Length; i++)
        {
            if (i == 3)
            {
                materials[i] = isRifle ? rifleAmmoMaterial : shotgunAmmoMaterial;
                continue;
            }
            
            materials[i] = meshRenderer.sharedMaterials[i];
        } 
        
        meshRenderer.sharedMaterials = materials;
    }

    public void ChangeWeaponVisability(bool Show)
    {
        meshRenderer.enabled = Show;
    }

}

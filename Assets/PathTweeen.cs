using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PathTweeen : MonoBehaviour
{
    [field: SerializeField]private Transform[] pathVertices;

    private List<Vector3> _verticesPositions;
    // Start is called before the first frame update
    void Start()
    {
        _verticesPositions = new List<Vector3>();
        foreach (var vertex in pathVertices)
        {
            _verticesPositions.Add(vertex.position);
        }

        transform.DOPath(_verticesPositions.ToArray(), 5f, PathType.CatmullRom).SetLoops(-1, LoopType.Yoyo);
    }

    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReduceSize : MonoBehaviour
{
    [field: SerializeField] private AnimationCurve animationCurve;
    private Vector3 _initialScale;
    [field: HideInInspector] public float duration;
    private float _timer;
    void Start()
    {
        _initialScale = transform.localScale;
        _timer = duration;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.LookAt(GameManager.Instance.player.transform);
        
        _timer -= Time.deltaTime;

        var value = _timer / duration;

        transform.localScale = _initialScale * animationCurve.Evaluate(value);

        if (_timer < 0)
        {
            Destroy(gameObject);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable]
public class SmoothlyTransitionVignette
{
    [HideInInspector]public Vignette _vignette;
    [Range(0f, 1f), SerializeField] public float minValue;
    [Range(0f, 1f), SerializeField] public float maxValue;
    [Range(0f, 10f), SerializeField] public float animationSpeed;

    [HideInInspector]public bool transitionEnded;
    
    public IEnumerator TransitionVignette(bool show)
    {
        transitionEnded = false;
        var goalValue = (show) ? maxValue : minValue;
        while (Math.Abs(_vignette.intensity.value - goalValue) > 0.01f)
        {
            if (_vignette.intensity.value > goalValue)
            {
                _vignette.intensity.value -= 0.5f * Time.deltaTime * animationSpeed;
            }
            else
            {
                _vignette.intensity.value += 0.5f * Time.deltaTime * animationSpeed;
            }
            yield return new WaitForSeconds(0.01f);

        }
        transitionEnded = true;
    }
}

public class PostProcessingController : MonoBehaviour
{
    private Volume _volume;
    public SmoothlyTransitionVignette VignetteValues;
    private ColorAdjustments _colorAdjustments;
    private bool colorAjustmentCourotine;
    private ChromaticAberration _chromaticAberration;
    public void Start()
    {
        _volume = GetComponent<Volume>();
        //_volume.profile.components.ForEach(c => Debug.Log(c.GetType().Name));
        _volume.profile.TryGet(out VignetteValues._vignette);
        _volume.profile.TryGet(out _colorAdjustments);
    }

    private IEnumerator FlashVignette(SmoothlyTransitionVignette values)
    {
        StartCoroutine(values.TransitionVignette(true));
        while (!values.transitionEnded)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);
        StartCoroutine(values.TransitionVignette(false));
    }
    
    private IEnumerator Decrease(ColorAdjustments color, float Decreaseduration, float targetValue  )
    {
        colorAjustmentCourotine = true;
        var floatParameter = color.saturation;
        float timeElapsed = 0;
        float initialValue = floatParameter.value;
        while (MathF.Abs(floatParameter.value - targetValue) > float.Epsilon)
        {
            floatParameter.value = Mathf.Lerp(initialValue, targetValue, timeElapsed / Decreaseduration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        floatParameter.value = targetValue;
        colorAjustmentCourotine = false;
    }

    public void DeathPostProcessing(Component sender, object data)
    {
        if (data is not (float, float)) return;
        var result = ((float, float))data;
        
        StartCoroutine(DeathColorAjustments(result.Item1, result.Item2));
    }
    public void WasDamagedPostProcessing(Component sender, object data)
    {
        StartCoroutine(FlashVignette(VignetteValues));
    }
    
    private IEnumerator DeathColorAjustments(float noColorTimer, float deadtimer)
    {
        StartCoroutine(Decrease(_colorAdjustments,noColorTimer,-100f));
        while (colorAjustmentCourotine)
        {
            yield return null;
        }

        yield return new WaitForSeconds(3f);
        StartCoroutine(Decrease(_colorAdjustments,deadtimer - noColorTimer - 3f,0f));
    }

    
    private void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.V))
        {
            StartCoroutine(FlashVignette(VignetteValues));
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            StartCoroutine(Decrease(_colorAdjustments,1f,-100f));
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            StartCoroutine(Decrease(_colorAdjustments,9f,0f));
        }*/
    }
}

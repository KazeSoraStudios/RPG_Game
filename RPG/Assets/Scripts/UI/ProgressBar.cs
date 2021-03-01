using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] float FillSpeed = 1.0f;
    [SerializeField] float TargetProgress;
    [SerializeField] Slider Slider;

    private bool filling = false;

    public void SetTargetFillAmountImmediate(float target)
    {
        filling = false;
        TargetProgress = target;
        Slider.value = TargetProgress;
    }

    public void SetTargetFillAmount(float target)
    {
        TargetProgress = target;
        if (!filling)
            StartCoroutine(Fill());
    }

    private IEnumerator Fill()
    {
        filling = true;
        while (Slider.value < TargetProgress)
        {
            Slider.value += FillSpeed * Time.deltaTime;
            yield return null;
        }
        filling = false;
    }
}

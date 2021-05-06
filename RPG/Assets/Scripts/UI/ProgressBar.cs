using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace RPG_UI
{
    public class ProgressBar : MonoBehaviour
    {
        [SerializeField] float FillSpeed = 1.0f;
        [SerializeField] float TargetProgress;
        [SerializeField] Slider Slider;

        private bool filling = false;

        public void SetTargetFillAmountImmediate(float target)
        {
            if (Mathf.Abs(TargetProgress - target) < 1.0f)
                return;
            filling = false;
            TargetProgress = target;
            Slider.value = TargetProgress;
        }

        public void SetTargetFillAmount(float target)
        {
            TargetProgress = target;
            if (!filling)
                Fill(target);
        }

        private void Fill(float target)
        {
            if (target > Slider.value)
                StartCoroutine(FillUp());
            else
                StartCoroutine(FillDown());
        }

        private IEnumerator FillUp()
        {
            filling = true;
            while (Slider.value < TargetProgress)
            {
                Slider.value += FillSpeed * Time.deltaTime;
                yield return null;
            }
            Slider.value = TargetProgress;
            filling = false;
        }

        private IEnumerator FillDown()
        {
            filling = true;
            while (Slider.value > TargetProgress)
            {
                Slider.value -= FillSpeed * Time.deltaTime;
                yield return null;
            }
            Slider.value = TargetProgress;
            filling = false;
        }
    }
}
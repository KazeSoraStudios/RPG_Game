using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace RPG_UI
{
    public class XPPopUp : ConfigMonoBehaviour
    {
        [SerializeField] public float DisplayTime = 0;
        [SerializeField] TextMeshProUGUI Text;
       
        private bool tweening = false;
        private bool tweenOn = true;
        private float fadeTime = 0.0001f;
        private float time = 0.0f;
        private float alpha = 0.0f;
        private List<CanvasRenderer> renderers = new List<CanvasRenderer>();

        public void Init(string text, Color color)
        {
            Text.SetText(text);
            Text.color = color;
            time = 0.0f;
            renderers.Add(GetComponent<CanvasRenderer>());
            foreach (var renderer in transform.GetComponentsInChildren<CanvasRenderer>())
                renderers.Add(renderer);
        }

        public void Execute(float deltaTime)
        {
            if (!tweening)
                return;
            time += deltaTime * fadeTime;
            alpha += Mathf.Lerp(alpha, 1, time / 1);
            var a = tweenOn ? alpha : 1 - alpha;
            if ((tweenOn && alpha >= 1) || (!tweenOn && a <= 0))
            {
                tweening = false;
                DisplayTime = Mathf.Min(3.0f, DisplayTime + deltaTime);
                time = 0.0f;
                alpha = 0.0f;
            }
            foreach (var renderer in renderers)
                renderer.SetAlpha(a);
        }

        public bool IsFinished()
        {
            return !tweening && !tweenOn;
        }

        public void TurnOn()
        {
            tweening = true;
            tweenOn = true;
        }

        public void TurnOff()
        {
            tweening = true;
            tweenOn = false;
        }

        public bool IsTurningOff()
        {
            return tweening && !tweenOn;
        }
    }

}
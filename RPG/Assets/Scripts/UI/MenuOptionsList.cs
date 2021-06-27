using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RPG_UI
{
    public class MenuOptionsList : ConfigMonoBehaviour
    {
        public class Config
        {
            public bool ShowSelection = true;
            public List<string> Names;
            public Action<int> OnClick;
        }

        [SerializeField] float SelectionPadding;
        [SerializeField] Image SelectionArrow;
        [SerializeField] TextMeshProUGUI[] Options;

        private int numberOfOptions;
        public int currentSelection = 0;
        private float time;
        private Config config;

        public void Init(Config config)
        {
            if (CheckUIConfigAndLogError(config, name))
            {
                return;
            }

            this.config = config;
            numberOfOptions = Options.Length - 1;
            time = 0.0f;
            if (config.Names != null)
                SetText(config.Names);
            if (config.ShowSelection)
                ShowCursor();
            else
                HideCursor();
        }
        public float sp = 1.0f;
        public void ApplySelectionBounce(float deltaTime)
        {
            time += deltaTime;
            var position = SelectionArrow.transform.position;
            var bounce = new Vector3(position.x, Mathf.Sin(time * sp) + position.y, 0.0f);
            SelectionArrow.transform.position = bounce;
        }

        public void HideCursor()
        {
            if (SelectionArrow == null)
                return;
            SelectionArrow.gameObject.SetActive(false);
        }

        public void ShowCursor()
        {
            if (SelectionArrow == null)
                return;
            SelectionArrow.gameObject.SetActive(true);
        }

        public int GetSelection()
        {
            return currentSelection;
        }

        public void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                IncreaseSelection();
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                DecreaseSelection();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                config.OnClick?.Invoke(currentSelection);

            }
        }

        public void IncreaseSelection()
        {
            currentSelection++;
            if (currentSelection > numberOfOptions)
                currentSelection = 0;
            var y = Options[currentSelection].transform.position.y;
            var position = new Vector2(SelectionArrow.transform.position.x, y);
            SelectionArrow.transform.position = position;
        }

        public void DecreaseSelection()
        {
            currentSelection--;
            if (currentSelection < 0)
                currentSelection = numberOfOptions;
            var y = Options[currentSelection].transform.position.y;
            var position = new Vector2(SelectionArrow.transform.position.x, y);
            SelectionArrow.transform.position = position;
        }

        public void SetTextColor(int option, Color color)
        {
            if (option < 0 || option >= Options.Length)
            {
                LogManager.LogWarn($"Index {option} is larger than OptionList size {Options.Length}");
                return;
            }
            Options[option].color = color;
        }

        private void SetText(List<string> names)
        {
            int i = 0;
            for (; i < names.Count && i < Options.Length; i++)
            {
                var name = names[i];
                Options[i].SetText(name);
            }
            for (; i < Options.Length; i++)
                Options[i].gameObject.SafeSetActive(false);
        }
    }
}
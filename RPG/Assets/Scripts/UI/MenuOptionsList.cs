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
            public bool VerticalSelection = true;
            public List<string> Names;
            public Action<int> OnChange;
            public Action<int> OnClick;
        }

        [SerializeField] float SelectionPadding;
        [SerializeField] Image SelectionArrow;
        [SerializeField] TextMeshProUGUI[] Options;

        private bool isVertical = true;
        private int numberOfOptions;
        public int currentSelection = 0;
        private float time;
        private KeyCode increase;
        private KeyCode decrease;
        private Config config;

        public void Init(Config config)
        {
            if (CheckUIConfigAndLogError(config, name))
            {
                return;
            }
            this.config = config;
            if (config.VerticalSelection)
            {
                isVertical = true;
                increase = KeyCode.DownArrow;
                decrease = KeyCode.UpArrow;
            }
            else
            {
                isVertical = false;
                increase = KeyCode.RightArrow;
                decrease = KeyCode.LeftArrow;
            }
            currentSelection = 0;
            numberOfOptions = Options.Length - 1;
            time = 0.0f;
            if (config.Names != null)
                SetText(config.Names);
            if (config.ShowSelection)
                ShowCursor();
            else
                HideCursor();
            SetSelectionPosition();
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
            if (Input.GetKeyDown(increase))
            {
                IncreaseSelection();
            }
            else if (Input.GetKeyDown(decrease))
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
            SetSelectionPosition();
            config.OnChange?.Invoke(currentSelection);
        }

        public void DecreaseSelection()
        {
            currentSelection--;
            if (currentSelection < 0)
                currentSelection = numberOfOptions;
            SetSelectionPosition();
            config.OnChange?.Invoke(currentSelection);
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

        public void OnClick()
        {
            config.OnClick?.Invoke(currentSelection);
        }

        private void SetSelectionPosition()
        {
            Vector2 position;
            if (isVertical)
            {
                var y = Options[currentSelection].transform.position.y;
                position = new Vector2(SelectionArrow.transform.position.x, y);
            }
            else
            {
                var x = Options[currentSelection].transform.position.x;
                var rect = (RectTransform)Options[currentSelection].transform;
                x -= rect.rect.width * 0.5f;
                position = new Vector2(x, SelectionArrow.transform.position.y);
            }
            SelectionArrow.transform.position = position;
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
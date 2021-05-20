using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RPG_UI
{
    public enum ScrollDirection { Horizontal, Vertical }

    public interface IScrollHandler
    {
        string GetPrefabPath();
        void InitCell(int index, ScrollViewCell cell);
        Vector2 GetCellSize();
        void OnAfterLoad();
        int GetNumberOfCells();
    }

    public class ScrollView : ConfigMonoBehaviour
    {
        [Min(1)]
        [SerializeField] int ColumnCount;
        [SerializeField] float XPadding;
        [SerializeField] float YPadding;
        [SerializeField] ScrollDirection Direction;
        [SerializeField] RectTransform Content;
        [SerializeField] IScrollHandler ScrollHandler;
        [SerializeField] Image SelectionArrow;
        [SerializeField] LayoutGroup LayoutGroup;

        private bool redraw;
        private int arrowY = 0;
        private int arrowX = 0;
        private int displayStart = 0;
        private int displayRows;
        private int displayIndex;
        private int numberOfRows;
        private List<ScrollViewCell> cells = new List<ScrollViewCell>();
        private Action<int> OnChange;
        private Action<int> OnSelect;
        private UIController uiController;

        public void Init(IScrollHandler scrollHandler, Action<int> onChange = null, Action<int> onSelect = null)
        {
            if (CheckUIConfigAndLogError(scrollHandler, "Scrollview"))
                return;
            uiController = ServiceManager.Get<UIController>();
            ResetScrollView();
            ScrollHandler = scrollHandler;
            OnChange = onChange;
            OnSelect = onSelect;
            Draw();
        }

        public void ResetScrollView()
        {
            ClearCells();
            arrowY = 0;
            arrowX = 0;
            displayRows = 0;
            displayIndex = 0;
            redraw = true;
        }

        public void Refresh()
        {
            ResetScrollView();
            Draw();
        }

        public void ClearCells()
        {
            for (int i = cells.Count - 1; i > -1; i--)
            {
                cells[i].OnExit();
                uiController.ReturnCellToPool(cells[i]);
                cells.RemoveAt(i);
            }
        }

        public void Execute()
        {
            if (!redraw)
                return;
            redraw = false;
            for (int i = 0; i < cells.Count; i++)
            {
                int index = i + displayStart * ColumnCount;
                ScrollHandler.InitCell(index, cells[i]);
            }
        }

        public void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                HandleUpMovement();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                HandleDownMovement();
            }
            else
                redraw = false;

            if (Input.GetKeyDown(KeyCode.Space))
                OnSelect?.Invoke(GetCurrentSelection());

            if (ColumnCount < 2)
                return;

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                HandleLeftInput();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
                HandleRightInput();
        }

        public void HideCursor()
        {
            if (SelectionArrow != null)
                SelectionArrow.gameObject.SafeSetActive(false);
        }

        public void ShowCursor()
        {
            if (SelectionArrow != null)
                SelectionArrow.gameObject.SafeSetActive(true);
        }

        public int GetCurrentSelection()
        {
            return (arrowY * ColumnCount + arrowX) + displayStart * ColumnCount;
        }

        private void Draw()
        {
            var path = ScrollHandler.GetPrefabPath();
            var cellSize = ScrollHandler.GetCellSize();
            displayRows = (int)(Content.rect.height / cellSize.y);
            var asset = ServiceManager.Get<AssetManager>().Load<ScrollViewCell>(path);
            numberOfRows = Mathf.Max(ScrollHandler.GetNumberOfCells() / ColumnCount, 1);
            var cells = displayRows * ColumnCount;

            for (int i = 0; i < cells; i++)
            {
                ScrollViewCell cell;
                if (!uiController.HasCell())
                    cell = Instantiate(asset);
                else
                    cell = uiController.GetCellFromPool();
                cell.gameObject.SafeSetActive(true);
                cell.transform.SetParent(Content, false);
                ScrollHandler.InitCell(i, cell);
                ScrollHandler.OnAfterLoad();
                this.cells.Add(cell);
            }
            LayoutGroup.CalculateLayoutInputHorizontal();
            LayoutGroup.CalculateLayoutInputVertical();
            LayoutGroup.SetLayoutHorizontal();
            LayoutGroup.SetLayoutVertical();
            SetSelectionPosition(true, true);
        }

        private void HandleUpMovement()
        {
            arrowY = Mathf.Max(arrowY - 1, 0);
            displayIndex = Mathf.Max(displayIndex - 1, 0);
            if (displayIndex < displayStart)
            {
                displayStart--;
                redraw = true;
            }
            SetSelectionPosition(false, true);
        }

        private void HandleDownMovement()
        {
            arrowY = Mathf.Min(arrowY + 1, displayRows - 1);
            displayIndex = Mathf.Min(displayIndex + 1, numberOfRows - 1);
            if (displayIndex >= displayStart + displayRows)
            {
                displayStart++;
                redraw = true;
            }
            SetSelectionPosition(false, true);
        }

        private void HandleLeftInput()
        {
            arrowX = Mathf.Max(arrowX - 1, 0);
            SetSelectionPosition(true, false);
        }

        private void HandleRightInput()
        {
            arrowX = Mathf.Min(arrowX + 1, ColumnCount - 1);
            SetSelectionPosition(true, false);
        }

        private void SetSelectionPosition(bool xAxis, bool yAxis)
        {
            int index = arrowY * ColumnCount + arrowX;
            var cellPosition = cells[index].transform.position;
            var cellSize = ScrollHandler.GetCellSize();
            var y = cellPosition.y;
            var x = cellPosition.x - cellSize.x * 0.33f;
            if (SelectionArrow != null)
                SelectionArrow.transform.position = new Vector2(x, y);
            OnChange?.Invoke(index + displayStart * ColumnCount);
        }
    }
}
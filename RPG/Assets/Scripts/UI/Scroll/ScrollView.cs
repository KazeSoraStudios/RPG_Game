﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ScrollDirection { Horizontal, Vertical }

public interface IScrollHandler
{
    string GetPrefabPath();
    void InitCell(int index, ScrollViewCell cell);
    Vector2 GetCellSize();
    void OnAfterLoad();
}

public class ScrollView : UIMonoBehaviour
{
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
    private int numberOfRows;
    private List<ScrollViewCell> cells = new List<ScrollViewCell>();
    private Action<int> OnChange;

    public void Init(IScrollHandler scrollHandler, Action<int> onChange)
    {
        if (CheckUIConfigAndLogError(scrollHandler, "Scrollview"))
            return;
        Reset();
        ScrollHandler = scrollHandler;
        OnChange = onChange;
        var path = ScrollHandler.GetPrefabPath();
        var asset = ServiceManager.Get<AssetManager>().Load<ScrollViewCell>(path);
        var cellSize = ScrollHandler.GetCellSize();
        displayRows = (int)(Content.rect.height / cellSize.y);
        numberOfRows = displayRows * ColumnCount;

        for (int i = 0; i < numberOfRows; i++)
        {
            var cell = Instantiate(asset, Content);
            ScrollHandler.InitCell(i, cell);
            scrollHandler.OnAfterLoad();
            cells.Add(cell);
        }
        LayoutGroup.CalculateLayoutInputHorizontal();
        LayoutGroup.CalculateLayoutInputVertical();
        LayoutGroup.SetLayoutHorizontal();
        LayoutGroup.SetLayoutVertical();
        SetSelectionPosition(true, true);
    }

    public void CleanUp()
    {
        for (int i = cells.Count - 1; i > -1; i--)
        {
            // TODO pooling
            cells[i].OnExit();
            Destroy(cells[i]);
        }
    }

    public void Execute()
    {
        if (!redraw)
            return;
        //currentRowIndex += movement * ColumnCount;
        for(int i = 0; i < cells.Count; i++)
        {
            int index = i + arrowY;
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

        if (ColumnCount < 2)
            return;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            HandleLeftInput();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            HandleRightInput();
    }

    private void HandleUpMovement()
    {
        arrowY = Mathf.Max(arrowY - 1, 0);
        if (arrowY < displayStart)
            displayStart--;
        SetSelectionPosition(false, true);
    }

    private void HandleDownMovement()
    {
        arrowY = Mathf.Min(arrowY + 1, displayRows - 1);
        if (arrowY >= displayStart + displayRows)
            displayStart++;
        SetSelectionPosition(false, true);
    }
    public float minus = 1.0f;

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
        int index = arrowY * 2 + arrowX;
        var cellPosition = cells[index].transform.position;
        var cellSize = ScrollHandler.GetCellSize();
        var y = cellPosition.y;
        var x = cellPosition.x - cellSize.x * minus;
        SelectionArrow.transform.position = new Vector2(x, y);
        OnChange?.Invoke(index);
    }

    private void Reset()
    {
        cells.Clear();
        arrowY = 0;
        arrowX = 0;
        displayRows = 0;
        redraw = true;
    }
}

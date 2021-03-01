using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuOptionsList : UIMonoBehaviour
{
    public class Config
    {
    }

    [SerializeField] float SelectionPadding;
    [SerializeField] Image SelectionArrow;
    [SerializeField] TextMeshProUGUI[] Options;

    private int numberOfOptions;
    public int currentSelection = 0;
    private Config config;

    public void Init(Config config)
    {
        if (CheckUIConfigAndLogError(config, name))
        {
            return;
        }

        this.config = config;
        numberOfOptions = Options.Length - 1;
        ShowCursor();
    }

    public void HideCursor()
    {
        SelectionArrow.gameObject.SetActive(false);
    }

    public void ShowCursor()
    {
        SelectionArrow.gameObject.SetActive(true);
    }

    public int GetSelection()
    {
        return currentSelection;
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
        var selectPosition = Options[currentSelection].transform.position;
        var y = Options[currentSelection].transform.position.y;
        var position = new Vector2(SelectionArrow.transform.position.x, y);
        SelectionArrow.transform.position = position;
    }
}

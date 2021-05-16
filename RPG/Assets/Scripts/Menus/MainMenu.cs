using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPG_GameState;

public class MainMenu : MonoBehaviour
{
    [SerializeField] Button NewGameButton;
    [SerializeField] Button ContinueButton;

    // Start is called before the first frame update
    void Start()
    {
        var gamesExist = ServiceManager.Get<GameStateManager>().GetNumberOfSaves() > 0;
        if (!gamesExist)
            ContinueButton.interactable = false;
    }
    
    public void NewGame()
    {
        ServiceManager.Get<GameLogic>().StartNewGame();
    }

    public void LoadGame()
    {
        ServiceManager.Get<GameLogic>().LoadGame();
    }
}

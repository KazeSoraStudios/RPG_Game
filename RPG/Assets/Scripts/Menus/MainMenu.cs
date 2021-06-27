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
        CheckForSaveData();
    }
    
    public void NewGame()
    {
        ServiceManager.Get<GameLogic>().StartNewGame();
    }

    public void LoadGame()
    {
        ServiceManager.Get<GameLogic>().LoadGame();
    }

    private void CheckForSaveData()
    {
        var gameManager = ServiceManager.Get<GameStateManager>();
        gameManager.LoadSavedGames();
        if (gameManager.GetNumberOfSaves() > 0)
        {
            gameManager.LoadGameStateData(0);
            ContinueButton.interactable = true;
        }
        else
        {
            ContinueButton.interactable = false;
        }
    }
}

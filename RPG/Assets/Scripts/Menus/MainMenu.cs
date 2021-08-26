using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPG_GameState;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject Credits;
    [SerializeField] GameObject Buttons;

    public void NewGame()
    {
        ServiceManager.Get<GameLogic>().StartNewGame();
    }

    public void LoadGame()
    {
        ServiceManager.Get<GameLogic>().LoadGame();
    }

    public void ShowCredits()
    {
        Credits.SafeSetActive(true);
        Buttons.SafeSetActive(false);
    }

    public void HideCredits()
    {
        Credits.SafeSetActive(false);
        Buttons.SafeSetActive(true);
    }

    // private void CheckForSaveData()
    // {
    //     var gameManager = ServiceManager.Get<GameStateManager>();
    //     gameManager.LoadSavedGames();
    //     if (gameManager.GetNumberOfSaves() > 0)
    //     {
    //         gameManager.LoadGameStateData(0);
    //         ContinueButton.interactable = true;
    //     }
    //     else
    //     {
    //         ContinueButton.interactable = false;
    //     }
    // }
}

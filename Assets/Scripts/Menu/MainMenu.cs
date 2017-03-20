using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using RTS;

public class MainMenu : Menu {

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    protected override void SetButtons()
    {
        buttons = new string[] { "Change Player", "Single Player", "Load Game", "Multi-Player Game", "Back to the Introduction", "Credits", "Quit" };
    }

    protected override void HandleButton(string text)
    {
        base.HandleButton(text);
        switch (text)
        {
            case "Change Player": ChangePlayer(); break;
            case "Single Player": NewGame(); break;
            case "Load Game": LoadGame(); break;
            case "Multi-Player Game": NewGame(); break;
            case "Back to the Introduction": NewGame(); break;
            case "Credits": NewGame(); break;
            case "Quit": ExitGame(); break;
            default: break;
        }
    }
    protected override void HideCurrentMenu()
    {
        GetComponent<MainMenu>().enabled = false;
    }
    private void NewGame()
    {
        ResourceManager.MenuOpen = false;
        //Application.LoadLevel("Map");
        SceneManager.LoadScene("Map");

        //makes sure that the loaded level runs at normal speed
        Time.timeScale = 1.0f;
    }
    private void ChangePlayer()
    {
        GetComponent<MainMenu>().enabled = false;
        GetComponent<SelectPlayerMenu>().enabled = true;
        SelectionList.LoadEntries(PlayerManager.GetPlayerNames());
    }
    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        Cursor.visible = true;
        if (PlayerManager.GetPlayerName() == "")
        {
            //no player yet selected so enable SetPlayerMenu
            GetComponent<MainMenu>().enabled = false;
            GetComponent<SelectPlayerMenu>().enabled = true;
        }
        else
        {
            //player selected so enable MainMenu
            GetComponent<MainMenu>().enabled = true;
            GetComponent<SelectPlayerMenu>().enabled = false;
        }
    }
}

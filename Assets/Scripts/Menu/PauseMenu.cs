using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using RTS;

public class PauseMenu : Menu {

    private Player player;

    protected override void Start()
    {
        base.Start();
        player = transform.root.GetComponent<Player>();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Resume();
    }

    protected override void SetButtons()
    {
        buttons = new string[] { "Load", "Save", "Mission Objective", "Abandon the Mission", "Restart Mission", "Back to the game" };
    }
    protected override void HandleButton(string text)
    {
        base.HandleButton(text);
        switch (text)
        {
            case "Load": LoadGame(); break;
            case "Save": SaveGame(); break;
            case "Mission Objective": Resume(); break;
            case "Abandon the Mission": ReturnToMainMenu(); break;
            case "Restart Mission": RestartLevel(); break;
            case "Back to the game": Resume(); break;
            default: break;
        }
    }
    protected override void HideCurrentMenu()
    {
        GetComponent<PauseMenu>().enabled = false;
    }

    private void Resume()
    {
        Time.timeScale = 1.0f;
        GetComponent<PauseMenu>().enabled = false;
        if (player) player.GetComponent<UserInput>().enabled = true;
        //Cursor.visible = false;
        ResourceManager.MenuOpen = false;
    }
    private void ReturnToMainMenu()
    {
        ResourceManager.LevelName = "";
        SceneManager.LoadScene("MainMenu");
        Cursor.visible = true;
    }
    private void RestartLevel()
    {
        SceneManager.LoadScene("Map");
        ResourceManager.MenuOpen = false;
        Time.timeScale = 1.0f;
    }
    private void SaveGame()
    {
        GetComponent<PauseMenu>().enabled = false;
        SaveMenu saveMenu = GetComponent<SaveMenu>();
        if (saveMenu)
        {
            saveMenu.enabled = true;
            saveMenu.Activate();
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    public Texture2D cursor;
    private GameObject pauseMenu;
    private GameObject settingsMenu;
    private GameObject currentMenuContext = null;
    public EventSystem eventSystem;
    private GameObject lastSelected = null;
    private GameState state;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        Time.timeScale = 0;
        eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        Cursor.SetCursor(cursor, Vector2.zero, CursorMode.ForceSoftware);
        pauseMenu = transform.Find("PauseVertContainer").gameObject;
        settingsMenu = transform.Find("SettingsMenuContainer").gameObject;
        currentMenuContext = pauseMenu;
        settingsMenu.GetComponent<RectTransform>().localScale = new Vector3(0, 0, 0);
        pauseMenu.transform.Find("ResumeButton").gameObject.GetComponent<Button>().Select();
        state = GameObject.Find("GameState").GetComponent<GameState>();
    }

    // Update is called once per frame
    void Update()
    {
        if (eventSystem.currentSelectedGameObject == null && lastSelected != null
            || (currentMenuContext == settingsMenu && eventSystem.currentSelectedGameObject.GetComponent<Button>().transform.parent == pauseMenu.transform)) {
            lastSelected.GetComponent<Button>().Select();
        } else if (eventSystem.currentSelectedGameObject != null) lastSelected = eventSystem.currentSelectedGameObject;
    }
    public void RetryClicked() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1;
        SceneManager.LoadSceneAsync("Castle");
    }
    public void QuitClicked() {
        Time.timeScale = 1;
        Game.Menu.SkipSplash = true;
        SceneManager.LoadSceneAsync("TitleScreen");
    }

    public void ControlsClicked() {
        settingsMenu.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        pauseMenu.GetComponent<RectTransform>().localScale = new Vector3(0, 0, 0);
        settingsMenu.transform.Find("InvertCameraXButton").gameObject.GetComponent<Button>().Select();
        currentMenuContext = settingsMenu;
    }

    public void ControlsBackClicked() {
        settingsMenu.GetComponent<RectTransform>().localScale = new Vector3(0, 0, 0);
        pauseMenu.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        pauseMenu.transform.Find("ControlsButton").gameObject.GetComponent<Button>().Select();
        currentMenuContext = pauseMenu;
    }

    public void ResumeClicked() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1;
        state.Unpause();
    }
}

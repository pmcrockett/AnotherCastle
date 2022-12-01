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
    private AudioSource menuClick;
    private AudioSource menuConfirm;
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
        state = GameObject.Find("GameState").GetComponent<GameState>();

        if (GameObject.Find("MenuClickSound") != null) {
            menuClick = GameObject.Find("MenuClickSound").GetComponent<AudioSource>();
        }
        if (GameObject.Find("MenuConfirmSound") != null) {
            menuConfirm = GameObject.Find("MenuConfirmSound").GetComponent<AudioSource>();
        }

        pauseMenu.transform.Find("ResumeButton").gameObject.GetComponent<Button>().Select();
        if (menuClick != null) menuClick.Play();
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
        if (menuConfirm != null) menuConfirm.Play();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1;
        SceneManager.LoadSceneAsync("Castle");
    }
    public void QuitClicked() {
        if (menuConfirm != null) menuConfirm.Play();
        Time.timeScale = 1;
        Game.Menu.SkipSplash = true;
        SceneManager.LoadSceneAsync("TitleScreen");
    }

    public void ControlsClicked() {
        if (menuConfirm != null) menuConfirm.Play();
        settingsMenu.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        pauseMenu.GetComponent<RectTransform>().localScale = new Vector3(0, 0, 0);
        settingsMenu.transform.Find("InvertCameraXButton").gameObject.GetComponent<Button>().Select();
        currentMenuContext = settingsMenu;
    }

    public void ControlsBackClicked() {
        if (menuConfirm != null) menuConfirm.Play();
        settingsMenu.GetComponent<RectTransform>().localScale = new Vector3(0, 0, 0);
        pauseMenu.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        pauseMenu.transform.Find("ControlsButton").gameObject.GetComponent<Button>().Select();
        currentMenuContext = pauseMenu;
    }

    public void ResumeClicked() {
        if (menuConfirm != null) menuConfirm.Play();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1;
        state.Unpause();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class GameOverMenu : MonoBehaviour
{
    public Texture2D cursor;
    public EventSystem eventSystem;
    private GameObject lastSelected = null;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        Cursor.SetCursor(cursor, Vector2.zero, CursorMode.ForceSoftware);
        transform.Find("RetryButton").gameObject.GetComponent<Button>().Select();
    }

    // Update is called once per frame
    void Update()
    {
        if (eventSystem.currentSelectedGameObject == null && lastSelected != null) {
            lastSelected.GetComponent<Button>().Select();
        } else if (eventSystem.currentSelectedGameObject != null) lastSelected = eventSystem.currentSelectedGameObject;
    }

    public void RetryClicked() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        SceneManager.LoadSceneAsync("Castle");
    }
    public void QuitClicked() {
        Game.Menu.SkipSplash = true;
        SceneManager.LoadSceneAsync("TitleScreen");
    }
}

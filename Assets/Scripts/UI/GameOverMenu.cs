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
    private AudioSource menuClick;
    private AudioSource menuConfirm;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        Cursor.SetCursor(cursor, Vector2.zero, CursorMode.ForceSoftware);
        if (GameObject.Find("MenuClickSound") != null) {
            menuClick = GameObject.Find("MenuClickSound").GetComponent<AudioSource>();
        }
        if (GameObject.Find("MenuConfirmSound") != null) {
            menuConfirm = GameObject.Find("MenuConfirmSound").GetComponent<AudioSource>();
        }
        if (transform.Find("RetryButton") != null) {
            transform.Find("RetryButton").gameObject.GetComponent<Button>().Select();
        } else if (transform.Find("QuitButton") != null) {
            transform.Find("QuitButton").gameObject.GetComponent<Button>().Select();
        }
        if (menuClick != null) menuClick.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (eventSystem.currentSelectedGameObject == null && lastSelected != null) {
            lastSelected.GetComponent<Button>().Select();
        } else if (eventSystem.currentSelectedGameObject != null) lastSelected = eventSystem.currentSelectedGameObject;
    }

    public void RetryClicked() {
        if (menuConfirm != null) menuConfirm.Play();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        eventSystem.currentSelectedGameObject.GetComponent<Button>().transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text += " (Loading...)";
        SceneManager.LoadScene("Castle");
    }
    public void QuitClicked() {
        if (menuConfirm != null) menuConfirm.Play();
        Game.Menu.SkipSplash = true;
        SceneManager.LoadScene("TitleScreen");
    }
}

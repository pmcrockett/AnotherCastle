using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Unity.VectorGraphics;
using System.Text.RegularExpressions;

namespace Controls {
    public class Axis {
        public static bool InvertCameraX { get; set; }
        public static bool InvertCameraY { get; set; }
        public static bool InvertAimX { get; set; }
        public static bool InvertAimY { get; set; }
    }
}

public class MainMenu : MonoBehaviour
{
    public Texture2D cursor;
    public Camera menuCamera;
    public EventSystem eventSystem;
    private GameObject horiz;
    private GameObject main;
    private GameObject settings;
    private GameObject controls;
    private GameObject title;
    private GameObject about;
    private GameObject currentMenuContext = null;
    private GameObject lastSelected;
    private GameObject spacer;
    private float timer;

    private void Awake() {
        horiz = transform.Find("HorizContainer").gameObject;
        main = horiz.transform.Find("MainMenuContainer").gameObject;
        settings = horiz.transform.Find("SettingsMenuContainer").gameObject;
        controls = transform.Find("Controls").gameObject;
        title = transform.Find("Title").gameObject;
        about = transform.Find("About").gameObject;
        spacer = horiz.transform.Find("Spacer").gameObject;
        Cursor.SetCursor(cursor, Vector2.zero, CursorMode.ForceSoftware);

        //currentMenuContext = main;
        settings.GetComponent<RectTransform>().localScale = new Vector3(0, 0, 0);
        controls.GetComponent<SVGImage>().enabled = false;
        title.GetComponent<Image>().enabled = false;
        main.GetComponent<RectTransform>().localScale = new Vector3(0, 0, 0);
        //main.transform.Find("StartButton").gameObject.GetComponent<Button>().Select();
    }
    // Start is called before the first frame update
    void Start()
    {
        timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > 6) {
            timer = -1;
            about.GetComponent<TextMeshProUGUI>().enabled = false;
            InitMain();
        } else if (timer > -1){
            timer += Time.deltaTime;
        }
        if (eventSystem.currentSelectedGameObject == null && lastSelected != null 
            || (currentMenuContext == settings && eventSystem.currentSelectedGameObject.GetComponent<Button>().transform.parent == main.transform)) {
            lastSelected.GetComponent<Button>().Select();
        } else if (eventSystem.currentSelectedGameObject != null) lastSelected = eventSystem.currentSelectedGameObject;
    }

    public void StartClicked() {
        if (currentMenuContext == main) SceneManager.LoadSceneAsync("Castle");
    }

    public void ExitClicked() {
        if(currentMenuContext == main) Application.Quit();
    }

    public void SettingsClicked() {
        if (currentMenuContext == main) {
            currentMenuContext = settings;
            settings.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            controls.GetComponent<SVGImage>().enabled = true;
            title.GetComponent<Image>().enabled = false;
            Color col = main.transform.Find("StartButton").gameObject.GetComponentInChildren<Image>().color;
            main.transform.Find("StartButton").gameObject.GetComponentInChildren<Image>().color = new Color(col.r, col.g, col.b, 0);
            main.transform.Find("SettingsButton").gameObject.GetComponentInChildren<Image>().color = new Color(col.r, col.g, col.b, 0);
            main.transform.Find("ExitButton").gameObject.GetComponentInChildren<Image>().color = new Color(col.r, col.g, col.b, 0);
            main.transform.Find("StartButton").gameObject.GetComponentInChildren<TextMeshProUGUI>().color = new Color(col.r, col.g, col.b, 0.3f);
            main.transform.Find("SettingsButton").gameObject.GetComponentInChildren<TextMeshProUGUI>().color = new Color(col.r, col.g, col.b, 0.3f);
            main.transform.Find("ExitButton").gameObject.GetComponentInChildren<TextMeshProUGUI>().color = new Color(col.r, col.g, col.b, 0.3f);
            settings.transform.Find("InvertCameraXButton").gameObject.GetComponent<Button>().Select();
            spacer.GetComponent<Image>().enabled = true;
        }
    }

    public void InitMain() {
        main.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        if (currentMenuContext == settings) {
            main.transform.Find("SettingsButton").gameObject.GetComponent<Button>().Select();
        } else {
            main.transform.Find("StartButton").gameObject.GetComponent<Button>().Select();
        }
        currentMenuContext = main;
        settings.GetComponent<RectTransform>().localScale = new Vector3(0, 0, 0);
        controls.GetComponent<SVGImage>().enabled = false;
        title.GetComponent<Image>().enabled = true;
        Color col = main.transform.Find("StartButton").gameObject.GetComponentInChildren<Image>().color;
        main.transform.Find("StartButton").gameObject.GetComponentInChildren<Image>().color = new Color(col.r, col.g, col.b, 1);
        main.transform.Find("SettingsButton").gameObject.GetComponentInChildren<Image>().color = new Color(col.r, col.g, col.b, 1);
        main.transform.Find("ExitButton").gameObject.GetComponentInChildren<Image>().color = new Color(col.r, col.g, col.b, 1);
        main.transform.Find("StartButton").gameObject.GetComponentInChildren<TextMeshProUGUI>().color = new Color(col.r, col.g, col.b, 1);
        main.transform.Find("SettingsButton").gameObject.GetComponentInChildren<TextMeshProUGUI>().color = new Color(col.r, col.g, col.b, 1);
        main.transform.Find("ExitButton").gameObject.GetComponentInChildren<TextMeshProUGUI>().color = new Color(col.r, col.g, col.b, 1);
        spacer.GetComponent<Image>().enabled = false;
    }

    public bool ToggleButtonState(TextMeshProUGUI text) {
        Regex noState = new Regex(@"\(No\)$");
        Regex yesState = new Regex(@"\(Yes\)$");
        Regex openState = new Regex(@"      >>>$");
        if (noState.IsMatch(text.text)) {
            text.text = noState.Replace(text.text, "(Yes)");
            return true;
        } else {
            text.text = yesState.Replace(text.text, "(No)");
            return false;
        }
    }

    public void InvertCameraXButtonClicked() {
        Controls.Axis.InvertCameraX = ToggleButtonState(settings.transform.Find("InvertCameraXButton").gameObject.GetComponentInChildren<TextMeshProUGUI>());
    }

    public void InvertCameraYButtonClicked() {
        Controls.Axis.InvertCameraY = ToggleButtonState(settings.transform.Find("InvertCameraYButton").gameObject.GetComponentInChildren<TextMeshProUGUI>());
    }

    public void InvertAimXButtonClicked() {
        Controls.Axis.InvertAimX = ToggleButtonState(settings.transform.Find("InvertAimXButton").gameObject.GetComponentInChildren<TextMeshProUGUI>());
    }

    public void InvertAimYButtonClicked() {
        Controls.Axis.InvertAimY = ToggleButtonState(settings.transform.Find("InvertAimYButton").gameObject.GetComponentInChildren<TextMeshProUGUI>());
    }
}

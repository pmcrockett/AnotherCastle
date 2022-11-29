using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Unity.VectorGraphics;
using System.Text.RegularExpressions;

namespace Game {
    public class Axis {
        public static bool InvertCameraX { get; set; }
        public static bool InvertCameraY { get; set; }
        public static bool InvertAimX { get; set; }
        public static bool InvertAimY { get; set; }
    }

    public class PlayerState {
        public static string Checkpoint { get; set; }
        public static bool PlayIntro { get; set; }
        public static int Deaths { get; set;}
        public static string HeroName { get; set; }
        public static bool Hookshot { get; set; }
        public static bool Jump { get; set; }
        public static bool Lift { get; set; }
        public static bool Sword { get; set; }
    }

    public class Menu {
        public static bool SkipSplash {  get; set; }

        public static TMP_FontAsset GameFont { get; set; }
    }
}

public class MainMenu : MonoBehaviour
{
    public Texture2D cursor;
    public Camera menuCamera;
    public EventSystem eventSystem;
    public TMP_FontAsset gameFont;
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
        Game.Menu.GameFont = gameFont;
    }
    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        timer = 0;
        Game.PlayerState.Checkpoint = null;
        Game.PlayerState.PlayIntro = true;
        Game.PlayerState.Deaths = 0;
        Game.PlayerState.HeroName = "";
        Game.PlayerState.Hookshot = false;
        Game.PlayerState.Jump = false;
        Game.PlayerState.Lift = false;
        Game.PlayerState.Sword = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > 6 || Game.Menu.SkipSplash) {
            timer = -1;
            Game.Menu.SkipSplash = false;
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
}

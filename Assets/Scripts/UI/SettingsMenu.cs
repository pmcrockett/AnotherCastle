using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Unity.VectorGraphics;
using System.Text.RegularExpressions;

public class SettingsMenu : MonoBehaviour
{
    private AudioSource menuClick;
    private AudioSource menuConfirm;
    // Start is called before the first frame update
    void Start()
    {
        if (Game.Axis.InvertCameraX) ToggleButtonState(transform.Find("InvertCameraXButton").gameObject.GetComponentInChildren<TextMeshProUGUI>());
        if (Game.Axis.InvertCameraY) ToggleButtonState(transform.Find("InvertCameraYButton").gameObject.GetComponentInChildren<TextMeshProUGUI>());
        if (Game.Axis.InvertAimX) ToggleButtonState(transform.Find("InvertAimXButton").gameObject.GetComponentInChildren<TextMeshProUGUI>());
        if (Game.Axis.InvertAimY) ToggleButtonState(transform.Find("InvertAimYButton").gameObject.GetComponentInChildren<TextMeshProUGUI>());

        if (GameObject.Find("MenuClickSound") != null) {
            menuClick = GameObject.Find("MenuClickSound").GetComponent<AudioSource>();
        }
        if (GameObject.Find("MenuConfirmSound") != null) {
            menuConfirm = GameObject.Find("MenuConfirmSound").GetComponent<AudioSource>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public bool ToggleButtonState(TextMeshProUGUI text) {
        Regex noState = new Regex(@"\(No\)$");
        Regex yesState = new Regex(@"\(Yes\)$");
        if (noState.IsMatch(text.text)) {
            text.text = noState.Replace(text.text, "(Yes)");
            return true;
        } else {
            text.text = yesState.Replace(text.text, "(No)");
            return false;
        }
    }

    public void InvertCameraXButtonClicked() {
        if (menuConfirm != null) menuConfirm.Play();
        Game.Axis.InvertCameraX = ToggleButtonState(transform.Find("InvertCameraXButton").gameObject.GetComponentInChildren<TextMeshProUGUI>());
    }

    public void InvertCameraYButtonClicked() {
        if (menuConfirm != null) menuConfirm.Play();
        Game.Axis.InvertCameraY = ToggleButtonState(transform.Find("InvertCameraYButton").gameObject.GetComponentInChildren<TextMeshProUGUI>());
    }

    public void InvertAimXButtonClicked() {
        if (menuConfirm != null) menuConfirm.Play();
        Game.Axis.InvertAimX = ToggleButtonState(transform.Find("InvertAimXButton").gameObject.GetComponentInChildren<TextMeshProUGUI>());
    }

    public void InvertAimYButtonClicked() {
        if (menuConfirm != null) menuConfirm.Play();
        Game.Axis.InvertAimY = ToggleButtonState(transform.Find("InvertAimYButton").gameObject.GetComponentInChildren<TextMeshProUGUI>());
    }
}

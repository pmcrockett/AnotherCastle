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
    // Start is called before the first frame update
    void Start()
    {
        if (Game.Axis.InvertCameraX) ToggleButtonState(transform.Find("InvertCameraXButton").gameObject.GetComponentInChildren<TextMeshProUGUI>());
        if (Game.Axis.InvertCameraY) ToggleButtonState(transform.Find("InvertCameraYButton").gameObject.GetComponentInChildren<TextMeshProUGUI>());
        if (Game.Axis.InvertAimX) ToggleButtonState(transform.Find("InvertAimXButton").gameObject.GetComponentInChildren<TextMeshProUGUI>());
        if (Game.Axis.InvertAimY) ToggleButtonState(transform.Find("InvertAimYButton").gameObject.GetComponentInChildren<TextMeshProUGUI>());
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
        Game.Axis.InvertCameraX = ToggleButtonState(transform.Find("InvertCameraXButton").gameObject.GetComponentInChildren<TextMeshProUGUI>());
    }

    public void InvertCameraYButtonClicked() {
        Game.Axis.InvertCameraY = ToggleButtonState(transform.Find("InvertCameraYButton").gameObject.GetComponentInChildren<TextMeshProUGUI>());
    }

    public void InvertAimXButtonClicked() {
        Game.Axis.InvertAimX = ToggleButtonState(transform.Find("InvertAimXButton").gameObject.GetComponentInChildren<TextMeshProUGUI>());
    }

    public void InvertAimYButtonClicked() {
        Game.Axis.InvertAimY = ToggleButtonState(transform.Find("InvertAimYButton").gameObject.GetComponentInChildren<TextMeshProUGUI>());
    }
}

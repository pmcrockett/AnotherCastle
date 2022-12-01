using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MouseOver : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler {
    private AudioSource menuClick;
    // Start is called before the first frame update
    void Start() {
        if (GameObject.Find("MenuClickSound") != null) {
            menuClick = GameObject.Find("MenuClickSound").GetComponent<AudioSource>();
        }
    }
    public void OnPointerEnter(PointerEventData eventData) {
        GetComponent<Button>().Select();
    }
    public void OnSelect(BaseEventData eventData) {
        if (menuClick != null) menuClick.Play();
    }

    public void OnPointerExit(PointerEventData eventData) {
        
    }
}

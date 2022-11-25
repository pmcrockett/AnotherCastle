using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MouseOver : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    // Start is called before the first frame update
    public void OnPointerEnter(PointerEventData eventData) {
        GetComponent<Button>().Select();
    }

    public void OnPointerExit(PointerEventData eventData) {
        
    }
}

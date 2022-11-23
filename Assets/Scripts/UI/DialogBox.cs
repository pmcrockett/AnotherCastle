using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogBox : MonoBehaviour
{
    public List<string> dialog = new List<string>();
    public List<Camera> cameras = new List<Camera>();
    public int activeIdx = -1;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool DisplayNextDialog() {
        activeIdx++;
        if (activeIdx - 1 >= 0 && cameras[activeIdx - 1] != null) {
            DeactivateCamera(activeIdx - 1);
        }
        if (activeIdx < dialog.Count) {
            if (activeIdx < cameras.Count && cameras[activeIdx] != null) {
                ActivateCamera(activeIdx);
            }
            GetComponent<TextMeshProUGUI>().text = Util.FixNewline(dialog[activeIdx]);
            return true;
        } else return false;
    }

    private void ActivateCamera(int _idx) {
        cameras[_idx].enabled = true;
        cameras[_idx].depth = 1;
    }

    private void DeactivateCamera(int _idx) {
        cameras[_idx].enabled = false;
        cameras[_idx].depth = 0;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class DialogTrigger : MonoBehaviour
{
    public Canvas canvas;
    public GameObject triggeringObject;
    public int maxUses = 1;
    public List<string> dialogText;
    public List<Camera> cameras;
    public int uses = 0;

    private void Awake() {
        //cameras[1].enabled = false;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject == triggeringObject && uses < maxUses) {
            if (Game.PlayerState.HeroName != null) {
                Regex heroRegex = new Regex(@"\[hero\]");
                foreach (Camera x in cameras) if (x != null) x.enabled = false;
                for (int i = 0; i < dialogText.Count; i++) {
                    dialogText[i] = heroRegex.Replace(dialogText[i], Game.PlayerState.HeroName);
                    Debug.Log(dialogText[i]);
                }
            }
            triggeringObject.GetComponent<InputHandler>().activeDialog = Util.CreateDialogBox(canvas, dialogText, cameras);
            uses++;
        }
    }
}

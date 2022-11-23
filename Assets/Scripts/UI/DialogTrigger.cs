using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogTrigger : MonoBehaviour
{
    public Canvas canvas;
    public GameObject triggeringObject;
    public int maxUses = 1;
    public List<string> dialogText;
    public List<Camera> cameras;
    public int uses = 0;

    private void Awake() {
        foreach (Camera x in cameras) if (x != null) x.enabled = false;
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
            triggeringObject.GetComponent<InputHandler>().activeDialog = Util.CreateDialogBox(canvas, dialogText, cameras);
            uses++;
        }
    }
}

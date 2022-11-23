using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Treasure : MonoBehaviour
{
    public List<string> dialog = new List<string>();
    public List<Camera> cameras = new List<Camera>();
    public string contents;
    public bool isOpen = false;
    private PlayerInput input;
    private Animation anim;
    // Start is called before the first frame update
    private void Awake() {
        input = new PlayerInput();
        anim = GetComponent<Animation>();
    }
    void OnEnable() {
        input.Player.Enable();
    }

    void OnDisable() {
        input.Player.Disable();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isOpen && anim.IsPlaying("Closed")) anim.Play("Opened");
    }

    private void OnTriggerStay(Collider other) {
        if (!isOpen && other.gameObject.GetComponent<InputHandler>() != null && Vector3.Dot(transform.forward, other.gameObject.transform.forward) < 0) {
            float treasureInput = input.Player.Interact.ReadValue<float>();
            if (treasureInput > 0) {
                anim.Play("Opening");
                anim.PlayQueued("Opened");
                isOpen = true;
                foreach(BoxCollider x in GetComponents<BoxCollider>()) {
                    if (!x.isTrigger) {
                        x.center = new Vector3(0, -0.13f, 0);
                        x.size = new Vector3(1, 0.63f, 1);
                    }
                }
                other.gameObject.GetComponent<InputHandler>().activeDialog = Util.CreateDialogBox(Object.FindObjectOfType<Canvas>(), dialog, cameras);
                GiveTreasure(other.gameObject);
            }
        }
    }
    private void GiveTreasure(GameObject _opener) {
        if (contents == "Jump") _opener.GetComponent<InputHandler>().jumpEnabled = true;
        else if (contents == "Hookshot") _opener.GetComponent<Hookshot>().isEnabled = true;
        else if (contents == "LiftGlove") _opener.GetComponent<LiftGlove>().isEnabled = true;
        else if (contents == "Attack") _opener.GetComponent<Attack>().isEnabled = true;
    }
}

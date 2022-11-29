using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VectorGraphics;

public class AbilitiesDisplay : MonoBehaviour
{
    public SVGImage hookshot;
    public SVGImage jump;
    public SVGImage lift;
    public SVGImage sword;

    // Start is called before the first frame update
    void Start()
    {
        Redraw();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Redraw() {
        if (Game.PlayerState.Hookshot && transform.Find("Hookshot") == null) {
            Instantiate(hookshot, transform).gameObject.name = "Hookshot";
        }
        if (Game.PlayerState.Jump && transform.Find("Jump") == null) {
            Instantiate(jump, transform).gameObject.name = "Jump";
        }
        if (Game.PlayerState.Lift && transform.Find("Lift") == null) {
            Instantiate(lift, transform).gameObject.name = "Lift";
        }
        if (Game.PlayerState.Sword && transform.Find("Sword") == null) {
            Instantiate(sword, transform).gameObject.name = "Sword";
        }
    }
}

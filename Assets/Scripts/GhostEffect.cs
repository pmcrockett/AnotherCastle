using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostEffect : MonoBehaviour {
    public float fadeSpeed = 1;
    public bool isGhost = false;
    public bool isTransparent = false;
    private float alphaLerp = 1;
    private float alphaTarget = 1;
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (isGhost) {
            if (!isTransparent) MakeTransparent();
            if (alphaLerp > alphaTarget) {
                SetAlpha(alphaLerp);
                alphaLerp -= fadeSpeed * Time.deltaTime;
            } else if (alphaLerp < alphaTarget) {
                alphaLerp = alphaTarget;
                SetAlpha(alphaLerp);
            }
        } else if (!isGhost) {
            if (isTransparent) MakeNontransparent();
            if (alphaLerp < alphaTarget) {
                SetAlpha(alphaLerp);
                alphaLerp += fadeSpeed * Time.deltaTime;
            } else if (alphaLerp > alphaTarget) {
                alphaLerp = alphaTarget;
                SetAlpha(alphaLerp);
            }
        }
        isGhost = false;
    }

    public void MakeTransparent() {
        alphaTarget = 0.5f;
        isTransparent = true;
    }

    public void MakeNontransparent() {
        alphaTarget = 1;
        isTransparent = false;
    }

    public void SetAlpha(float _alpha) {
        Color color;
        foreach (MeshRenderer x in GetComponentsInChildren<MeshRenderer>()) {
            color = x.material.color;
            color.a = _alpha;
            x.material.color = color;
        }
        foreach (SkinnedMeshRenderer x in GetComponentsInChildren<SkinnedMeshRenderer>()) {
            color = x.material.color;
            color.a = _alpha;
            x.material.color = color;
        }
        Debug.Log("Alpha is now " + _alpha);
    }
}

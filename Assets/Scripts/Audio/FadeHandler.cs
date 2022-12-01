using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeHandler : MonoBehaviour
{
    public bool isFadingOut = false;
    public float fadeSpeed = 1;
    private float prefadeVol;
    private AudioSource source;
    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isFadingOut) {
            if (source.volume > 0) {
                source.volume -= fadeSpeed * Time.deltaTime;
            } else {
                source.volume = 0;
                source.Stop();
                source.volume = prefadeVol;
                isFadingOut = false;
            }
        }
    }

    public void FadeAudio() {
        isFadingOut = true;
        prefadeVol = source.volume;
    }
}

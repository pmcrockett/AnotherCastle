using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footstep : MonoBehaviour {
    private AudioSource footstepLSound;
    private AudioSource footstepRSound;

    private void Awake() {
        footstepLSound = transform.Find("FootstepL").GetComponent<AudioSource>();
        footstepRSound = transform.Find("FootstepR").GetComponent<AudioSource>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FootstepL() {
        footstepLSound.GetComponent<RandomRobin>().RefreshClip();
        footstepLSound.Play();
    }
    
    public void FootstepR() {
        footstepRSound.GetComponent<RandomRobin>().RefreshClip();
        footstepRSound.Play();
    }
}

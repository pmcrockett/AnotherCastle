using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicTrigger : MonoBehaviour
{
    public AudioClip music;
    public GameObject triggeringObj;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject == triggeringObj) {
            if (GameObject.Find("MusicContainer") != null
                && GameObject.Find("MusicContainer").GetComponent<AudioSource>().clip != music 
                || !GameObject.Find("MusicContainer").GetComponent<AudioSource>().isPlaying) {
                GameObject.Find("MusicContainer").GetComponent<AudioSource>().clip = music;
                GameObject.Find("MusicContainer").GetComponent<AudioSource>().Play();
            }
        }
    }
}

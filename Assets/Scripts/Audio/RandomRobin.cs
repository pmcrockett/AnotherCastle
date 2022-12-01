using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomRobin : MonoBehaviour
{
    public List<AudioClip> clips = new List<AudioClip>();
    private AudioSource source;

    private void Awake() {
        source = GetComponent<AudioSource>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RefreshClip() {
        GetComponent<AudioSource>().clip = clips[Random.Range(0, clips.Count)];
    }
}

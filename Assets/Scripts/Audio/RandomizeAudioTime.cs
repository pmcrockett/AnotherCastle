using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomizeAudioTime : MonoBehaviour
{
    private AudioSource source;

    private void Awake() {
        source = GetComponent<AudioSource>();
    }
    // Start is called before the first frame update
    void Start()
    {
        RandomizeTime();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RandomizeTime() {
        source.time = UnityEngine.Random.Range(0, source.clip.length);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatformAudio : MonoBehaviour
{
    public bool isMoving = false;
    private Vector3 lastPosition;
    // Start is called before the first frame update
    void Start()
    {
        lastPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position != lastPosition && !isMoving) {
            isMoving = true;
            transform.Find("MovingPlatformSound").GetComponent<AudioSource>().Play();
        } else if (transform.position == lastPosition && isMoving) {
            isMoving = false;
            transform.Find("MovingPlatformSound").GetComponent<FadeHandler>().FadeAudio();
        }
        lastPosition = transform.position;
    }
}

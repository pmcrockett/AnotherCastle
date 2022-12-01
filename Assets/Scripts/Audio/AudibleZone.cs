using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudibleZone : MonoBehaviour
{
    public float adjustSpeed = 5;
    public GameObject triggeringObj;
    private bool isAudible = false;
    private AudioSource source;
    private float targetVol;
    private float volLerp;
    private void Awake() {
        source = GetComponent<AudioSource>();
        targetVol = source.volume;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!isAudible && volLerp > 0) {
            volLerp = Mathf.Clamp(volLerp - adjustSpeed * Time.deltaTime, 0, 1);
        } else if (isAudible && volLerp < 1) {
            volLerp = Mathf.Clamp(volLerp + adjustSpeed * Time.deltaTime, 0, 1);
        }
        source.volume = Mathf.Lerp(0, targetVol, volLerp);
    }

    private void OnTriggerEnter(Collider other) {
        Debug.Log("In audible zone: " + other.name);
        if (other.gameObject == triggeringObj) {
            isAudible = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (isAudible && other.gameObject == triggeringObj) {
            isAudible = false;
        }
    }
}

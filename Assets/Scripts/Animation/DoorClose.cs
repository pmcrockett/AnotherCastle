using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorClose : MonoBehaviour
{
    public Quaternion closedRotation;
    public float speed;
    public bool isClosing = false;
    public bool isClosed = false;
    private Quaternion startRot;
    private float lerpVal;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isClosing && !isClosed) {
            lerpVal += speed * Time.deltaTime;
            //transform.rotation = Quaternion.RotateTowards(transform.rotation, closedRotation, 45);
            transform.rotation = Quaternion.Lerp(startRot, closedRotation, lerpVal);
            Debug.Log(transform.rotation);
        }
        if (transform.rotation == closedRotation) {
            isClosed = true;
            isClosing = false;
            lerpVal = 1;
        }
    }

    public void Close() {
        isClosing = true;
        startRot = transform.rotation;
        lerpVal = 0;
    }
}

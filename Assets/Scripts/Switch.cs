using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : MonoBehaviour
{
    public GameObject switchTarget;
    public Vector3 switchTargetTriggeredPosition;
    public Vector3 switchTargetTriggeredRotation;
    public float triggerSpeed = 5;
    public float targetMoveSpeed = 2;
    public bool permanentTrigger = false;
    public bool isTriggered = false;
    private Vector3 switchTargetInitialPosition;
    private Quaternion switchTargetInitialQuat;
    private Quaternion switchTargetTriggeredQuat;
    private Vector3 initialPosition;
    private Vector3 triggeredPosition;
    private List<GameObject> triggerList = new List<GameObject>();
    private float switchLerp = 0;
    private float switchTargetLerp = 0;

    // Start is called before the first frame update
    void Start()
    {
        switchTargetInitialPosition = switchTarget.transform.position;
        switchTargetInitialQuat = switchTarget.transform.rotation;
        if (switchTarget.transform.parent != null) {
            switchTargetTriggeredQuat = Quaternion.Euler(switchTarget.transform.parent.rotation.eulerAngles + switchTargetTriggeredRotation);
            switchTargetTriggeredPosition = switchTarget.transform.parent.position + switchTargetTriggeredPosition;
        } else {
            switchTargetTriggeredQuat = Quaternion.Euler(switchTargetTriggeredRotation);
        }
        initialPosition = transform.position;
        triggeredPosition = transform.position + new Vector3(0, -0.2f, 0);
        Debug.Log("switchTargetTriggeredPosition: " + switchTargetTriggeredPosition);
    }

    // Update is called once per frame
    void Update()
    {
        if (isTriggered && switchLerp < 1) {
            switchLerp = LerpSwitch(switchLerp, "+");
        } else if (isTriggered && switchLerp >= 1 && switchTargetLerp < 1) {
            switchTargetLerp = LerpTarget(switchTargetLerp, "+");
        } else if (!isTriggered && switchLerp > 0) {
            switchLerp = LerpSwitch(switchLerp, "-");
        } else if (!isTriggered && switchLerp <= 0 && switchTargetLerp > 0) {
            switchTargetLerp = LerpTarget(switchTargetLerp, "-");
        }
    }
    private float LerpSwitch(float _lerpVal, string _lerpDir) {
        transform.position = Vector3.Lerp(initialPosition, triggeredPosition, _lerpVal);
        if (_lerpDir == "+") return _lerpVal + triggerSpeed * Time.deltaTime;
        else return _lerpVal - triggerSpeed * Time.deltaTime;
    }
    private float LerpTarget(float _lerpVal, string _lerpDir) {
        switchTarget.transform.rotation = Quaternion.Lerp(switchTargetInitialQuat, switchTargetTriggeredQuat, _lerpVal);
        switchTarget.transform.position = Vector3.Lerp(switchTargetInitialPosition, switchTargetTriggeredPosition, _lerpVal);
        if (_lerpDir == "+") return _lerpVal + targetMoveSpeed * Time.deltaTime;
        else return _lerpVal - targetMoveSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.GetComponent<SwitchTrigger>() != null) {
            isTriggered = true;
            triggerList.Add(other.gameObject);
            transform.Find("SwitchDown").GetComponent<AudioSource>().Play();
        }
    }
    private void OnTriggerExit(Collider other) {
        triggerList.Remove(other.gameObject);
        if (triggerList.Count <= 0 && !permanentTrigger) {
            isTriggered = false;
            transform.Find("SwitchUp").GetComponent<AudioSource>().Play();
        }
    }
}

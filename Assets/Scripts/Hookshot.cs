using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hookshot : MonoBehaviour
{
    public Camera playerCam;
    public float hookshotLength = 20;
    public float reelSpeed = 4;
    public float aimSpeedDenominator = 10;
    public PlayerInput input;
    public bool isMoving = false;
    public Vector3 aimTarget;
    private Vector3 moveTarget;
    private Vector3 startPosition;
    private float lerpVal = 0f;
    private bool inputHeld = false;

    void Awake() {
        input = new PlayerInput();
    }

    void OnEnable() {
        input.Player.Enable();
    }

    void OnDisable() {
        input.Player.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate() {
        fire();

        if (isMoving) {
            lerpVal += reelSpeed * Time.deltaTime;
            if (lerpVal >= 1) {
                lerpVal = 0;
                isMoving = false;
            } else {
                transform.position = Vector3.Lerp(startPosition, moveTarget, lerpVal);
                GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
            }
        }
    }

    private void fire() {
        float fireInput = input.Player.Hookshot.ReadValue<float>();
        RaycastHit hookshotHit = new RaycastHit();
        if (fireInput > 0) {
            if (!isMoving
                && Physics.Raycast(transform.position, aimTarget - transform.position, out hookshotHit, hookshotLength, 0b10000000, QueryTriggerInteraction.Ignore) 
                && !inputHeld) {
                Debug.Log("Hookshot hit something");
                HookshotTarget hitTarget = hookshotHit.collider.GetComponent<HookshotTarget>();
                if (hitTarget != null) {
                    moveTarget = hookshotHit.point + new Vector3(0, GetComponent<CapsuleCollider>().height / 2 + 0.1f, 0);
                    startPosition = transform.position;
                    isMoving = true;
                    Debug.Log("Hookshot hit a target");
                }
                Debug.DrawLine(transform.position, aimTarget, Color.red, 3);
            }
            inputHeld = true;
        } else {
            inputHeld = false;
        }
    }
}

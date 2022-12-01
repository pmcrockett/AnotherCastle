using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hookshot : MonoBehaviour
{
    public Camera playerCam;
    public GameObject target;
    public Material yesMat;
    public Material noMat;
    public GameObject point;
    public GameObject chain;
    public float hookshotLength = 20;
    public float reelSpeed = 4;
    public float aimSpeedDenominator = 10;
    public bool isAiming = false;
    public bool isEnabled = false;
    public PlayerInput input;
    public bool isMoving = false;
    public Vector3 aimTarget;
    private Vector3 moveTarget;
    private Vector3 startPosition;
    private float lerpVal = 0f;
    private bool inputHeld = false;
    private GameObject targetInstance = null;
    private GameObject pointInstance = null;
    private List<GameObject> chainInstances = new List<GameObject>();
    private Vector3 lastMovePosition;
    private AudioSource hookshotSound;

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
    void Start() {
        hookshotSound = transform.Find("Hookshot").GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate() {
        if (isEnabled) {
            if (isAiming) {
                DrawTarget();
                Fire();
            } else if (targetInstance != null) Destroy(targetInstance);
            if (isMoving) {
            //if (false) {
                lerpVal += reelSpeed * Time.deltaTime;
                if (lerpVal >= 1) {
                    Interrupt();
                } else {
                    transform.position = Vector3.Lerp(startPosition, moveTarget, lerpVal);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(Util.FlattenVectorOnY(moveTarget - transform.position)), 1500 * Time.deltaTime);
                    GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
                    if (Vector3.Distance(transform.position, moveTarget) >= Vector3.Distance(lastMovePosition, moveTarget)) {
                        Interrupt();
                    }
                    lastMovePosition = transform.position;
                    for (int i = 0; i < chainInstances.Count; i++) {
                        if (Vector3.Distance(chainInstances[i].transform.position, moveTarget) > Vector3.Distance(transform.position, moveTarget)) {
                            GameObject removeChain = chainInstances[i];
                            chainInstances.Remove(removeChain);
                            Destroy(removeChain);
                            i--;
                        }
                    }
                }
            }
        }
    }

    private void Destroy() {
        for (int i = 0; i < chainInstances.Count; i++) {
            GameObject removeChain = chainInstances[i];
            chainInstances.Remove(removeChain);
            Destroy(removeChain);
            i--;
        }
        //chainInstances.Clear();
        Destroy(pointInstance);
        pointInstance = null;
    }

    private void DrawTarget() {
        if (targetInstance == null) {
            targetInstance = Instantiate(target);
        }
        RaycastHit hookshotHit;
        hookshotHit = GetHookshotHit();
        if (hookshotHit.collider != null) {
            //targetInstance.transform.position = hookshotHit.point - hookshotHit.normal * 0.1f;
            targetInstance.transform.position = hookshotHit.point;
            targetInstance.transform.up = hookshotHit.normal;
            if (hookshotHit.collider.gameObject.GetComponent<HookshotTarget>() == null) {
                targetInstance.GetComponent<MeshRenderer>().material = noMat;
            //} else if (hookshotHit.collider.gameObject.GetComponent<HookshotTarget>() != null && targetInstance.GetComponent<MeshRenderer>().material == noMat) {
            } else {
                targetInstance.GetComponent<MeshRenderer>().material = yesMat;
            }
        }
    }

    private RaycastHit GetHookshotHit() {
        RaycastHit hookshotHit;
        Physics.Raycast(transform.position, aimTarget - transform.position, out hookshotHit, hookshotLength, 0b10000000, QueryTriggerInteraction.Ignore);
        return hookshotHit;
    }

    private void Fire() {
        float fireInput = input.Player.Attack.ReadValue<float>();
        RaycastHit hookshotHit = new RaycastHit();
        if (fireInput > 0) {
            hookshotHit = GetHookshotHit();
            if (!isMoving
                && hookshotHit.collider != null
                && !inputHeld) {
                Debug.Log("Hookshot hit something");
                HookshotTarget hitTarget = hookshotHit.collider.GetComponent<HookshotTarget>();
                if (hitTarget != null) {
                    moveTarget = hookshotHit.point + new Vector3(0, GetComponent<CapsuleCollider>().height / 2 + 0.1f, 0);
                    startPosition = transform.position;
                    isMoving = true;
                    pointInstance = Instantiate(point);
                    pointInstance.transform.position = hookshotHit.point;
                    pointInstance.transform.up = (startPosition - moveTarget).normalized;
                    float chainFrequency = 0.5f;
                    for (float i = 0; i < hookshotHit.distance; i += chainFrequency) {
                        chainInstances.Add(Instantiate(chain));
                        chainInstances[(int)(i / chainFrequency)].transform.position = (hookshotHit.point - startPosition).normalized * (i / hookshotHit.distance) * hookshotHit.distance + startPosition;
                        Debug.Log("points[" + (int)(i / chainFrequency) + "]");
                    }
                    lastMovePosition = startPosition;
                    hookshotSound.Play();
                    Debug.Log("Hookshot hit a target");
                }
                Debug.DrawLine(transform.position, aimTarget, Color.red, 3);
            }
            inputHeld = true;
        } else {
            inputHeld = false;
        }
    }

    public void Interrupt() {
        lerpVal = 0;
        isMoving = false;
        hookshotSound.Stop();
        Destroy();
    }
}

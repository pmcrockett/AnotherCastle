using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiftGlove : MonoBehaviour
{
    public PlayerInput input;
    public bool isEnabled = false;
    public bool isLifting = false;
    public bool isThrowFrame = false;
    public GameObject liftObj;
    private Animator anim;
    private float inputCooldownStart;
    private bool inputHeld = false;
    void Awake() {
        input = new PlayerInput();
        anim = GetComponent<Animator>();
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
        inputCooldownStart = Time.time;
    }

    // Update is called once per frame
    void Update() {
        if (isEnabled) {
            Throw();
            Lift();
            isThrowFrame = false;
        }
    }
    void FixedUpdate() {

    }
    private void Lift() {
        float liftInput = input.Player.Interact.ReadValue<float>();
        RaycastHit liftHit = new RaycastHit();
        if (liftInput > 0
            && Physics.Raycast(transform.position, transform.forward, out liftHit, 2, 0b10000000, QueryTriggerInteraction.Ignore)
            && CooldownIsUp() 
            && !inputHeld
            && !isLifting
            && !isThrowFrame
            && !GetComponent<InputHandler>().isFalling) {
            //Debug.Log("LiftGlove hit something");
            LiftTarget hitTarget = liftHit.collider.GetComponent<LiftTarget>();
            if (hitTarget != null) {
                //Debug.Log("Lifting object");
                hitTarget.StartLift(gameObject);
                liftObj = hitTarget.gameObject;
                isLifting = true;
                anim.SetBool("liftStart", true);
                anim.SetFloat("liftSpeed", hitTarget.liftSpeed / 3);
            }
        }
        if (liftInput > 0) {
            Debug.DrawLine(transform.position, transform.forward * 2 + transform.position, Color.red, 3);
            inputHeld = true;
            inputCooldownStart = Time.time;
        } else {
            inputHeld = false;
        }
    }
    private void Throw() {
        float liftInput = input.Player.Interact.ReadValue<float>();
        if (isLifting && liftInput > 0 && CooldownIsUp() && !inputHeld && liftObj.GetComponent<LiftTarget>().liftLerp >= 1) {
            isLifting = false;
            Vector3 throwDirection = (Quaternion.AngleAxis(liftObj.GetComponent<LiftTarget>().throwAngle * -1, transform.right) * transform.forward).normalized;
            //Vector3 throwDirection = (Quaternion.AngleAxis(0, transform.right) * transform.forward);
            liftObj.GetComponent<LiftTarget>().EndLift(throwDirection, GetComponent<Rigidbody>().velocity / 5);
            isThrowFrame = true;
            anim.SetBool("throwStart", true);
            //liftObj.GetComponent<LiftTarget>().EndLift(throwDirection, 5);
            Debug.Log("Throwing object at angle: " + liftObj.GetComponent<LiftTarget>().throwAngle * -1 + " with velocity " + GetComponent<Rigidbody>().velocity / 10 + " in direction " + throwDirection);
        }
    }

    private bool CooldownIsUp() {
        return Time.time - inputCooldownStart > 0.25;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiftTarget : MonoBehaviour
{
    public bool isLifted = false;
    public GameObject lifter;
    public Vector3 liftPosition = new Vector3(0, 2, 0);
    public float liftSpeed = 3;
    public float throwForce = 200;
    public float throwAngle = 55;
    public float liftLerp = 0;
    private Vector3 liftStartPos;
    private float groundedDrag;
    private Vector3 lifterOldForward;
    private Quaternion rotationDelta;
    private bool wasPreviouslyGrounded = true;
    // Start is called before the first frame update
    void Start()
    {
        groundedDrag = GetComponent<Rigidbody>().drag;
    }

    // Update is called once per frame
    void Update() {
        if (isLifted) {
            transform.position = Vector3.Lerp(lifter.transform.position + liftStartPos, lifter.transform.position + liftPosition, liftLerp);
            if (liftLerp < 1) liftLerp += Time.deltaTime * liftSpeed;
            else if (lifter.GetComponent<InputHandler>()!= null && lifter.GetComponent<InputHandler>().disableMoveImpulse == true) lifter.GetComponent<InputHandler>().disableMoveImpulse = false;
            rotationDelta = Quaternion.FromToRotation(lifterOldForward, lifter.transform.forward);
            transform.forward = rotationDelta * transform.forward;
            lifterOldForward = lifter.transform.forward;
        }
    }
    private void FixedUpdate() {
        //Debug.Log("Width: " + Util.GetColliderWidth(gameObject) + "; Scale x: " + transform.localScale.x + "; Scale z: " + transform.localScale.z + "; (Util.GetColliderWidth(gameObject) - 0.02f) / 2: " + (Util.GetColliderWidth(gameObject) - 0.02f) / 2);
        //RaycastHit fallCast;
        //Debug.Log(Physics.SphereCast(transform.position, (Util.GetColliderWidth(gameObject) - 0.02f) / 2, Vector3.down, out fallCast, 9999999, 0b10000000, QueryTriggerInteraction.Ignore));
        //Debug.Log(fallCast.distance + " vs. " + (Util.GetColliderHeight(gameObject) / 1.9 - (Util.GetColliderWidth(gameObject) - 0.02f) / 2));
        if (Util.IsGrounded(gameObject, (Util.GetColliderWidth(gameObject) - 0.02f) / 2)) {
            if (GetComponent<Rigidbody>().velocity == Vector3.zero) GetComponent<Rigidbody>().isKinematic = true;
            if (!wasPreviouslyGrounded) {
                if (transform.Find("Thud") != null) {
                    transform.Find("Thud").GetComponent<AudioSource>().Play();
                }
                GetComponent<Rigidbody>().drag = groundedDrag;
                wasPreviouslyGrounded = true;
            }
        } else {
            GetComponent<Rigidbody>().drag = 0;
            if (!isLifted) GetComponent<Rigidbody>().isKinematic = false;
            wasPreviouslyGrounded = false;
        }
    }

    public void StartLift(GameObject _lifter) {
        lifter = _lifter;
        isLifted = true;
        GetComponent<Collider>().isTrigger = true;
        GetComponent<Rigidbody>().isKinematic = true;
        liftStartPos = transform.position - lifter.transform.position;
        if (lifter.GetComponent<InputHandler>() != null) lifter.GetComponent<InputHandler>().disableMoveImpulse = true;
        lifter.GetComponent<Rigidbody>().velocity = Vector3.zero;
        lifterOldForward = lifter.transform.forward;
    }

    public void EndLift(Vector3 _throwDirection, Vector3 _velModifier) {
        isLifted = false;
        GetComponent<Collider>().isTrigger = false;
        GetComponent<Rigidbody>().isKinematic = false;
        GetComponent<Rigidbody>().AddForce((_throwDirection + _velModifier) * throwForce);
        GetComponent<Rigidbody>().drag = 0;
        liftLerp = 0;
    }
}

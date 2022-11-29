using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseAI : MonoBehaviour {
    public GameObject target;
    public float moveSpeed = 5;
    public float rotateSpeed = 200;
    public float desiredDistance = 0;
    public float desiredDistanceThreshold = 0.05f;
    public bool isInTargetRange = false;
    private float closestDist;
    private float magnitude;
    private Vector3 lastPosition;
    private SightPerception sight;
    private Animator anim;
    private Health health;

    void Awake() {
        anim = GetComponent<Animator>();
        health = GetComponent<Health>();
    }

    // Start is called before the first frame update
    void Start() {
        sight = GetComponent<SightPerception>();
        lastPosition = transform.position;
    }

    // Update is called once per frame
    void Update() {
        magnitude = Vector3.Distance(transform.position, lastPosition) * Time.deltaTime;
        lastPosition = transform.position;
        isInTargetRange = false;
        if (sight.targetInSight && (sight.target.GetComponent<Health>() == null || sight.target.GetComponent<Health>().health > 0)
            && health.health > 0) {
            if (sight.target != null && target != sight.target) {
                Init();
            }
            //if (GetComponent<Rigidbody>().velocity.magnitude < 0.01) {
            if (Util.IsGrounded(gameObject, GetComponent<CapsuleCollider>().radius - 0.05f)
                && target.GetComponent<InputHandler>().activeDialog == null) {
                float targetDist = Vector3.Distance(transform.position, target.transform.position);
                if (targetDist > closestDist + desiredDistanceThreshold) {
                    transform.position = Vector3.MoveTowards(transform.position, target.transform.position, moveSpeed * Time.deltaTime);
                    anim.SetFloat("walkSpeed", moveSpeed / 4);
                    //Debug.Log("walkSpeed: " + moveSpeed / 4);
                } else if (targetDist <= closestDist - desiredDistanceThreshold) {
                    Vector3 targetPos = target.transform.position + (transform.position - target.transform.position).normalized * (closestDist);
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                    anim.SetFloat("walkSpeed", moveSpeed / 4);
                    //Debug.Log("walkSpeed: " + moveSpeed / 4);
                } else {
                    isInTargetRange = true;
                    anim.SetFloat("walkSpeed", 0);
                }
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(Util.FlattenVectorOnY(target.transform.position - transform.position), Vector3.up), rotateSpeed * Time.deltaTime);
            } else anim.SetFloat("walkSpeed", 0);
        } else {
            anim.SetFloat("walkSpeed", 0);
        }
        Debug.DrawLine(transform.position, transform.position + transform.forward * 2, Color.blue);
    }

    private void Init() {
        target = sight.target;
        closestDist = GetComponent<CapsuleCollider>().radius + target.GetComponent<CapsuleCollider>().radius + desiredDistance;
    }
}
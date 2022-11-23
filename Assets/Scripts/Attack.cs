using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour {
    public float attackRange = 1;
    public float attackSpeed = 5;
    public float attackForce = 175;
    public float forceAngle = 45;
    public bool isEnabled = false;
    public bool isAttacking = false;
    public bool isReturning = false;
    private float adjustedRange;
    private float attackLerp = 0;
    private Vector3 attackStartPos;
    private Vector3 attackStartDir;
    private GameObject target;
    private Animator anim;

    private void Awake() {
        anim = GetComponent<Animator>();
    }
    // Start is called before the first frame update
    void Start() {
        //movement = GetComponent<ChaseAI>();
    }

    // Update is called once per frame
    void Update() {
        if (isAttacking) {
            // If there's an obstacle in the attack range
            CapsuleCollider collider = GetComponent<CapsuleCollider>();
            Vector3 point1 = transform.position - new Vector3(0, collider.height / 2 + collider.radius, 0);
            Vector3 point2 = transform.position + new Vector3(0, collider.height / 2 - collider.radius, 0);
            //if (Physics.CapsuleCast(point1, point2, collider.radius - 0.01f, attackStartDir, 0.02f, 0b10000000, QueryTriggerInteraction.Ignore)) {
            //    InterruptAttack();
            //}
            transform.position = Vector3.Lerp(attackStartPos, attackStartDir * adjustedRange + attackStartPos, attackLerp);
            if (!isReturning) {
                attackLerp += attackSpeed * Time.deltaTime;
            } else {
                attackLerp -= attackSpeed * Time.deltaTime;
            }
            if (attackLerp >= 1) {
                isReturning = true;
                attackLerp = 1;
            } else if (attackLerp <= 0) {
                InterruptAttack();
            }
        }
    }
    public void StartAttack(GameObject _target) {
        if (isEnabled) {
            isAttacking = true;
            isReturning = false;
            attackLerp = 0;
            attackStartPos = transform.position;
            attackStartDir = transform.forward;
            target = _target;
            anim.SetBool("isAttacking", true);
            CapsuleCollider collider = GetComponent<CapsuleCollider>();
            RaycastHit attackCast;
            Vector3 point1 = transform.position - new Vector3(0, collider.height / 2 + collider.radius, 0);
            Vector3 point2 = transform.position + new Vector3(0, collider.height / 2 - collider.radius, 0);
            if (Physics.CapsuleCast(point1, point2, collider.radius - 0.01f, attackStartDir, out attackCast, attackRange - collider.radius, 0b10000000, QueryTriggerInteraction.Ignore)) {
                adjustedRange = attackCast.distance;
            } else adjustedRange = attackRange;
        }
    }

    public void InterruptAttack() {
        isAttacking = false;
        isReturning = false;
        attackLerp = 0;
        anim.SetBool("isAttacking", false);
    }

    private void OnCollisionEnter(Collision collision) {
        if (isAttacking && !isReturning &&
            ((target == null && collision.gameObject.layer == 6)
            || (collision.gameObject == target))) {
            Attack targetAttack = collision.gameObject.GetComponent<Attack>();
            if (targetAttack != null && targetAttack.isAttacking && (attackLerp > targetAttack.attackLerp || targetAttack.isReturning)) {
                targetAttack.InterruptAttack();
            }
            Vector3 attackForceVector = Quaternion.FromToRotation(Vector3.forward, transform.forward) * Quaternion.Euler(forceAngle * -1, 0, 0) * Vector3.forward * attackForce;
            collision.gameObject.GetComponent<Rigidbody>().AddForce(attackForceVector);
        }
    }
}

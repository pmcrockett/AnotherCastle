using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour {
    public bool isPlayer = false;
    public int attackStrength = 1;
    public float attackRange = 1;
    public float attackSpeed = 5;
    public float attackForce = 175;
    public float forceAngle = 45;
    public GameObject sword;
    public bool isEnabled = false;
    public bool isAttacking = false;
    public bool isReturning = false;
    private float adjustedRange;
    private float attackLerp = 0;
    private Vector3 attackStartPos;
    private Vector3 attackStartDir;
    private GameObject target = null;
    private Animator anim;
    private GameObject swordInstance;
    List<GameObject> hitObjects = new List<GameObject>();
    private AudioSource attackSound;

    private void Awake() {
        anim = GetComponent<Animator>();
        attackSound = transform.Find("SwordSwish").GetComponent<AudioSource>();
    }
    // Start is called before the first frame update
    void Start() {
        if (isEnabled || (isPlayer && Game.PlayerState.Sword)) Enable();
    }

    // Update is called once per frame
    void Update() {
        if (isAttacking) {
            // If there's an obstacle in the attack range
            CapsuleCollider collider = GetComponent<CapsuleCollider>();
            Vector3 point1 = transform.position - new Vector3(0, collider.height / 2 + collider.radius, 0);
            Vector3 point2 = transform.position + new Vector3(0, collider.height / 2 - collider.radius, 0);
            transform.position = Vector3.Lerp(attackStartPos, attackStartDir * adjustedRange + attackStartPos, attackLerp);
            transform.forward = attackStartDir;
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
            attackSound.GetComponent<RandomRobin>().RefreshClip();
            attackSound.Play();
            ResetSwordPos();
        }
    }

    public void InterruptAttack() {
        isAttacking = false;
        isReturning = false;
        attackLerp = 0;
        anim.SetBool("isAttacking", false);
        hitObjects.Clear();
        ResetSwordPos();
    }

    private void OnCollisionEnter(Collision collision) {
        AttackCollision(collision.gameObject);
    }

    public void AttackCollision(GameObject _obj, bool _canHitOnReturn = false) {
        if (isAttacking && (!isReturning || _canHitOnReturn) && !hitObjects.Contains(_obj) &&
            ((target == null && _obj.layer == 6 && _obj.GetComponent<Health>() != null)
            || (_obj == target))) {
            Attack targetAttack = _obj.GetComponent<Attack>();
            if (targetAttack != null && targetAttack.isAttacking && (attackLerp > targetAttack.attackLerp || targetAttack.isReturning)) {
                targetAttack.InterruptAttack();
            }
            Vector3 attackForceVector = Quaternion.FromToRotation(Vector3.forward, transform.forward) * Quaternion.Euler(forceAngle * -1, 0, 0) * Vector3.forward * attackForce;
            _obj.GetComponent<Rigidbody>().AddForce(attackForceVector);
            _obj.GetComponent<Health>().ApplyDamage(attackStrength);
            hitObjects.Add(_obj);
            Debug.Log("Hit " + _obj.name);
        }
    }

    public void Enable() {
        isEnabled = true;
        swordInstance = Instantiate(sword, transform.Find("root").transform.Find("Hips").transform.Find("Stomach").transform.Find("Chest").transform.Find("Hand.R").transform);
        ResetSwordPos();
    }

    private void ResetSwordPos() {
        swordInstance.transform.rotation = swordInstance.transform.parent.transform.rotation * Quaternion.Euler(new Vector3(24.2405167f, 70.7948303f, 108.666313f));
        swordInstance.transform.position = swordInstance.transform.parent.transform.position;
        swordInstance.transform.localPosition = new Vector3(0.000750000007f, 0.00677999994f, 0.000380000012f);
    }
}

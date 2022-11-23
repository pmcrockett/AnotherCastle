using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InputHandler : MonoBehaviour
{
    public PlayerInput input;
    public Camera playerCamera;
    public float walkSpeed = 1000;
    public float airMovementSpeed = 10;
    public float jumpHeight = 1000;
    public float airMovementFactor = 20;
    public float floorDrag = 10;
    public float airDrag = 0;
    public float climbSpeed = 2;
    public float rotateSpeed = 1000;
    public float dialogInputCooldown = 0.25f;
    public float currentVelocity;
    public bool disableMoveImpulse = false;
    public bool isWallJumping = false;
    public bool isFalling = false;
    public bool jumpEnabled = true;
    public GameObject activeDialog = null;
    private Rigidbody body;
    private Animator anim;
    private SphereCollider jumpCollider;
    private Attack attack;
    private bool canJump = true;
    private float lastJumpTime;
    private float lastWallJumpTime;
    private float wallJumpWindowStart;
    private Vector3 wallJumpVector;
    private float fallCastRadius;
    private bool jumpInputHeld = false;
    private bool attackInputHeld = false;
    private Vector3 lastUpdatePosition;
    private Vector3 currentMoveDirection = new Vector3(0, 0, 0);
    private Hookshot hookshot;
    private Vector3 climbStartPos;
    private Vector3 climbEndPos;
    private float climbLerp = 0;
    private Vector3 lastMoveInput = Vector3.zero;
    private Vector3 desiredForward = Vector3.zero;
    private float lastDialogInput = -1;
    TextMesh textMesh;
    
    void Awake()
    {
        input = new PlayerInput();
        body = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        attack = GetComponent<Attack>();
        //jumpCollider = GetComponent<BoxCollider>();
        jumpCollider = GetComponent<SphereCollider>();
        fallCastRadius = (Util.GetColliderWidth(gameObject) - 0.02f) / 2;
        hookshot = GetComponent<Hookshot>();
    }

    void OnEnable()
    {
        input.Player.Enable();
    }
    
    void OnDisable()
    {
        input.Player.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        lastJumpTime = Time.time;
        lastWallJumpTime = Time.time;
        lastUpdatePosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (activeDialog == null) {
            //if (!isFalling && lastMoveInput != Vector3.zero && !disableMoveImpulse) {
            if (!isFalling && desiredForward != Vector3.zero && !disableMoveImpulse && climbLerp <= 0) {
                //transform.rotation = Quaternion.LookRotation(lastMoveInput, Vector3.up);
                //Quaternion idealRotation = Quaternion.LookRotation(lastMoveInput, Vector3.up);
                Quaternion idealRotation = Quaternion.LookRotation(desiredForward, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, idealRotation, rotateSpeed * Time.deltaTime);
            } else if (isFalling && lastMoveInput != Vector3.zero) {
                AirMovement();
            }
            //forwared vector
            Debug.DrawLine(transform.position, transform.position + transform.forward * 1.25f, Color.yellow, 0.2f);
        } else {
            Dialog();
        }
        if (climbLerp > 0) ContinueClimbEdge();
    }

    void FixedUpdate()
    {
            //UpdateWallCollisions();
        currentMoveDirection = (transform.position - lastUpdatePosition).normalized;
        currentVelocity = Vector3.Distance(transform.position, lastUpdatePosition);
        Attack();
        Jump();
        Movement();
        ClimbStairs();
        lastUpdatePosition = transform.position;
    }

    private void Movement()
    {
        float moveSpeed;
        if (activeDialog == null) {
            if (canJump) moveSpeed = walkSpeed;
            else moveSpeed = airMovementSpeed;
        } else moveSpeed = 0;
        Vector2 moveInput = input.Player.Move.ReadValue<Vector2>() * moveSpeed * Time.deltaTime;
        //Debug.Log("moveInput: " + input.Player.Move.ReadValue<Vector2>());
        Vector3 flattenedForward = new Vector3(playerCamera.transform.forward.x, 0, playerCamera.transform.forward.z).normalized;
        Vector3 flattenedRight = new Vector3(playerCamera.transform.right.x, 0, playerCamera.transform.right.z).normalized;
        Vector3 moveForce = flattenedForward * moveInput.y + flattenedRight * moveInput.x;
        //if (!disableMoveImpulse && !hookshot.isMoving && climbLerp <= 0) body.AddForce(moveForce);
        if (!disableMoveImpulse && !hookshot.isMoving && climbLerp <= 0 ) {
            if ((!isFalling && Vector3.Dot(transform.forward, moveForce.normalized) >= 0.8) || isFalling) {
                body.AddForce(transform.forward * moveForce.magnitude);
            }
        }
        lastMoveInput = moveForce / (moveSpeed * Time.deltaTime);
        if ((lastMoveInput != Vector3.zero && !isFalling) || isFalling) desiredForward = lastMoveInput;
        if (body.velocity.magnitude > 0 && !isFalling) {
            anim.SetFloat("walkSpeed", body.velocity.magnitude / 7);
        } else {
            anim.SetFloat("walkSpeed", 0);
        }
    }

    private void Jump()
    {
        if (Util.IsGrounded(gameObject, fallCastRadius) && Time.time - lastJumpTime > 0.25 && !canJump) {
            canJump = true;
            body.drag = floorDrag;
            disableMoveImpulse = false;
            isFalling = false;
            isWallJumping = false;
            anim.SetBool("isFalling", false);
        } else if (!Util.IsGrounded(gameObject, fallCastRadius)) {
            canJump = false;
            body.drag = airDrag;
            isFalling = true;
            anim.SetBool("isFalling", true);
            if (CanEdgeGrab() && jumpInputHeld && jumpEnabled && activeDialog == null) {
                StartClimbEdge();
            }
        }

        if (activeDialog == null && jumpEnabled) {
            float jumpInput = input.Player.Jump.ReadValue<float>();
            if (jumpInput > 0) {
                if (climbLerp <= 0 && !GetComponent<LiftGlove>().isLifting && !attack.isAttacking) {
                    if (canJump && !jumpInputHeld && !hookshot.isMoving) {
                        body.AddForce(Vector3.up * jumpHeight);
                        disableMoveImpulse = false;
                        lastJumpTime = Time.time;
                        anim.SetBool("jumpStart", true);
                    } else if (!canJump && !jumpInputHeld && Time.time - wallJumpWindowStart <= 0.25) {
                        WallJump(wallJumpVector);
                    } else if (!canJump && !jumpInputHeld) {
                        lastWallJumpTime = Time.time;
                    }
                }
                jumpInputHeld = true;
            } else jumpInputHeld = false;
        }
    }

    private void Attack() {
        if (activeDialog == null) {
            float attackInput = input.Player.Attack.ReadValue<float>();
            if (attackInput > 0) {
                if (!attackInputHeld && !attack.isAttacking && !isFalling && climbLerp <= 0 && !disableMoveImpulse && !GetComponent<LiftGlove>().isLifting) {
                    attack.StartAttack(null);
                }
                attackInputHeld = true;
            } else attackInputHeld = false;
        }
    }

    private bool CanClimbStairs() {
        if (!isFalling && climbLerp <= 0) {
            bool lowCast = Physics.Raycast(transform.position - new Vector3(0, GetComponent<CapsuleCollider>().height / 2.05f, 0), transform.forward, GetComponent<CapsuleCollider>().radius + 0.05f, 0b10000000, QueryTriggerInteraction.Ignore);
            bool highCast = Physics.Raycast(transform.position - new Vector3(0, GetComponent<CapsuleCollider>().height / 3.5f, 0), transform.forward, GetComponent<CapsuleCollider>().radius + 0.05f, 0b10000000, QueryTriggerInteraction.Ignore);
            Debug.DrawLine(transform.position - new Vector3(0, GetComponent<CapsuleCollider>().height / 2.05f, 0), transform.position - new Vector3(0, GetComponent<CapsuleCollider>().height / 2.05f, 0) + transform.forward * (GetComponent<CapsuleCollider>().radius + 0.05f), Color.green, 0.2f);
            Debug.DrawLine(transform.position - new Vector3(0, GetComponent<CapsuleCollider>().height / 3.5f, 0), transform.position - new Vector3(0, GetComponent<CapsuleCollider>().height / 3.5f, 0) + transform.forward * (GetComponent<CapsuleCollider>().radius + 0.05f), Color.green, 0.2f);
            if (lowCast && !highCast) return true;
        }
        return false;
    }
    private float GetStairDepth() {
        RaycastHit depthCast;
        bool lowCast = Physics.Raycast(transform.position - new Vector3(0, GetComponent<CapsuleCollider>().height / 2.05f, 0), transform.forward, out depthCast, GetComponent<CapsuleCollider>().radius + 0.05f, 0b10000000, QueryTriggerInteraction.Ignore);
        return depthCast.distance;
    }

    private float GetStairHeight() {
        float depth = GetStairDepth();
        RaycastHit heightCast;
        Physics.Raycast(transform.position - new Vector3(0, GetComponent<CapsuleCollider>().height / 3.5f, 0) + transform.forward * (depth + 0.01f), Vector3.down, out heightCast, 99, 0b10000000, QueryTriggerInteraction.Ignore);
        return heightCast.point.y - GetFloorY();
    }

    private float GetFloorY() {
        RaycastHit floorCast;
        Physics.Raycast(transform.position, Vector3.down, out floorCast, 99, 0b10000000, QueryTriggerInteraction.Ignore);
        return floorCast.point.y;
    }

    private void ClimbStairs() {
        if (CanClimbStairs()) {
            Debug.Log("Climbing stairs");
            Debug.Log("transform.position.y: " + transform.position.y + "; GetFloorY: " + GetFloorY() + "; GetStairHeight: " + GetStairHeight() + "; big equation: " + GetFloorY() + Mathf.Lerp(GetStairHeight(), 0, GetStairDepth() / (GetComponent<CapsuleCollider>().radius + 0.05f)) + GetComponent<CapsuleCollider>().height / 2 + "; distance ratio: " + GetStairDepth() / (GetComponent<CapsuleCollider>().radius + 0.05f) + "; depth: " + GetStairDepth() + "; trace distance: " + (GetComponent<CapsuleCollider>().radius + 0.05f));
            float stairClimbPosition = GetFloorY() + Mathf.Lerp(GetStairHeight(), 0, (GetStairDepth() - 0.3f) / (GetComponent<CapsuleCollider>().radius + 0.05f)) + GetComponent<CapsuleCollider>().height / 2;
            if (transform.position.y < stairClimbPosition) {
                transform.position = new Vector3(transform.position.x, stairClimbPosition, transform.position.z);
            }
        }
    }

    private void AirMovement() {
        if (!disableMoveImpulse) {
            float moveFactor;
            if (isWallJumping) moveFactor = airMovementFactor / 8;
            else moveFactor = airMovementFactor;
            Vector3 idealVel = Quaternion.FromToRotation(Util.FlattenVectorOnY(body.velocity), lastMoveInput.normalized) * body.velocity;
            Vector2 oldXZVel = new Vector2(body.velocity.x, body.velocity.z);
            Vector2 idealXZVel = new Vector2(idealVel.x, idealVel.z);
            Vector2 actualXZVel = (idealXZVel - oldXZVel).normalized * moveFactor * lastMoveInput.magnitude * Time.deltaTime + oldXZVel;
            body.velocity = new Vector3(actualXZVel.x, body.velocity.y, actualXZVel.y);
            if (Util.FlattenVectorOnY(body.velocity) != Vector3.zero && Vector3.Dot(body.velocity.normalized, transform.forward) >= 0.75) {
                transform.rotation = Quaternion.LookRotation(Util.FlattenVectorOnY(body.velocity), Vector3.up);
            }
        }
    }
    private void StartClimbEdge() {
        if (climbLerp <= 0) {
            climbStartPos = transform.position;
            RaycastHit lowCastHit;
            Physics.Raycast(transform.position - new Vector3(0, GetComponent<CapsuleCollider>().height / 2, 0), transform.forward, out lowCastHit, 1.25f, 0b10000000, QueryTriggerInteraction.Ignore);
            Vector3 highCast = transform.position + new Vector3(0, GetComponent<CapsuleCollider>().height / 4, 0) + transform.forward * lowCastHit.distance * 1.01f;
            RaycastHit climbHit;
            Physics.Raycast(highCast, Vector3.down, out climbHit, 5, 0b10000000, QueryTriggerInteraction.Ignore);
            climbEndPos = climbHit.point + new Vector3(0, GetComponent<CapsuleCollider>().height / 2, 0);
            climbLerp += Time.deltaTime * climbSpeed;
            GetComponent<CapsuleCollider>().enabled = false;
            anim.SetBool("climbStart", true);
        }
    }

    private void ContinueClimbEdge() {
        transform.position = Vector3.Lerp(climbStartPos, climbEndPos, climbLerp);
        body.velocity = new Vector3(0, 0, 0);
        if (climbLerp < 1) climbLerp += Time.deltaTime * climbSpeed;
        else {
            climbLerp = 0;
            GetComponent<CapsuleCollider>().enabled = true;
        }
    }

    private void WallJump(Vector3 _reboundDir) {
        body.AddForce(_reboundDir * 150);
        disableMoveImpulse = false;
        transform.rotation = Quaternion.LookRotation(Util.FlattenVectorOnY(_reboundDir), Vector3.up);
        isWallJumping = true;
        wallJumpWindowStart = 0;
        lastJumpTime = 0;
        anim.SetBool("wallJumpStart", true);
    }
    private bool CanEdgeGrab() {
        bool lowCast = Physics.Raycast(transform.position - new Vector3(0, GetComponent<CapsuleCollider>().height / 3.6f, 0), transform.forward, 1.25f, 0b10000000, QueryTriggerInteraction.Ignore);
        bool highCast = Physics.Raycast(transform.position + new Vector3(0, GetComponent<CapsuleCollider>().height / 4, 0), transform.forward, 1.25f, 0b10000000, QueryTriggerInteraction.Ignore);
        Debug.DrawLine(transform.position - new Vector3(0, GetComponent<CapsuleCollider>().height / 3.6f, 0), transform.position - new Vector3(0, GetComponent<CapsuleCollider>().height / 3.6f, 0) + transform.forward * 1.25f, Color.blue, 3);
        Debug.DrawLine(transform.position + new Vector3(0, GetComponent<CapsuleCollider>().height / 4, 0), transform.position + new Vector3(0, GetComponent<CapsuleCollider>().height / 4, 0) + transform.forward * 1.25f, Color.blue, 3);
        return lowCast && !highCast;
    }

    private void Dialog() {
        if (playerCamera.GetComponent<CameraBehavior>().activeDialog == null) playerCamera.GetComponent<CameraBehavior>().activeDialog = activeDialog;
        if (lastDialogInput < 0) {
            lastDialogInput = Time.time;
        }
        float dialogInput = input.Player.Attack.ReadValue<float>() > 0 ? 1 : input.Player.Jump.ReadValue<float>();
        if (dialogInput > 0 && !jumpInputHeld) {
            if (Time.time - lastDialogInput >= dialogInputCooldown) {
                if (activeDialog.GetComponentInChildren<DialogBox>().DisplayNextDialog()) {
                    lastDialogInput = Time.time;
                } else {
                    Object.Destroy(activeDialog);
                    jumpInputHeld = false;
                    lastDialogInput = -1;
                    playerCamera.GetComponent<CameraBehavior>().activeDialog = null;
                }
            }
            jumpInputHeld = true;
        } else if (dialogInput <= 0 && jumpInputHeld) {
            jumpInputHeld = false;
        }
    }
    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.layer == 7 && !canJump && climbLerp <= 0) {
            disableMoveImpulse = true;
            Debug.Log("Current move direction: " + currentMoveDirection);
            RaycastHit wallCast = new RaycastHit();
            Vector3 wallCastDir = new Vector3(currentMoveDirection.x, 0, currentMoveDirection.z).normalized;
            Physics.SphereCast(transform.position - wallCastDir / 2, GetComponent<CapsuleCollider>().radius - 0.1f, wallCastDir, out wallCast, 1.25f, 0b10000000, QueryTriggerInteraction.Ignore);
            Debug.DrawLine(transform.position, transform.position + wallCastDir, Color.red, 5);
            if (wallCast.collider) {
                Vector3 reboundDir;
                //wallJumpVector = new Vector3(currentMoveDirection.x * -1, Mathf.Clamp(currentMoveDirection.y * 12, 0.75f, 8), currentMoveDirection.z * -1).normalized;
                wallJumpVector = new Vector3(wallCast.normal.x, Mathf.Clamp(currentMoveDirection.y * 12, 0.75f, 2), wallCast.normal.z).normalized;
                if (Time.time - lastWallJumpTime <= 0.25 && jumpEnabled && !canJump && !GetComponent<LiftGlove>().isLifting && activeDialog == null) {
                    WallJump(wallJumpVector);
                } else {
                    wallJumpWindowStart = Time.time;
                    reboundDir = Vector3.Dot(currentMoveDirection, Vector3.up) < 0 ? new Vector3(currentMoveDirection.x, 0, currentMoveDirection.z).normalized * -1 : currentMoveDirection * -1;
                    body.AddForce(reboundDir * currentVelocity * 150);
                    gameObject.transform.forward = Util.FlattenVectorOnY(currentMoveDirection);
                    Debug.Log("Bump!");
                }
            }
        }
    }
    private void OnCollisionExit(Collision collision) {
        if (collision.collider.tag == "Floor") {
            //disableMoveImpulse = false;
        }
    }
    private void OnCollisionStay(Collision collision) {
        if (collision.collider.tag == "Floor" && !canJump) {
            disableMoveImpulse = true;
        }
    }
    
    private void OnTriggerEnter(Collider other) {
        if (hookshot.isMoving) {
            hookshot.isMoving = false;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public float currentVelocity;
    public bool disableMoveImpulse = false;
    public bool isWallJumping = false;
    public bool isFalling = false;
    private Rigidbody body;
    private SphereCollider jumpCollider;
    private bool canJump = true;
    private float lastJumpTime;
    private float lastWallJumpTime;
    private float wallJumpWindowStart;
    private Vector3 wallJumpVector;
    private float fallCastRadius;
    private bool jumpInputHeld;
    private Vector3 lastUpdatePosition;
    private Vector3 currentMoveDirection = new Vector3(0, 0, 0);
    private Hookshot hookshot;
    private Vector3 climbStartPos;
    private Vector3 climbEndPos;
    private float climbLerp = 0;
    private Vector3 lastMoveInput = Vector3.zero;
    private Vector3 desiredForward = Vector3.zero;

    void Awake()
    {
        input = new PlayerInput();
        body = GetComponent<Rigidbody>();
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
        Debug.Log("Player start position: " + transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        //if (!isFalling && lastMoveInput != Vector3.zero && !disableMoveImpulse) {
        if (!isFalling && desiredForward != Vector3.zero && !disableMoveImpulse) {
            //transform.rotation = Quaternion.LookRotation(lastMoveInput, Vector3.up);
            //Quaternion idealRotation = Quaternion.LookRotation(lastMoveInput, Vector3.up);
            Quaternion idealRotation = Quaternion.LookRotation(desiredForward, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, idealRotation, rotateSpeed * Time.deltaTime);
        } else if (isFalling && lastMoveInput != Vector3.zero) {
            AirMovement();
        }
        if (climbLerp > 0) ContinueClimbEdge();
        //forwared vector
        Debug.DrawLine(transform.position, transform.position + transform.forward * 1.25f, Color.yellow, 0.2f);
    }

    void FixedUpdate()
    {
        //UpdateWallCollisions();
        currentMoveDirection = (transform.position - lastUpdatePosition).normalized;
        currentVelocity = Vector3.Distance(transform.position, lastUpdatePosition);
        Jump();
        Movement();
        ClimbStairs();
        lastUpdatePosition = transform.position;
    }

    private void Movement()
    {
        float moveSpeed;
        if (canJump) moveSpeed = walkSpeed;
        else moveSpeed = airMovementSpeed;
        Vector2 moveInput = input.Player.Move.ReadValue<Vector2>() * moveSpeed * Time.deltaTime;
        //Debug.Log("moveInput: " + input.Player.Move.ReadValue<Vector2>());
        Vector3 flattenedForward = new Vector3(playerCamera.transform.forward.x, 0, playerCamera.transform.forward.z).normalized;
        Vector3 flattenedRight = new Vector3(playerCamera.transform.right.x, 0, playerCamera.transform.right.z).normalized;
        Vector3 moveForce = flattenedForward * moveInput.y + flattenedRight * moveInput.x;
        //if (!disableMoveImpulse && !hookshot.isMoving && climbLerp <= 0) body.AddForce(moveForce);
        if (!disableMoveImpulse && !hookshot.isMoving && climbLerp <= 0 ) {
            if ((!isFalling && Vector3.Dot(transform.forward, moveForce.normalized) >= 0.8) || isFalling) body.AddForce(transform.forward * moveForce.magnitude);
        }
        lastMoveInput = moveForce / (moveSpeed * Time.deltaTime);
        if ((lastMoveInput != Vector3.zero && !isFalling) || isFalling) desiredForward = lastMoveInput;
    }

    private void Jump()
    {
        if (Util.IsGrounded(gameObject, fallCastRadius) && Time.time - lastJumpTime > 0.25 && !canJump) {
            canJump = true;
            body.drag = floorDrag;
            disableMoveImpulse = false;
            isFalling = false;
            isWallJumping = false;
        } else if (!Util.IsGrounded(gameObject, fallCastRadius)) {
            canJump = false;
            body.drag = airDrag;
            isFalling = true;
            if (CanEdgeGrab() && jumpInputHeld) {
                StartClimbEdge();
            }
        }
        
        float jumpInput = input.Player.Jump.ReadValue<float>();
      
        if (jumpInput > 0 ) {
            if (climbLerp <= 0 && !GetComponent<LiftGlove>().isLifting) {
                if (canJump && !jumpInputHeld && !hookshot.isMoving) {
                    body.AddForce(Vector3.up * jumpHeight);
                    disableMoveImpulse = false;
                    lastJumpTime = Time.time;
                } else if (!canJump && !jumpInputHeld && Time.time - wallJumpWindowStart <= 0.25) {
                    WallJump(wallJumpVector);
                } else if (!canJump && !jumpInputHeld) {
                    lastWallJumpTime = Time.time;
                }
            }
            jumpInputHeld = true;
        } else jumpInputHeld = false;

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
    }
    private bool CanEdgeGrab() {
        bool lowCast = Physics.Raycast(transform.position - new Vector3(0, GetComponent<CapsuleCollider>().height / 3.6f, 0), transform.forward, 1.25f, 0b10000000, QueryTriggerInteraction.Ignore);
        bool highCast = Physics.Raycast(transform.position + new Vector3(0, GetComponent<CapsuleCollider>().height / 4, 0), transform.forward, 1.25f, 0b10000000, QueryTriggerInteraction.Ignore);
        Debug.DrawLine(transform.position - new Vector3(0, GetComponent<CapsuleCollider>().height / 3.6f, 0), transform.position - new Vector3(0, GetComponent<CapsuleCollider>().height / 3.6f, 0) + transform.forward * 1.25f, Color.blue, 3);
        Debug.DrawLine(transform.position + new Vector3(0, GetComponent<CapsuleCollider>().height / 4, 0), transform.position + new Vector3(0, GetComponent<CapsuleCollider>().height / 4, 0) + transform.forward * 1.25f, Color.blue, 3);
        return lowCast && !highCast;
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
                if (Time.time - lastWallJumpTime <= 0.25 && !canJump && !GetComponent<LiftGlove>().isLifting) {
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour {
    public PlayerInput input;
    public GameObject cameraTarget;
    public GameObject cameraSightCollider;
    public bool debugLerp = false;
    public float cameraDistance = 10;
    public float cameraSpeed = 180;
    public float innerCameraTraceRadius = 0.5f;
    public float outerCameraTraceRadius = 1.5f;
    public float minimumViewAngle = -15;
    public float maximumViewAngle = 60;
    public float blockAdjustSpeed = 0.1f;
    public bool invertX = false;
    public bool invertY = false;
    public GameObject activeDialog = null;
    private Vector3 idealCameraPosition;
    private Vector3 idealCameraForward;
    private Vector3 oldCameraAngle;
    private float[] hookshotXY = new float[2];
    private float blockStart = -1;
    private Hookshot hookshot;

    void Awake()
    {
        input = new PlayerInput();
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
        //Move();
        oldCameraAngle = (transform.position - cameraTarget.transform.position).normalized;
        Vector3 directionToCamera = (transform.position - cameraTarget.transform.position).normalized;
        hookshot = cameraTarget.GetComponent<Hookshot>();
    }

    // Update is called once per frame
    void Update()
    {
        if (cameraTarget != null) {
            Move();
        }
        //Debug.DrawLine(new Vector3(0, 0, 0), cameraTarget.transform.position, Color.red);
    }

    private void Move() {
        float aimInput = input.Player.Aim.ReadValue<float>();
        Vector2 camInput;
        if (activeDialog == null) {
            if (aimInput <= 0.25 || !cameraTarget.GetComponent<Hookshot>().isEnabled) {
                camInput = input.Player.Look.ReadValue<Vector2>() * cameraSpeed;
                hookshot.isAiming = false;
            } else {
                camInput = new Vector2(0, 0);
                AimHookshot();
            }
        } else camInput = Vector2.zero;
        if (Game.Axis.InvertCameraX) camInput[0] *= -1;
        if (Game.Axis.InvertCameraY) camInput[1] *= -1;
        Vector3 desiredPos = GetBlockedCameraPosition();
        Vector3 currentCameraAngle = (oldCameraAngle * cameraDistance).normalized;
        Vector3 flattenedForward = Util.FlattenVectorOnY(idealCameraForward);
        Quaternion horizontalDelta = Quaternion.AngleAxis(camInput.x * Time.deltaTime, Vector3.up);
        Quaternion verticalDelta = Quaternion.AngleAxis(camInput.y * -1 * Time.deltaTime, transform.right);
        float verticalAngle = Vector3.Angle(verticalDelta * currentCameraAngle, flattenedForward * -1);

        Quaternion revisedDelta = ClampVerticalDelta(verticalAngle, verticalDelta, currentCameraAngle);
        if (revisedDelta != verticalDelta) {
            verticalDelta = revisedDelta;
            currentCameraAngle = flattenedForward * -1;
        }

        if (desiredPos == idealCameraPosition) {
            transform.position = horizontalDelta * verticalDelta * currentCameraAngle * cameraDistance + cameraTarget.transform.position;
        } else transform.position = desiredPos;
        idealCameraPosition = horizontalDelta * verticalDelta * currentCameraAngle * cameraDistance + cameraTarget.transform.position;
        idealCameraForward = (cameraTarget.transform.position - idealCameraPosition).normalized;
        transform.LookAt(cameraTarget.transform);
        oldCameraAngle = (idealCameraPosition - cameraTarget.transform.position).normalized;
    }
    private Quaternion ClampVerticalDelta(float _verticalAngle, Quaternion _verticalDelta, Vector3 _currentAngle) {
        if (_verticalAngle > maximumViewAngle && Vector3.Dot(_verticalDelta * _currentAngle, Vector3.up) > 0) {
            return Quaternion.AngleAxis(maximumViewAngle, transform.right);
        } else if (_verticalAngle > Mathf.Abs(minimumViewAngle) && Vector3.Dot(_verticalDelta * _currentAngle, Vector3.up) <= 0) {
            return Quaternion.AngleAxis(minimumViewAngle, transform.right);
        }
        return _verticalDelta;
    }
    private Vector3 GetBlockedCameraPosition() {
        Vector3 directionToCamera = (idealCameraPosition - cameraTarget.transform.position).normalized;
        Vector3 traceStart = cameraTarget.transform.position;
        Vector3 closestBlockedToCamera = traceStart;
        Vector3 farthestBlockedFromCamera = idealCameraPosition;
        float traceDistance = Vector3.Distance(traceStart, (directionToCamera * cameraDistance) + traceStart);
        RaycastHit[] cameraSight = Physics.SphereCastAll(traceStart, innerCameraTraceRadius, directionToCamera, traceDistance, 0b10000000, QueryTriggerInteraction.Ignore);
        foreach (RaycastHit x in cameraSight) {
            //Debug.Log("Camera sight: " + x.collider.gameObject.name);
            if (x.collider.gameObject.GetComponent<NoCameraCollision>() == null
            && !(x.collider.gameObject.GetComponent<LiftTarget>()
            && (x.collider.gameObject.layer == 6 && !Util.IsGrounded(x.collider.gameObject, (Util.GetColliderWidth(x.collider.gameObject) - 0.02f) / 2)))) {
                Vector3 hitPosition = Vector3.Project(x.point - cameraTarget.transform.position, directionToCamera) + cameraTarget.transform.position;
                if (Vector3.Distance(traceStart, hitPosition) > Vector3.Distance(traceStart, closestBlockedToCamera)) closestBlockedToCamera = hitPosition;
                if (Vector3.Distance(traceStart, hitPosition) < Vector3.Distance(traceStart, farthestBlockedFromCamera)) farthestBlockedFromCamera = hitPosition;
            } else if (x.collider.gameObject.GetComponent<NoCameraCollision>() != null) {
                x.collider.gameObject.GetComponent<GhostEffect>().isGhost = true;
            }
        }
        if (farthestBlockedFromCamera != idealCameraPosition) {
            if (blockStart < 0) {
                blockStart = Time.time;
            }
            if (Time.time - blockStart >= blockAdjustSpeed || Vector3.Distance(transform.position, closestBlockedToCamera) < cameraDistance / 2) {
                return farthestBlockedFromCamera;
            } else return idealCameraPosition;
        } else blockStart = -1;
        return idealCameraPosition;
    }
    private void AimHookshot() {
        if (!hookshot.isAiming) {
            hookshot.isAiming = true;
            hookshotXY[0] = Screen.currentResolution.width / 2;
            hookshotXY[1] = Screen.currentResolution.height * 0.9f;
            hookshot.aimTarget = gameObject.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(hookshotXY[0], hookshotXY[1], cameraTarget.GetComponent<Hookshot>().hookshotLength));
        }
        Vector2 camInput = input.Player.Look.ReadValue<Vector2>();
        if (Game.Axis.InvertAimX) camInput[0] *= -1;
        if (Game.Axis.InvertAimY) camInput[1] *= -1;
        hookshotXY[0] = Mathf.Clamp(hookshotXY[0] + camInput[0] * Time.deltaTime * Screen.currentResolution.width / hookshot.aimSpeedDenominator, 0, Screen.currentResolution.width);
        hookshotXY[1] = Mathf.Clamp(hookshotXY[1] + camInput[1] * Time.deltaTime * Screen.currentResolution.width / hookshot.aimSpeedDenominator, 0, Screen.currentResolution.height);
        hookshot.aimTarget = gameObject.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(hookshotXY[0], hookshotXY[1], cameraTarget.GetComponent<Hookshot>().hookshotLength));
        cameraTarget.GetComponent<GhostEffect>().isGhost = true;
        Debug.DrawLine(cameraTarget.transform.position, hookshot.aimTarget, Color.yellow);
    }

    public void Respawn(Checkpoint _checkpoint) {
        if (_checkpoint != null) {
            transform.position = (Quaternion.AngleAxis(135, _checkpoint.transform.right) * _checkpoint.transform.forward * -1 * cameraDistance) + _checkpoint.transform.position;
        } else {
            transform.position = Vector3.zero;
        }
        transform.LookAt(cameraTarget.transform);
        idealCameraPosition = transform.position;
        idealCameraForward = (cameraTarget.transform.position - idealCameraPosition).normalized;
        oldCameraAngle = (transform.position - cameraTarget.transform.position).normalized;
    }
}

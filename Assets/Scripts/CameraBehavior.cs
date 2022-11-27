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
    private Vector3 outerHitPosition;
    private bool cameraIsLerping = false;
    private bool sightSpheresActive = false;
    private bool resetHookshotAim = true;
    private float[] hookshotXY = new float[2];
    private float blockStart = -1;
    List<GameObject> cameraSpheres = new List<GameObject>();

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
        for (float i = 1; (outerCameraTraceRadius > 0 ? i * outerCameraTraceRadius : i) < cameraDistance; i++) {
            GameObject newSphere = Instantiate(cameraSightCollider, i * outerCameraTraceRadius * transform.forward * -1 + cameraTarget.transform.position, Quaternion.identity);
            SphereCollider newCollider = newSphere.GetComponent<SphereCollider>();
            newCollider.isTrigger = true;
            newCollider.radius = outerCameraTraceRadius;
            //newSphere.isTrigger = true;
            cameraSpheres.Add(newSphere);
            //Debug.Log(newSphere.transform.position);
        }
        //cameraSpheres.Reverse();
    }

    // Update is called once per frame
    void Update()
    {
        if (cameraTarget != null) {
            Move();
            MoveSightSpheres();
        }
        //Debug.DrawLine(new Vector3(0, 0, 0), cameraTarget.transform.position, Color.red);
    }

    private void Move() {
        float aimInput = input.Player.Aim.ReadValue<float>();
        Vector2 camInput;
        if (activeDialog == null) {
            if (aimInput <= 0.25 || !cameraTarget.GetComponent<Hookshot>().isEnabled) {
                camInput = input.Player.Look.ReadValue<Vector2>() * cameraSpeed;
                resetHookshotAim = true;
            } else {
                camInput = new Vector2(0, 0);
                AimHookshot();
                resetHookshotAim = false;
            }
        } else camInput = Vector2.zero;
        //if (invertX) camInput[0] *= -1;
        //if (invertY) camInput[1] *= -1;
        if (Controls.Axis.InvertCameraX) camInput[0] *= -1;
        if (Controls.Axis.InvertCameraY) camInput[1] *= -1;
        if (debugLerp) {
            camInput = input.Player.Look.ReadValue<Vector2>();
            transform.position = GetLerpPosition(Mathf.Clamp(camInput[1], 0, 1));
            transform.LookAt(cameraTarget.transform);
            return;
        }
        Vector3 desiredPos = GetBlockedCameraPosition();

        Vector3 currentCameraAngle = (oldCameraAngle * cameraDistance).normalized;
        //Vector3 normalizedForward = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
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
                //return cameraSight.point + directionToCamera;
                Vector3 hitPosition = Vector3.Project(x.point - cameraTarget.transform.position, directionToCamera) + cameraTarget.transform.position;
                if (Vector3.Distance(traceStart, hitPosition) > Vector3.Distance(traceStart, closestBlockedToCamera)) closestBlockedToCamera = hitPosition;
                if (Vector3.Distance(traceStart, hitPosition) < Vector3.Distance(traceStart, farthestBlockedFromCamera)) farthestBlockedFromCamera = hitPosition;
                //if (Vector3.Distance(cameraTarget.transform.position, blockedPosition) > Vector3.Distance(cameraTarget.transform.position, idealCameraPosition)) return idealCameraPosition;
                //else return blockedPosition;
            } else if (x.collider.gameObject.GetComponent<NoCameraCollision>() != null) {
                //x.collider.gameObject.GetComponent<NoCameraCollision>().MakeTransparent();
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
        Hookshot hookshot = cameraTarget.GetComponent<Hookshot>();
        if (resetHookshotAim) {
            hookshotXY[0] = Screen.currentResolution.width / 2;
            hookshotXY[1] = Screen.currentResolution.height / 2;
            hookshot.aimTarget = gameObject.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(hookshotXY[0], hookshotXY[1], cameraTarget.GetComponent<Hookshot>().hookshotLength));
        }
        Vector2 camInput = input.Player.Look.ReadValue<Vector2>();
        if (Controls.Axis.InvertAimX) camInput[0] *= -1;
        if (Controls.Axis.InvertAimY) camInput[1] *= -1;
        //camInput[0] = (camInput[0] + 1) / 2;
        //camInput[1] = (camInput[1] + 1) / 2;
        hookshotXY[0] = Mathf.Clamp(hookshotXY[0] + camInput[0] * Time.deltaTime * Screen.currentResolution.width / hookshot.aimSpeedDenominator, 0, Screen.currentResolution.width);
        hookshotXY[1] = Mathf.Clamp(hookshotXY[1] + camInput[1] * Time.deltaTime * Screen.currentResolution.width / hookshot.aimSpeedDenominator, 0, Screen.currentResolution.height);
        hookshot.aimTarget = gameObject.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(hookshotXY[0], hookshotXY[1], cameraTarget.GetComponent<Hookshot>().hookshotLength));
        cameraTarget.GetComponent<GhostEffect>().isGhost = true;
        //hookshot.aimTarget = gameObject.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(camInput[0] * Screen.currentResolution.width, camInput[1] * Screen.currentResolution.height, cameraTarget.GetComponent<Hookshot>().hookshotLength));
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

    private void ActivateSightSpheres() {
        if (!sightSpheresActive) {
            foreach (GameObject x in cameraSpheres) {
                SphereCollider collider = x.GetComponent<SphereCollider>();
                collider.isTrigger = false;
                sightSpheresActive = true;
            }
        }
    }

    private void DeactivateSightSpheres() {
        if (sightSpheresActive) {
            for (int i = cameraSpheres.Count - 1; i >= 0; i--) {
                SphereCollider collider = cameraSpheres[i].GetComponent<SphereCollider>();
                collider.isTrigger = true;
                Vector3 desiredPosition = (i + 1) * outerCameraTraceRadius * idealCameraForward + (idealCameraForward * -1 * cameraDistance + cameraTarget.transform.position);
                //cameraSpheres[i].transform.position = desiredPosition;
                sightSpheresActive = false;
            }
        }
    }

    private void MoveSightSpheres() {
        //if (sightSpheresActive) {
            for (int i = cameraSpheres.Count - 1; i >= 0; i--) {
                //cameraSpheres[i].center = (i + 1) * 2.25f * outerCameraTraceRadius * new Vector3(0, 0, 1);
                Rigidbody sightSphere = cameraSpheres[i].GetComponent<Rigidbody>();
                Vector3 desiredPosition = (i + 1) * outerCameraTraceRadius * idealCameraForward + (idealCameraForward * -1 * cameraDistance + cameraTarget.transform.position + new Vector3(0, outerCameraTraceRadius, 0));
                float forceScale = Mathf.Clamp(Vector3.Distance(cameraSpheres[i].transform.position, desiredPosition) / 1, 0, 1);
                Vector3 sphereForceVector = (desiredPosition - cameraSpheres[i].transform.position).normalized * 0.3f * forceScale;
                sightSphere.AddForce(sphereForceVector);
                //sightSphere.transform.position = (i + 1) * 2.25f * outerCameraTraceRadius * transform.forward + transform.position;
                //Debug.DrawLine(cameraTarget.transform.position, cameraSpheres[i].transform.position, Color.green);
            }
        //} else {
        //    for (int i = cameraSpheres.Count - 1; i >= 0; i--) {
        //        Vector3 desiredPosition = (i + 1) * outerCameraTraceRadius * idealCameraForward + (idealCameraForward * -1 * cameraDistance + cameraTarget.transform.position);
        //        cameraSpheres[i].transform.position = desiredPosition;
        //    }
        //}
    }
    private Vector3 GetLerpPosition(float _lerpVal) {
        _lerpVal = Mathf.Clamp(_lerpVal, 0f, 1f);
        //Debug.DrawLine(outerHitPosition, outerHitIntersect, Color.red);
        //Vector3 idealCameraPosition = (transform.position - cameraTarget.transform.position).normalized * cameraDistance + cameraTarget.transform.position;
        float lerpValBetweenSpheres = _lerpVal * (cameraSpheres.Count) % 1;
        int sphereIdx = Mathf.FloorToInt(_lerpVal * (cameraSpheres.Count));
        //Vector3 lerpCameraPosition = Vector3.Lerp(directionToCamera * outerHitDistance + cameraTarget.transform.position, directionToCamera * cameraDistance + cameraTarget.transform.position, lerpVal);
        //Debug.Log("sphereIdx is " + sphereIdx + " ; cameraSpheres.Count is " + cameraSpheres.Count);
        Vector3 lerpStart = sphereIdx <= 0 ? idealCameraPosition : cameraSpheres[Mathf.Clamp(sphereIdx - 1, 0, cameraSpheres.Count - 1)].transform.position;
        Vector3 lerpEnd = sphereIdx >= cameraSpheres.Count ? cameraSpheres[cameraSpheres.Count - 1].transform.position : cameraSpheres[sphereIdx].transform.position;
        Vector3 lerpCameraPosition = Vector3.Lerp(lerpStart, lerpEnd, lerpValBetweenSpheres);
        //Debug.DrawLine(directionToCamera * outerHitDistance + cameraTarget.transform.position, transform.position, Color.green);
        //Vector3 lerpCameraPosition = Vector3.Lerp(cameraDirection * 1, transform.position, lerpVal);
        //float lerpDistance = Vector3.Distance(cameraTarget.transform.position, lerpCameraPosition);
        //Debug.Log("lerpVal: " + _lerpVal + " lerpCameraPosition: " + lerpCameraPosition);
        return lerpCameraPosition;
    }

    private Vector3 GetBlockedCameraPositionSphere() {
        //Vector3 idealCameraPosition = (transform.position - cameraTarget.transform.position).normalized * cameraDistance + cameraTarget.transform.position;
        Vector3 directionToCamera = (idealCameraPosition - cameraTarget.transform.position).normalized;
        float outerHitDistance = -1;
        float innerHitDistance = -1;
        Vector3 outerTraceStart = directionToCamera * outerCameraTraceRadius + cameraTarget.transform.position;
        //float outerTraceDistance = Vector3.Distance(outerTraceStart, directionToCamera * -1 * outerCameraTraceRadius + transform.position);
        float outerTraceDistance = Vector3.Distance(outerTraceStart, (directionToCamera * cameraDistance - outerTraceStart) + outerTraceStart);
        //Vector3 innerTraceStart = directionToCamera * innerCameraTraceRadius + cameraTarget.transform.position;
        //float innerTraceDistance = Vector3.Distance(innerTraceStart, directionToCamera * -1 * innerCameraTraceRadius + transform.position);
        Vector3 innerTraceStart = cameraTarget.transform.position;
        float innerTraceDistance = Vector3.Distance(innerTraceStart, (directionToCamera * cameraDistance - innerTraceStart) + innerTraceStart);
        Debug.DrawLine(outerTraceStart, directionToCamera * -1 * outerCameraTraceRadius + idealCameraPosition, Color.blue);
        RaycastHit[] cameraSightInner = Physics.SphereCastAll(innerTraceStart, innerCameraTraceRadius, directionToCamera, innerTraceDistance, 0b10000000, QueryTriggerInteraction.Ignore);
        foreach (RaycastHit x in cameraSightInner) {
            //if (x.collider != cameraTarget.GetComponent<CapsuleCollider>() && x.collider != GetComponent<SphereCollider>() && x.distance > 2) {
            if (x.collider != cameraTarget.GetComponent<CapsuleCollider>() && x.collider != GetComponent<SphereCollider>()) {
                //innerHitDistance = x.distance;
                DeactivateSightSpheres();
                cameraIsLerping = false;
                return GetLerpPosition(1);
            }
        }
        RaycastHit[] cameraSightOuter = Physics.SphereCastAll(outerTraceStart, outerCameraTraceRadius, directionToCamera, outerTraceDistance, 0b10000000, QueryTriggerInteraction.Ignore);
        foreach (RaycastHit x in cameraSightOuter) {
            //if (x.collider != cameraTarget.GetComponent<CapsuleCollider>() && x.collider != GetComponent<SphereCollider>() && x.distance > 2) {
            if (x.collider != cameraTarget.GetComponent<CapsuleCollider>() && x.collider != GetComponent<SphereCollider>()) {
                //if (x.collider != cameraTarget.GetComponent<CapsuleCollider>() && x.collider != GetComponent<SphereCollider>()) {
                outerHitDistance = x.distance;
                if (!cameraIsLerping) {
                    outerHitPosition = x.point;
                    cameraIsLerping = true;
                    ActivateSightSpheres();
                }
                break;
            }
        }
        //Debug.Log("outerHitDistance: " + outerHitDistance);
        //if (outerHitDistance >= 0) {
        if (innerHitDistance < 0 && outerHitDistance >= 0) {
            //if (outerHitDistance >= 0) {
            Vector3 idealCameraRight = Vector3.Cross(Vector3.up, idealCameraForward);
            //projecting to a vertical plane means that only horizontal camera movement lerps correctly
            Vector3 outerHitIntersect = Vector3.ProjectOnPlane(outerHitPosition - idealCameraPosition, idealCameraRight) + idealCameraPosition;
            float lerpVal = 1 - ((Vector3.Distance(outerHitPosition, outerHitIntersect) - innerCameraTraceRadius) / (outerCameraTraceRadius - innerCameraTraceRadius));
            Debug.DrawLine(outerHitPosition, outerHitIntersect, Color.red);
            //float lerpVal = 1 - (Vector3.Distance(outerHitPosition, outerHitIntersect) / outerCameraTraceRadius);
            return GetLerpPosition(lerpVal);
        }
        cameraIsLerping = false;
        DeactivateSightSpheres();
        //Debug.Log("Trace didn't hit wall");
        return idealCameraPosition;
    }




    private float GetCollisionDistance() {
        //Vector3 idealCameraPosition = (transform.position - cameraTarget.transform.position).normalized * cameraDistance + cameraTarget.transform.position;
        Vector3 directionToCamera = (transform.position - cameraTarget.transform.position).normalized;
        float outerHitDistance = -1;
        Vector3 outerHitPosition = new Vector3();
        float innerHitDistance = -1;
        Vector3 outerTraceStart = directionToCamera * outerCameraTraceRadius + cameraTarget.transform.position;
        //float outerTraceDistance = Vector3.Distance(outerTraceStart, directionToCamera * -1 * outerCameraTraceRadius + transform.position);
        float outerTraceDistance = Vector3.Distance(outerTraceStart, (directionToCamera * cameraDistance - outerTraceStart) + outerTraceStart);
        //Vector3 innerTraceStart = directionToCamera * innerCameraTraceRadius + cameraTarget.transform.position;
        //float innerTraceDistance = Vector3.Distance(innerTraceStart, directionToCamera * -1 * innerCameraTraceRadius + transform.position);
        Vector3 innerTraceStart = cameraTarget.transform.position;
        float innerTraceDistance = Vector3.Distance(innerTraceStart, (directionToCamera * cameraDistance - innerTraceStart) + innerTraceStart);
        Debug.DrawLine(outerTraceStart, directionToCamera * -1 * outerCameraTraceRadius + transform.position, Color.blue, 3);
        RaycastHit[] cameraSightOuter = Physics.SphereCastAll(outerTraceStart, outerCameraTraceRadius, directionToCamera, outerTraceDistance, 1, QueryTriggerInteraction.Ignore);
        foreach (RaycastHit x in cameraSightOuter) {
            if (x.collider != cameraTarget.GetComponent<CapsuleCollider>() && x.collider != GetComponent<SphereCollider>() && x.distance > 2) {
            //if (x.collider != cameraTarget.GetComponent<CapsuleCollider>() && x.collider != GetComponent<SphereCollider>()) {
                outerHitDistance = x.distance;
                outerHitPosition = x.point;
                break;
            }
        }
        //Debug.Log("outerHitDistance: " + outerHitDistance);
        //if (outerHitDistance >= 0) {
            RaycastHit[] cameraSightInner = Physics.SphereCastAll(innerTraceStart, innerCameraTraceRadius, directionToCamera, innerTraceDistance, 1, QueryTriggerInteraction.Ignore);
            foreach (RaycastHit x in cameraSightInner) {
                if (x.collider != cameraTarget.GetComponent<CapsuleCollider>() && x.collider != GetComponent<SphereCollider>() && x.distance > 2) {
                    innerHitDistance = x.distance;
                    break;
                }
            }
        //if (innerHitDistance < 0 && outerHitDistance >= 0) {
          if (outerHitDistance >= 0) {
            //projecting to a vertical plane means that only horizontal camera movement lerps correctly
            Vector3 outerHitIntersect = Vector3.ProjectOnPlane(outerHitPosition - transform.position, transform.right) + transform.position;
            Debug.DrawLine(outerHitPosition, outerHitIntersect, Color.red);
            float lerpVal = (Vector3.Distance(outerHitPosition, outerHitIntersect) - innerCameraTraceRadius) / (outerCameraTraceRadius - innerCameraTraceRadius);
            Vector3 lerpCameraPosition = Vector3.Lerp(directionToCamera * outerHitDistance + cameraTarget.transform.position, directionToCamera * cameraDistance + cameraTarget.transform.position, lerpVal);
            Debug.DrawLine(directionToCamera * outerHitDistance + cameraTarget.transform.position, transform.position, Color.green);
            //Vector3 lerpCameraPosition = Vector3.Lerp(cameraDirection * 1, transform.position, lerpVal);
            float lerpDistance = Vector3.Distance(cameraTarget.transform.position, lerpCameraPosition);
            //Debug.Log("lerpVal: " + lerpVal + " lerpDistance: " + lerpDistance);
            return lerpDistance < cameraDistance ? lerpDistance : cameraDistance;
        } else if (innerHitDistance >= 0) {
            //Debug.Log("Trace hit wall");
            return innerHitDistance;
        }
        //}
        //Debug.Log("Trace didn't hit wall");
        return cameraDistance;
    }
}

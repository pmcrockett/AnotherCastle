using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SightPerception : MonoBehaviour {
    public GameObject target;
    public float sightHorizAngle = 90;
    public float sightVertAngle = 90;
    public float sightDist = 10;
    public float sightOriginOffset = 0;
    public bool targetInSight = false;
    public bool isActive = true;
    public bool debugSightCone = false;
    private float castHorizCount;
    private float castVertCount;
    private Vector3 sightOrigin;
    private Vector3 castStartDir;
    private GameObject oldTarget;
    private Health health;
    private bool oldScanResult = false;

    private void Awake() {
        health = GetComponent<Health>();
    }
    // Start is called before the first frame update
    void Start() {
        if (target != null) {
            Init();
            oldTarget = target;
        }
    }

    // Update is called once per frame
    void Update() {
        if (target != null && target != oldTarget) {
            Init();
            oldTarget = target;
        }
        oldScanResult = targetInSight;
        if (health.health <= 0) {
            target = null;
            targetInSight = false;
        } else if (isActive && target != null) ScanForTarget(target);
        if (targetInSight && !oldScanResult) transform.Find("GuardAlerted").GetComponent<AudioSource>().Play();
    }

    private void Init() {
        castHorizCount = Mathf.Ceil((2 * Mathf.PI * sightDist * (sightHorizAngle / 360)) / target.GetComponent<CapsuleCollider>().radius);
        castVertCount = Mathf.Ceil((2 * Mathf.PI * sightDist * (sightVertAngle / 360)) / (target.GetComponent<CapsuleCollider>().height / 2));        
        Quaternion castVertRot = Quaternion.AngleAxis(sightVertAngle / 2, Vector3.right);
        Quaternion castHorizRot = Quaternion.AngleAxis(sightHorizAngle / -2, Vector3.up);
        castStartDir = castVertRot * castHorizRot * Vector3.forward;
    }

    private void ScanForTarget(GameObject _target) {
        targetInSight = false;
        if (Vector3.Distance(_target.transform.position, transform.position) > sightDist) return;
        sightOrigin = new Vector3(transform.position.x, transform.position.y + sightOriginOffset, transform.position.z);
        RaycastHit sightHit;
        for (int i = 0; i < castVertCount; i++) {
            for (int j = 0; j < castHorizCount; j++) {
                Vector3 castDir = Quaternion.FromToRotation(Vector3.forward, transform.forward) 
                    * Quaternion.Euler(0, (sightHorizAngle / castHorizCount) * j, 0) 
                    * Quaternion.Euler((sightVertAngle / castVertCount * -1) * i, 0, 0) 
                    * castStartDir;
                if (Physics.Raycast(sightOrigin, castDir, out sightHit, sightDist, 0b11000000, QueryTriggerInteraction.Ignore)) {
                    if (sightHit.collider.gameObject == _target) {
                        if (debugSightCone) Debug.DrawLine(sightOrigin, sightOrigin + castDir * sightDist, Color.green);
                        targetInSight = true;
                        break;
                    } else if (debugSightCone) Debug.DrawLine(sightOrigin, sightOrigin + castDir * sightDist, Color.red);
                } else if (debugSightCone) Debug.DrawLine(sightOrigin, sightOrigin + castDir * sightDist, Color.yellow);
            }
            if (targetInSight) break;
        }
    }
}

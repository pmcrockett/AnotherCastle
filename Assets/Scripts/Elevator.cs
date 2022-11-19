using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    private Vector3 lastPosition;
    private List<GameObject> carriedObjects = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        lastPosition = transform.position;   
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position != lastPosition) {
            foreach (GameObject obj in carriedObjects) {
                obj.transform.position += transform.position - lastPosition;
            }
            lastPosition = transform.position;
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject != gameObject) {
            carriedObjects.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other) {
        carriedObjects.Remove(other.gameObject);
    }
}

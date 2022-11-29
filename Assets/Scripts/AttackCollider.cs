using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCollider : MonoBehaviour
{
    private Attack attack;

    private void Awake() {
        attack = transform.parent.transform.parent.transform.parent.transform.parent.transform.parent.transform.parent.GetComponent<Attack>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other) {
        attack.AttackCollision(other.gameObject, true);
        Debug.Log("Sword hit " + other.name);
    }
}

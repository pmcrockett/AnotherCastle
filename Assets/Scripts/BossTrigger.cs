using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    public GameObject boss;
    public GameObject bossArena;
    public Camera bossCam;
    public bool isTriggered = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (bossCam != null) {
            bossCam.transform.rotation = Quaternion.LookRotation(boss.transform.position - bossCam.transform.position);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.GetComponent<SwitchTrigger>() != null && !isTriggered) {
            isTriggered = true;
            boss = Instantiate(boss, bossArena.transform);
            boss.transform.position = bossArena.transform.position + new Vector3(0, 75, 0);
            boss.transform.rotation = Quaternion.Euler(0, 180, 0);
            boss.GetComponent<SightPerception>().target = other.gameObject;
        }
    }
}

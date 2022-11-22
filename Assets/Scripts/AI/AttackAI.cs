using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAI : MonoBehaviour
{
    private ChaseAI movement;
    private GameObject target;
    private Attack attack;
    // Start is called before the first frame update
    void Start()
    {
        movement = GetComponent<ChaseAI>();
        attack = GetComponent<Attack>();
    }

    // Update is called once per frame
    void Update()
    {
        if (movement.isInTargetRange && !attack.isAttacking) {
            attack.StartAttack(movement.target);
        }
    }
}

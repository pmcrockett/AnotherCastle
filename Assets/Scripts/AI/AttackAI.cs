using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAI : MonoBehaviour
{
    private ChaseAI movement;
    private Attack attack;
    private Health health;
    // Start is called before the first frame update
    void Start()
    {
        health = GetComponent<Health>();
        movement = GetComponent<ChaseAI>();
        attack = GetComponent<Attack>();
        attack.isEnabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (health.health > 0) {
            if (movement.target != null && movement.target.GetComponent<InputHandler>().activeDialog == null
                && (movement.target.GetComponent<Health>() == null || movement.target.GetComponent<Health>().health > 0)) {
                if (movement.isInTargetRange && !attack.isAttacking) {
                    attack.StartAttack(movement.target);
                }
            } else if (attack.isAttacking) attack.InterruptAttack();
        }
    }
}

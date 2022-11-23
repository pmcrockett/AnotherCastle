using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAI : MonoBehaviour
{
    private ChaseAI movement;
    private Attack attack;
    // Start is called before the first frame update
    void Start()
    {
        movement = GetComponent<ChaseAI>();
        attack = GetComponent<Attack>();
        attack.isEnabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (movement.target != null && movement.target.GetComponent<InputHandler>().activeDialog == null) {
            if (movement.isInTargetRange && !attack.isAttacking) {
                attack.StartAttack(movement.target);
            }
        } else if (attack.isAttacking) attack.InterruptAttack();
    }
}

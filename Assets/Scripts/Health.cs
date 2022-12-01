using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public int health = 3;
    public bool isPlayer;
    public HealthDisplay display;
    private Animator anim;
    private GameState state = null;

    private void Awake() {
        anim = GetComponent<Animator>();
        if (isPlayer) {
            state = GameObject.Find("GameState").GetComponent<GameState>();
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Kill() {
        health = 0;
        anim.SetBool("isDead", true);
        transform.Find("Die").GetComponent<AudioSource>().Play();
        if (isPlayer && state != null) {
            state.Death();
        }
    }

    public bool ApplyDamage(int _amount) {
        if (health > 0) {
            health -= _amount;
            bool isDead = health <= 0;
            if (isDead) Kill();
            else transform.Find("Hurt").GetComponent<AudioSource>().Play();
            if (display != null) display.Redraw();
            return isDead;
        } else return true;
    }
}

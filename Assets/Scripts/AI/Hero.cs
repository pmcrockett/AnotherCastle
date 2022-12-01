using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour
{
    public float walkSpeed = 2;
    private Animator anim;

    private void Awake() {
        anim = GetComponent<Animator>();
    }
    // Start is called before the first frame update
    void Start()
    {
        anim.SetFloat("walkSpeed", walkSpeed);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

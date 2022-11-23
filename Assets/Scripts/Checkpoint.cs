using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public GameState gameState;
    private GameObject triggeringObject;

    private void Awake() {
        triggeringObject = gameState.playerObj;
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
        if (other.gameObject == triggeringObject) {
            gameState.currentCheckpoint = this;
        }
    }
}

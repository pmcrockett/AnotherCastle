using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingTrigger : MonoBehaviour
{
    private bool gameHasEnded = false;
    public GameObject triggeringObj;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject == triggeringObj && !gameHasEnded) {
            gameHasEnded = true;
            if (GameObject.Find("GameState") != null) {
                GameObject.Find("GameState").GetComponent<GameState>().AWinnerIsYou();
            } else {
                Debug.LogWarning("EndingTrigger.cs: Trying to access GameState but it doesn't exist.");
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameState : MonoBehaviour
{
    public GameObject playerObj;
    public Camera mainCamera;
    public Canvas uiCanvas;
    public Checkpoint currentCheckpoint;
    public GameObject error;
    public float errorTimer;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Killplane();
        if (error!= null) {
            if (Time.time - errorTimer > 7) Object.Destroy(error);
        }
    }

    private void Respawn() {
        if (currentCheckpoint != null) {
            playerObj.transform.position = currentCheckpoint.transform.position;
            playerObj.transform.rotation = currentCheckpoint.transform.rotation;
        } else {
            error = Instantiate(new GameObject(), uiCanvas.transform);
            error.AddComponent<TextMeshProUGUI>();
            error.AddComponent<RectTransform>();
            error.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
            error.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
            error.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
            error.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            error.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
            error.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
            error.GetComponent<TextMeshProUGUI>().text = "NullReferenceException: Object reference currentCheckpoint not set to an instance of an object\nGameState.Respawn () (at Assets/Scripts/GameState.cs:22)";
            errorTimer = Time.time;
            playerObj.transform.position = new Vector3(25, 1000, 108);
            playerObj.GetComponent<Rigidbody>().velocity = Vector3.zero;
            playerObj.GetComponent<InputHandler>().disableMoveImpulse = false;
            playerObj.transform.rotation = Quaternion.identity;
        }
        mainCamera.GetComponent<CameraBehavior>().Respawn(currentCheckpoint);
    }

    private void Killplane() {
        if (playerObj.transform.position.y < -500) {
            Respawn();
        }
    }
}

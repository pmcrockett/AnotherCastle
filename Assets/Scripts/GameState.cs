using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System;
using System.Reflection;

public class GameState : MonoBehaviour
{
    public GameObject playerObj;
    public Camera mainCamera;
    public Canvas uiCanvas;
    public Checkpoint currentCheckpoint;
    public GameObject error;
    public float errorTimer;
    public GameObject introWizard;
    public GameObject introPrincess;
    public GameObject nameInput;
    public string heroName;
    public GameObject hero;
    private delegate IntroStep IntroStep();
    private Camera introCam = null;
    private float introTimer;
    private GameObject introDialog;
    private PlayerInput input;
    private float introInput = 0;
    private bool introInputHeld = false;
    private float lastTextInputTime = -1;
    private float camGlitchSpeed = 0.6f;

    IntroStep introStep;

    void Awake() {
        input = new PlayerInput();
    }
    // Start is called before the first frame update
    void Start() {
        playerObj.GetComponent<InputHandler>().enabled = false;
        mainCamera.enabled = false;
        introStep = GetIntroStep(1);
        introPrincess.GetComponent<LiftTarget>().StartLift(introWizard);
        introPrincess.GetComponent<Animator>().SetBool("isLifted", true);
        introWizard.GetComponent<Animator>().SetBool("liftStart", true);
        introWizard.GetComponent<Animator>().SetFloat("liftSpeed", introPrincess.GetComponent<LiftTarget>().liftSpeed);
    }
    void OnEnable() {
        input.Player.Enable();
    }

    void OnDisable() {
        input.Player.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        introInput = input.Player.Attack.ReadValue<float>() > 0 ? 1 : input.Player.Jump.ReadValue<float>() > 0 ? 1 : input.Player.Interact.ReadValue<float>();
        Killplane();
        if (error!= null) {
            if (Time.time - errorTimer > 7) UnityEngine.Object.Destroy(error);
        }

        if (introStep != null) {
            introTimer += Time.deltaTime;
            introStep = introStep();
        }
        if (introInput > 0) introInputHeld = true;
        else introInputHeld = false;
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
    private void DisableIntroCam(int idx) {
        for (int i = idx; i > 0; i--) {
            GameObject cam = GameObject.Find("IntroCam" + i);
            if (cam != null) {
                //Destroy(GameObject.Find("IntroCam" + idx));
                cam.GetComponent<Camera>().enabled = false;
                cam.GetComponent<Camera>().depth = 0;
            }
        }
    }
    private void UpdateIntroCam(int idx) {
        DisableIntroCam(idx - 1);
        introCam = GameObject.Find("IntroCam" + idx.ToString()).GetComponent<Camera>();
        introCam.enabled = true;
        introCam.depth = 1;
    }
    private IntroStep GetIntroStep(int idx) {
        MethodInfo info = GetType().GetMethod("Intro" + idx.ToString(), BindingFlags.Instance | BindingFlags.NonPublic);
        return (IntroStep)Delegate.CreateDelegate(typeof(IntroStep), this, info);
    }

    private bool CanAdvanceDialog() {
        return introTimer > playerObj.GetComponent<InputHandler>().dialogInputCooldown && !introInputHeld && introInput > 0;
    }
    private IntroStep Intro1() {
        int idx = 1;
        if (introTimer >= 0) {
            introTimer = 0;
            GameObject.Find("IntroCam" + idx).GetComponent<Camera>().enabled = true;
            GameObject.Find("IntroCam" + idx).GetComponent<Camera>().depth = 1;
            Debug.Log("Intro" + idx);
            //return Intro2;
            return GetIntroStep(idx + 1);
        }
        return GetIntroStep(idx);
    }

    private IntroStep Intro2() {
        int idx = 2;
        if (introTimer >= 1) {
            introTimer = 0;
            DisableIntroCam(idx - 1);
            List<string> dialogText = new List<string> { "Once upon a time there was a beautiful PRINCESS who ruled her kingdom with wisdom and compassion.", "" };
            List<Camera> cameras = new List<Camera> { GameObject.Find("IntroCam" + idx).GetComponent<Camera>() };
            introDialog = Util.CreateDialogBox(uiCanvas, dialogText, cameras);
            Debug.Log("Intro" + idx);
            return GetIntroStep(idx + 1);
        }
        return GetIntroStep(idx);
    }
    
    private IntroStep Intro3() {
        int idx = 3;
        if (CanAdvanceDialog()) {
            introTimer = 0;
            Destroy(introDialog);
            List<string> dialogText = new List<string> { "But darkness fell over the land, and the princess was kidnapped by the evil wizard, GROMDAK ...", "" };
            List<Camera> cameras = new List<Camera> { GameObject.Find("IntroCam" + idx).GetComponent<Camera>() };
            introDialog = Util.CreateDialogBox(uiCanvas, dialogText, cameras);
            Debug.Log("IntroCam" + idx);
            return GetIntroStep(idx + 1);
        }
        return GetIntroStep(idx);
    }

    private IntroStep Intro4() {
        int idx = 4;
        if (CanAdvanceDialog()) {
            introTimer = 0;
            Destroy(introDialog);
            UpdateIntroCam(idx);
            introCam.transform.parent = introWizard.transform;
            introWizard.GetComponent<Animator>().SetBool("liftStart", false);
            Debug.Log("IntroCam" + idx, this);
            return GetIntroStep(idx + 1);
        }
        return GetIntroStep(idx);
    }
    private IntroStep Intro5() {
        int idx = 5;
        if (introTimer >= 4) {
            introTimer = 0;
            List<string> dialogText = new List<string> { "... to live out her days in a cold, dark dungeon.", "" };
            //List<Camera> cameras = new List<Camera> { GameObject.Find("IntroCam" + idx).GetComponent<Camera>() };
            List<Camera> cameras = new List<Camera>();
            introDialog = Util.CreateDialogBox(uiCanvas, dialogText, cameras);
            Debug.Log("IntroCam" + idx, this);
            return GetIntroStep(idx + 1);
        } else {
            if (introWizard.transform.localPosition.x < 24) {
                introWizard.GetComponent<Rigidbody>().AddForce(introWizard.transform.forward * 1000 * Time.deltaTime);
            } else {
                if (Vector3.Dot(introWizard.transform.forward, new Vector3(0, 0, 1)) < 0.999) {
                    introCam.transform.parent = null;
                    introWizard.transform.rotation = Quaternion.RotateTowards(introWizard.transform.rotation, Quaternion.Euler(0, 0, 1), 180 * Time.deltaTime);
                } else if (introPrincess.GetComponent<LiftTarget>().isLifted) {
                    introPrincess.GetComponent<LiftTarget>().EndLift(new Vector3(0, 0, 1), Vector3.zero);
                    introPrincess.GetComponent<Animator>().SetBool("isLifted", false);
                    introWizard.GetComponent<Animator>().SetBool("throwStart", true);
                } else {
                    //introPrincess.GetComponent<CapsuleCollider>().isTrigger = true;
                    introPrincess.transform.rotation = Quaternion.RotateTowards(introPrincess.transform.rotation, Quaternion.Euler(0, 0, 1), 360 * Time.deltaTime);
                    introWizard.GetComponent<Animator>().SetBool("throwStart", false);
                }
            }
            introWizard.GetComponent<Animator>().SetFloat("walkSpeed", introWizard.GetComponent<Rigidbody>().velocity.magnitude / 7);
        }
        return GetIntroStep(idx);
    }

    private IntroStep Intro6() {
        int idx = 6;
        if (CanAdvanceDialog()) {
            introTimer = 0;
            Destroy(introDialog);
            Destroy(introWizard);
            List<string> dialogText = new List<string> { "But in the kingdom's hour of greatest need, a hero arose.", "" };
            List<Camera> cameras = new List<Camera> { GameObject.Find("IntroCam" + idx).GetComponent<Camera>() };
            introDialog = Util.CreateDialogBox(uiCanvas, dialogText, cameras);
            UpdateIntroCam(idx);
            Debug.Log("IntroCam" + idx, this);
            return GetIntroStep(idx + 1);
        }
        return GetIntroStep(idx);
    }

    private IntroStep Intro7() {
        int idx = 7;
        if (CanAdvanceDialog()) {
            introTimer = 0;
            Destroy(introDialog);
            List<string> dialogText = new List<string> { "Please tell me your name, hero:", "" };
            List<Camera> cameras = new List<Camera>();
            introDialog = Util.CreateDialogBox(uiCanvas, dialogText, cameras);
            introDialog.transform.Find("DialogMore").GetComponent<TextMeshProUGUI>().text = "";
            nameInput = Instantiate(nameInput, introDialog.transform);
            nameInput.GetComponent<TMP_InputField>().Select();
            nameInput.GetComponent<TMP_InputField>().ActivateInputField();
            //nameInput.GetComponent<TMP_InputField>();
            Debug.Log("IntroCam" + idx, this);
            return GetIntroStep(idx + 1);
        }
        return GetIntroStep(idx);
    }
    private IntroStep Intro8() {
        int idx = 8;
        if (nameInput.GetComponent<TMP_InputField>().text.Length >= 1 && lastTextInputTime < 0) {
            lastTextInputTime = Time.time + 0.16f;
        }
        if (Time.time - lastTextInputTime > 0.08 && nameInput.GetComponent<TMP_InputField>().text.Length >= 1 && nameInput.GetComponent<TMP_InputField>().text.Length < 32) {
            //nameInput.GetComponent<TMP_InputField>().SetTextWithoutNotify(nameInput.GetComponent<TMP_InputField>().text + nameInput.GetComponent<TMP_InputField>().text[nameInput.GetComponent<TMP_InputField>().textComponent.text.Length - 2]);
            nameInput.GetComponent<TMP_InputField>().text += nameInput.GetComponent<TMP_InputField>().text[nameInput.GetComponent<TMP_InputField>().textComponent.text.Length - 2];
            lastTextInputTime = Time.time;
            nameInput.GetComponent<TMP_InputField>().caretPosition = nameInput.GetComponent<TMP_InputField>().text.Length;
            //nameInput.GetComponent<TMP_InputField>().DeactivateInputField();
            //Debug.Log(nameInput.GetComponent<TMP_InputField>().textComponent.text + "; length: " + nameInput.GetComponent<TMP_InputField>().textComponent.text.Length);
        } else if (nameInput.GetComponent<TMP_InputField>().text.Length >= 32) {
            heroName = nameInput.GetComponent<TMP_InputField>().textComponent.text;
            introTimer = 0;
            Destroy(introDialog);
            List<string> dialogText = new List<string> { "Yes, " + heroName + " is a very heroic name!", "" };
            List<Camera> cameras = new List<Camera>();
            introDialog = Util.CreateDialogBox(uiCanvas, dialogText, cameras);
            //introDialog.transform.Find("DialogMore").GetComponent<TextMeshProUGUI>().text = "";
            Destroy(nameInput);
            Debug.Log("IntroCam" + idx, this);
            return GetIntroStep(idx + 1);
        }
        return GetIntroStep(idx);
   }

    private IntroStep Intro9() {
        int idx = 9;
        if (CanAdvanceDialog()) {
            introTimer = 0;
            Destroy(introDialog);
            introPrincess.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -1));
            List<string> dialogText = new List<string> { "PRINCESS:\nPlease save me, " + heroName + "!" };
            List<Camera> cameras = new List<Camera> { GameObject.Find("IntroCam" + idx).GetComponent<Camera>() };
            introDialog = Util.CreateDialogBox(uiCanvas, dialogText, cameras);
            introCam = GameObject.Find("IntroCam" + idx.ToString()).GetComponent<Camera>();
            Debug.Log("IntroCam" + idx, this);
            return GetIntroStep(idx + 1);
        }
        return GetIntroStep(idx);
    }

    private IntroStep Intro10() {
        int idx = 10;
        if (CanAdvanceDialog()) {
            introTimer = 0;
            Destroy(introDialog);
            //introCam.transform.parent = introPrincess.transform;
            Debug.Log("IntroCam" + idx, this);
            return GetIntroStep(idx + 1);
        }
        if (introCam.transform.position.y > introPrincess.transform.position.y) {
            introCam.transform.position = new Vector3(introCam.transform.position.x, introCam.transform.position.y - camGlitchSpeed * Time.deltaTime, introCam.transform.position.z);
        } else {
            introCam.transform.position = new Vector3(introCam.transform.position.x, introCam.transform.position.y + 1, introCam.transform.position.z);
            camGlitchSpeed = Mathf.Min(camGlitchSpeed *= 1.3f, 15);
        }
        introCam.transform.rotation = Quaternion.LookRotation(introPrincess.transform.position - introCam.transform.position);
        return GetIntroStep(idx);
    }
    
    private IntroStep Intro11() {
        int idx = 11;
        if (introTimer >= 4) {
            introTimer = 0;
            DisableIntroCam(9);
            playerObj.GetComponent<InputHandler>().enabled = true;
            //mainCamera.GetComponent<CameraBehavior>().enabled = true;
            mainCamera.enabled = true;
            playerObj.transform.position = new Vector3(introPrincess.transform.position.x, introPrincess.transform.position.y + 0.05f, introPrincess.transform.position.z);
            playerObj.transform.rotation = introPrincess.transform.rotation;
            Destroy(introPrincess);
            Debug.Log("IntroCam" + idx, this);
            return null;
        } else if (Vector3.Dot(introCam.transform.forward, Vector3.down) >= 0.999) {
            introPrincess.transform.position = new Vector3(introPrincess.transform.position.x, introPrincess.transform.position.y, introPrincess.transform.position.z - 0.1f);
            introCam.transform.rotation = Quaternion.LookRotation(introPrincess.transform.position - introCam.transform.position);
        } else {
            introCam.transform.rotation = Quaternion.RotateTowards(introCam.transform.rotation, Quaternion.Euler(90, 0, 0), 1000 * Time.deltaTime);
            Debug.Log(Vector3.Dot(introCam.transform.forward, Vector3.down));
        }
        return GetIntroStep(idx);
    }
}

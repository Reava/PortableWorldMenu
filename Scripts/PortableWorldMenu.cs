using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using UnityEngine.UI;

/*
 ///////////////////////// //////////////////////////////////////////////////// /////////////////////////
  THIS SCRIPT IS NOT MEANT TO BE EDITED UNLESS YOU KNOW EXACTLY WHAT YOU ARE DOING, PLEASE REFER TO DOCS

                       Script by Reava_, helped by wonderful peeps @ Prefabs. 
     Extra Thanks to Nestor for the general Udon help & Superbstingray for the Avatar Utility Script
 ///////////////////////// //////////////////////////////////////////////////// /////////////////////////
*/

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class PortableWorldMenu : UdonSharpBehaviour
{
    [Space]
    [Header("Settings")]
    [Range(0f, 5f)]
    [SerializeField] private float holdTimeSeconds = 1.5f;
    [Space]
    [Header("Menus stuff")]
    [Tooltip("Resets to the default tab of the menu when the menu closes")]
    public bool resetTabOnExit = false;
    [SerializeField] private int defaultMenuTab = 0;
    private int maxMenuNum = 5;
    [Tooltip("Max # of menus is 5, please refer to documentation on how to edit this value.")]
    [SerializeField] private GameObject[] MenusList = new GameObject[5];
    [Space]
    [Header("References")]
    [SerializeField] private Image ProgressIndicator;
    [SerializeField] private GameObject popupIndicator;
    [SerializeField] private GameObject CanvasTargetPosition;
    [SerializeField] private GameObject CanvasTargetRotation;
    [SerializeField] private GameObject UIContainer;
    [SerializeField] private GameObject UI_ActiveIndicator;
    [SerializeField] private GameObject MainCanvas;
    [Space]
    [Header("Audio")]
    public bool playAudioFeedback = true;
    [SerializeField] private AudioSource AudioFeedbackSource;
    [SerializeField] private AudioClip AudioclipMenuOpen;
    [SerializeField] private AudioClip AudioclipMenuClose;
    [SerializeField] private AudioClip AudioclipMenuChange;
    private Vector3 defaultIndicatorPos = new Vector3(0f, 13.75f, 0f); //This is the default position for the selector when menu 0 is selected
    private float IndicatorHeight = 7f;
    private bool isValidRefs = true;
    private bool state = false;
    private float currentHeld;
    private GameObject SelectedMenuCanvas;
    private GameObject ListedMenuCanvas;
    private float detectedScale = 1f;

    void Start()
    {
        if (!ProgressIndicator || !popupIndicator || !CanvasTargetPosition || !UIContainer || !CanvasTargetRotation || MenusList.Length > maxMenuNum) _sendDebugError("Missing Reference");
        if(playAudioFeedback) if(!AudioclipMenuOpen || !AudioclipMenuClose || !AudioclipMenuChange) _sendDebugError("Missing Audio Clip/Source");
        foreach (GameObject o in MenusList)
        {
            if(o) if (!o.GetComponent<Canvas>() || !o.GetComponent<BoxCollider>() || !o.GetComponent<GraphicRaycaster>()) isValidRefs = false;
        }
        if (!MainCanvas.GetComponent<Canvas>() || !MainCanvas.GetComponent<BoxCollider>() || !MainCanvas.GetComponent<GraphicRaycaster>()) isValidRefs = false;
        if (!isValidRefs) { UIContainer.SetActive(false); _sendDebugError("Invalid References"); } else { _DespawnMenu(); }
        popupIndicator.SetActive(false);
    }

    public void OnPlayerRespawn(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            if (isValidRefs) _DespawnMenu();
            popupIndicator.SetActive(false);
        }
    }

    public void _scaleChange(float scale)
    {
        detectedScale = scale;
        //do smth to scale things properly
    }

    public void Update()
    {
        if (!isValidRefs) return;
        if (Input.GetKeyDown(KeyCode.E)) if (!state) _spawnMenu(); else _DespawnMenu();
        if (Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryThumbstickVertical") < -0.95f || Input.GetKey(KeyCode.Keypad3))
        {
            if (currentHeld == 0 && !state)
            {
                popupIndicator.transform.SetPositionAndRotation(CanvasTargetPosition.transform.position, CanvasTargetRotation.transform.rotation);
                popupIndicator.SetActive(true);
            }
            currentHeld += Time.deltaTime;
            if (currentHeld > holdTimeSeconds)
            {
                if (!state) _spawnMenu();
                currentHeld = 0f;
            }
            ProgressIndicator.fillAmount = currentHeld / holdTimeSeconds;
        }
        else
        {
            currentHeld = 0f;
            ProgressIndicator.fillAmount = 0f;
            popupIndicator.SetActive(false);
        }
        if (Input.GetButton("Oculus_CrossPlatform_Button4") || Input.GetButton("Oculus_CrossPlatform_Button2") || Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryThumbstickHorizontal") != 0 || Input.GetButton("Horizontal") || Input.GetButton("Vertical") || Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryThumbstickVertical") > 0.95f)
        {
            _DespawnMenu();
        }
    }

    public void _spawnMenu()
    {
        if (!isValidRefs) return;
        state = true;
        if (resetTabOnExit) _ChangeMenuTo(defaultMenuTab);
        UIContainer.transform.SetPositionAndRotation(CanvasTargetPosition.transform.position, CanvasTargetRotation.transform.rotation);
        popupIndicator.SetActive(false);
        MainCanvas.GetComponent<Canvas>().enabled = true;
        MainCanvas.GetComponent<GraphicRaycaster>().enabled = true;
        MainCanvas.GetComponent<BoxCollider>().enabled = true;
        if (playAudioFeedback) AudioFeedbackSource.PlayOneShot(AudioclipMenuOpen);
    }

    public void _DespawnMenu()
    {
        if (!isValidRefs) return;
        state = false;
        MainCanvas.GetComponent<Canvas>().enabled = false;
        MainCanvas.GetComponent<GraphicRaycaster>().enabled = false;
        MainCanvas.GetComponent<BoxCollider>().enabled = false;
        if (playAudioFeedback) AudioFeedbackSource.PlayOneShot(AudioclipMenuClose);
    }

    public void _ChangeMenuTo(int menuSelection)
    {
        if (!isValidRefs || menuSelection >= maxMenuNum) return;
        if (MenusList.Length != 0)
        {

            if (MenusList[menuSelection])
            {
                MenusList[menuSelection].GetComponent<Canvas>().enabled = true;
                MenusList[menuSelection].GetComponent<GraphicRaycaster>().enabled = true;
                MenusList[menuSelection].GetComponent<BoxCollider>().enabled = true;
            }
            else
            {
                _sendDebugError("Invalid Menu Selected");
                return;
            }
            foreach (GameObject menu in MenusList)
            {
                if (menu)
                {
                    menu.GetComponent<Canvas>().enabled = false;
                    menu.GetComponent<GraphicRaycaster>().enabled = false;
                    menu.GetComponent<BoxCollider>().enabled = false;
                }
            }
            UI_ActiveIndicator.transform.localPosition = new Vector3(defaultIndicatorPos.x, defaultIndicatorPos.y - (IndicatorHeight * menuSelection), defaultIndicatorPos.z);
            if (playAudioFeedback) AudioFeedbackSource.PlayOneShot(AudioclipMenuChange);
        }
        else
        {
            _sendDebugError("No Menus Found");
        }
    }

    private void _sendDebugError(string errorReported) => Debug.LogError("<color=white>Reava_UwUtils:<color=red> <b>" + errorReported + "</b></color>, please review <color=orange>References / Settings</color> on: " + gameObject + ".</color>", gameObject);
}
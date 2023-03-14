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
    [Tooltip("Keybind for desktop users")]
    [SerializeField] private KeyCode KeybindDesktop = KeyCode.E;
    [SerializeField] private bool useAudioFeedback = true;
    [Tooltip("Resets to the default tab of the menu when the menu closes")]
    public bool resetTabOnExit = false;
    [Range(0f, 5f)]
    [SerializeField] private float holdTimeSeconds = 1.5f;
    [Range(0, 4)]
    [SerializeField] private int defaultMenuTab = 0;
    private int maxMenuNum = 5;
    [Tooltip("Max # of menus is 5, please refer to documentation on how to edit this value.")]
    [SerializeField] private GameObject[] MenusList = new GameObject[5];
    [Space]
    [Header("References")]
    [SerializeField] private Image ProgressIndicator;
    [SerializeField] private GameObject popupIndicator;
    [SerializeField] private GameObject HandPosition;
    [SerializeField] private GameObject HeadPosition;
    [SerializeField] private GameObject DesktopTargetPosition;
    [SerializeField] private GameObject UI_ActiveIndicator;
    [SerializeField] private GameObject MainCanvas;
    [Space]
    [Header("Audio references")]
    [SerializeField] private AudioSource AudioFeedbackSource;
    [SerializeField] private AudioClip AudioclipMenuOpen;
    [SerializeField] private AudioClip AudioclipMenuClose;
    [SerializeField] private AudioClip AudioclipMenuChange;
    [Space]
    [Header("Advanced settings, only edit if you know what you're doing!")]
    [Tooltip("This is the default position for the selector when menu 0 is selected")]
    [SerializeField] private Vector3 defaultIndicatorPos = new Vector3(0f, 13.75f, 0f); //This is the default position for the selector when menu 0 is selected
    [Tooltip("Indicator offset substracted to the default position, multiplied by menu selection.")]
    [SerializeField] private Vector3 IndicatorOffset = new Vector3(0f, 7f, 0f);
    [Tooltip("Canvas Offset added to the transform of the hand target.")]
    [SerializeField] private Vector3 CanvasOffset = new Vector3(0f, 0f, 0f);
    [Tooltip("Default Scale of the UI. (Warning: scales to the scale detected of the avatar)")]
    [Range(0.1f, 6f)]
    [SerializeField] private float SystemScale = 1f;
    private bool isValidRefs = true;
    private bool state = false;
    private float currentHeld;
    private int SelectedMenu = 0;
    private GameObject ListedMenuCanvas;
    private Vector3 detectedScale = new Vector3(1f, 1f, 1f);

    void Start()
    {
        if (!ProgressIndicator || !popupIndicator || !HandPosition || !HeadPosition || !UI_ActiveIndicator || !MainCanvas) { _sendDebugError("Missing Main Reference(s)",64); isValidRefs = false; }
        if (useAudioFeedback) if (!AudioclipMenuOpen || !AudioclipMenuClose || !AudioclipMenuChange) { _sendDebugError("Missing Audio Clip/Source",65);}
        int itemp = 0;
        foreach (GameObject o in MenusList)
        {
            if (o)
            {
                if (!o.GetComponent<Canvas>() || !o.GetComponent<BoxCollider>() || !o.GetComponent<GraphicRaycaster>())
                {
                    _sendDebugError("Invalid Menu #" + itemp + " <color=white>('" + o.gameObject.name + "'),</color> it will be ignored", 68);
                }
            }
            itemp += 1;
        }
        if (!MainCanvas.GetComponent<Canvas>() || !MainCanvas.GetComponent<BoxCollider>() || !MainCanvas.GetComponent<GraphicRaycaster>()) { _sendDebugError("Missing component on Main Canvas",73); isValidRefs = false; }
        if (!isValidRefs) { MainCanvas.SetActive(false); _sendDebugError("Invalid setup, disabling Menu now..", 74); _disableSelf(); } else { _DespawnMenu(); popupIndicator.SetActive(false); }
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
        Debug.LogWarning("PWM: Scale change detected:" + scale + " - Object:");
        CanvasOffset = CanvasOffset * scale;
        popupIndicator.transform.localScale = popupIndicator.transform.localScale  * (scale * SystemScale);
        MainCanvas.transform.localScale = MainCanvas.transform.localScale * (scale * SystemScale);
        DesktopTargetPosition.transform.localPosition = new Vector3(0f, 0f, 0.16f * scale);
    }

    public void Update()
    {
        if (!isValidRefs) return;
        if (Input.GetKeyDown(KeybindDesktop)) if (!state) _spawnMenu(false); else _DespawnMenu();
        if (Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryThumbstickVertical") < -0.95f || Input.GetKey(KeyCode.Keypad3))
        {
            if (currentHeld == 0 && !state)
            {
                popupIndicator.transform.SetPositionAndRotation(HandPosition.transform.position + CanvasOffset, Quaternion.Euler(new Vector3(HeadPosition.transform.rotation.x, 0f, HeadPosition.transform.rotation.z)));
                popupIndicator.SetActive(true);
            }
            currentHeld += Time.deltaTime;
            if (currentHeld > holdTimeSeconds)
            {
                if (!state) _spawnMenu(true);
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
            if (state) _DespawnMenu();
        }
    }

    public void _spawnMenu(bool isVR)
    {
        if (!isValidRefs) return;
        state = true;
        if (resetTabOnExit && defaultMenuTab <= 4) _ChangeMenuTo(defaultMenuTab);
        if (isVR){
            MainCanvas.transform.SetPositionAndRotation((HandPosition.transform.position + CanvasOffset), Quaternion.Euler(HeadPosition.transform.rotation.x, 0f, HeadPosition.transform.rotation.z));
        }
        else
        {
            MainCanvas.transform.SetPositionAndRotation(DesktopTargetPosition.transform.position, HeadPosition.transform.rotation);
        }
        popupIndicator.SetActive(false);
        MainCanvas.GetComponent<Canvas>().enabled = true;
        MainCanvas.GetComponent<GraphicRaycaster>().enabled = true;
        MainCanvas.GetComponent<BoxCollider>().enabled = true;
        if (MenusList[SelectedMenu] && MenusList[SelectedMenu].GetComponent<GraphicRaycaster>())
        {
            MenusList[SelectedMenu].GetComponent<GraphicRaycaster>().enabled = true;
        }
        if (useAudioFeedback) AudioFeedbackSource.PlayOneShot(AudioclipMenuOpen);
    }

    public void _DespawnMenu()
    {
        if (!isValidRefs) return;
        state = false;
        MainCanvas.GetComponent<Canvas>().enabled = false;
        MainCanvas.GetComponent<GraphicRaycaster>().enabled = false;
        MainCanvas.GetComponent<BoxCollider>().enabled = false;
        if (MenusList[SelectedMenu] && MenusList[SelectedMenu].GetComponent<GraphicRaycaster>())
        {
            MenusList[SelectedMenu].GetComponent<GraphicRaycaster>().enabled = false;
        }
        if (useAudioFeedback) AudioFeedbackSource.PlayOneShot(AudioclipMenuClose);
    }

    public void _ChangeMenuTo(int menuSelection)
    {
        SelectedMenu = menuSelection;
        if (!isValidRefs || menuSelection >= maxMenuNum) return;
        if (MenusList.Length != 0)
        {
            foreach (GameObject menu in MenusList)
            {
                if (menu && menu.GetComponent<Canvas>())
                {
                    menu.GetComponent<GraphicRaycaster>().enabled = false;
                    menu.GetComponent<Canvas>().enabled = false;
                    menu.GetComponent<BoxCollider>().enabled = false;
                }
            }
            if (MenusList[menuSelection] && MenusList[menuSelection].GetComponent<Canvas>())
            {
                MenusList[menuSelection].GetComponent<Canvas>().enabled = true;
                MenusList[menuSelection].GetComponent<GraphicRaycaster>().enabled = true;
                MenusList[menuSelection].GetComponent<BoxCollider>().enabled = true;
            }
            UI_ActiveIndicator.transform.localPosition = new Vector3(defaultIndicatorPos.x - (IndicatorOffset.x * menuSelection), defaultIndicatorPos.y - (IndicatorOffset.y * menuSelection), defaultIndicatorPos.z - (IndicatorOffset.z * menuSelection));
            if (useAudioFeedback) AudioFeedbackSource.PlayOneShot(AudioclipMenuChange);
        }
        else
        {
            _sendDebugError("No Menus Found", 166);
        }
    }

    private void _sendDebugError(string errorReported, int lineErrored) => Debug.LogError("<color=white>#"+lineErrored+" | Reava_UwUtils:<color=red> <b>" + errorReported + "</b></color>, please review <color=orange>References <color=white>/</color> Settings</color> on: " + gameObject.name + ".</color>", gameObject);
    private void _disableSelf() { this.gameObject.SetActive(false); } //Shuts the entire system off to prevent things from running for nothing
}
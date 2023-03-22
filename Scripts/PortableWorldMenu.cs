using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using UnityEngine.UI;
using VRC.Udon.Common;

/*
 ///////////////////////// //////////////////////////////////////////////////// /////////////////////////
  THIS SCRIPT IS NOT MEANT TO BE EDITED UNLESS YOU KNOW EXACTLY WHAT YOU ARE DOING, PLEASE REFER TO DOCS

                       Script by Reava_, helped by wonderful peeps @ Prefabs. 
     Extra Thanks to Nestor for the general Udon help & Superbstingray for the Avatar Utility Script
 ///////////////////////// //////////////////////////////////////////////////// /////////////////////////
*/

namespace UwUtils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PortableWorldMenu : UdonSharpBehaviour
    {
        [Space]
        [Header("General Settings")]
        [Tooltip("Resets to the default tab of the menu when the menu closes")]
        public bool resetTabOnExit = false;
        [Range(0f, 5f)]
        [Tooltip("How long to hold in VR to open the menu")]
        [SerializeField] private float holdTimeSeconds = 1.5f;
        [Range(0, 4)]
        [SerializeField] private int defaultMenuTab = 0;
        [Tooltip("Keybind for desktop users")]
        [SerializeField] private KeyCode KeybindDesktop = KeyCode.E;
        [Space]
        [Tooltip("Max # of menus is currently 5.")]
        [SerializeField] private GameObject[] MenusList = new GameObject[5];
        [Space]
        [Header("Audio stuff")]
        [SerializeField] private bool useAudioFeedback = true;
        [Range(0f, 0.5f)]
        [SerializeField] private float AudioFeedbackVolume = 0.2f;
        [SerializeField] private AudioSource AudioFeedbackSource;
        [SerializeField] private AudioClip AudioclipMenuOpen;
        [SerializeField] private AudioClip AudioclipMenuClose;
        [SerializeField] private AudioClip AudioclipMenuChange;
        [Space]
        [Header("References")]
        [SerializeField] private Image ProgressIndicator;
        [SerializeField] private GameObject popupIndicator;
        [SerializeField] private Transform HandPosition;
        [SerializeField] private Transform HeadPosition;
        [SerializeField] private Transform DesktopTargetPosition;
        [SerializeField] private GameObject UI_ActiveIndicator;
        [SerializeField] private GameObject MainCanvas;
        [Space]
        [Header("Advanced settings, only edit if you know what you're doing!")]
        [Tooltip("This is the default position for the selector when menu 0 is selected")]
        [SerializeField] private Vector3 defaultIndicatorPos = new Vector3(0f, 13.75f, 0f); //This is the default position for the selector when menu 0 is selected
        [Tooltip("Indicator offset substracted to the default position, multiplied by menu selection.")]
        [SerializeField] private Vector3 IndicatorOffset = new Vector3(0f, 7f, 0f);
        [Tooltip("Canvas Offset added to the transform of the hand target.")]
        [SerializeField] public Vector3 CanvasOffset = new Vector3(0f, 0f, 0f);
        [Tooltip("Default Scale of the UI. (Warning: scales to the scale detected of the avatar)")]
        [Range(0.1f, 2.5f)]
        [SerializeField] private float SystemScale = 1f;
        [SerializeField] private int maxMenuNum = 4;
        [Space]
        [SerializeField] private bool enableLogging = true;
        private bool isValidRefs = true;
        private bool state = true;
        private float currentHeld;
        private int SelectedMenu = 0;
        private GameObject ListedMenuCanvas;
        private float detectedScale = 1f;
        VRCPlayerApi playerApi;
        public Slider[] canvasinputs = new Slider[3];

        void Start()
        {
            canvasinputs[0].value = CanvasOffset.x;
            canvasinputs[1].value = CanvasOffset.y;
            canvasinputs[2].value = CanvasOffset.z;
            if (!ProgressIndicator || !popupIndicator || !HandPosition || !HeadPosition || !UI_ActiveIndicator || !MainCanvas) { _sendDebugError("Missing Main Reference(s)"); isValidRefs = false; }
            if (useAudioFeedback) if (!AudioclipMenuOpen || !AudioclipMenuClose || !AudioclipMenuChange || !AudioFeedbackSource) _sendDebugWarning("Missing Audio Clip/Source");
            int itemp = 0;
            maxMenuNum += 1;
            foreach (GameObject o in MenusList)
            {
                if (o)
                {
                    if (!o.GetComponent<Canvas>() || !o.GetComponent<BoxCollider>() || !o.GetComponent<GraphicRaycaster>())
                    {
                        _sendDebugError("Invalid Menu #" + itemp + " <color=white>('" + o.gameObject.name + "'),</color> it will be ignored");
                    }
                }
                itemp += 1;
            }
            if (!MainCanvas.GetComponent<Canvas>() || !MainCanvas.GetComponent<BoxCollider>() || !MainCanvas.GetComponent<GraphicRaycaster>())
            {
                _sendDebugError("Missing component on Main Canvas");
                isValidRefs = false;
            }
            if (!isValidRefs)
            {
                MainCanvas.SetActive(false);
                _sendDebugError("Invalid setup, disabling Menu now..");
                _disableSelf();
            }
            else
            {
                if (state) _DespawnMenu(false);
                popupIndicator.SetActive(false);
            }
            if (AudioFeedbackSource) AudioFeedbackSource.volume = AudioFeedbackVolume;
            playerApi = Networking.LocalPlayer;
            // Disable menu and switch to default tab to avoid it being visible or have multiple tabs open at the same time, allows editing tabs without disabling their components in Editor.
            _ChangeMenuTo(defaultMenuTab, false);
            _DespawnMenu(false);
        }

        public void OnPlayerRespawn(VRCPlayerApi player)
        {
            if (player.isLocal)
            {
                if (isValidRefs && state)
                {
                    _DespawnMenu(false);
                    popupIndicator.SetActive(false);
                }
            }
        }

        public void _scaleChange(float scale)
        {
            if (enableLogging) _sendDebugLog("Scale change detected:" + scale);
            detectedScale = scale;
            CanvasOffset = CanvasOffset * scale;
            popupIndicator.transform.localScale = popupIndicator.transform.localScale * (scale * SystemScale);
            MainCanvas.transform.localScale = MainCanvas.transform.localScale * (scale * SystemScale);
            DesktopTargetPosition.localPosition = new Vector3(0f, 0f, 0.14f * scale);
        }

        public void Update()
        {
            CanvasOffset = new Vector3(canvasinputs[0].value, canvasinputs[1].value, canvasinputs[2].value);
            if (!isValidRefs) return;
            if (Input.GetKeyDown(KeybindDesktop))
            {
                if (!state)
                {
                    _spawnMenu(false, true);
                    if (enableLogging) _sendDebugLog("Spawn from desktop keybind (" + KeybindDesktop + ")");
                }
                else
                {
                    _DespawnMenu(true);
                    if (enableLogging) _sendDebugLog("Despawn from desktop keybind (" + KeybindDesktop + ")");
                }
            }
            if (Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryThumbstickVertical") < -0.95f || Input.GetKey(KeyCode.Keypad3))
            {
                HeadPosition.SetPositionAndRotation(playerApi.GetBonePosition(HumanBodyBones.Head), playerApi.GetBoneRotation(HumanBodyBones.Head));
                HandPosition.SetPositionAndRotation(playerApi.GetBonePosition(HumanBodyBones.RightHand), playerApi.GetBoneRotation(HumanBodyBones.RightHand));
                popupIndicator.transform.localPosition = HandPosition.transform.position + CanvasOffset;
                popupIndicator.transform.localRotation = HeadPosition.transform.rotation;
                if (currentHeld == 0 && !state)
                {
                    if (enableLogging) _sendDebugLog("Popup indicator spawn");
                    popupIndicator.SetActive(true);
                    
                }
                currentHeld += Time.deltaTime;
                if (currentHeld > holdTimeSeconds)
                {
                    if (enableLogging) _sendDebugLog("VR Keybind held for " + currentHeld);
                    if (!state) _spawnMenu(true, true);
                    currentHeld = 0f;
                }
                ProgressIndicator.fillAmount = currentHeld / holdTimeSeconds;
            }
            else
            {
                if (currentHeld > 0)
                {
                    if (enableLogging) _sendDebugLog("VR Keybind released");
                    currentHeld = 0f;
                    ProgressIndicator.fillAmount = 0f;
                    popupIndicator.SetActive(false);
                }
            }
            if (Input.GetButton("Oculus_CrossPlatform_Button4") || Input.GetButton("Oculus_CrossPlatform_Button2") || Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryThumbstickHorizontal") != 0 || Input.GetButton("Horizontal") || Input.GetButton("Vertical") || Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryThumbstickVertical") > 0.95f)
            {
                if (state) _DespawnMenu(true);
            }
        }

        public void _changeVolume(float newVolume)
        {
            AudioFeedbackSource.volume = AudioFeedbackVolume = newVolume;
            if (enableLogging) _sendDebugLog("Volume changed to: " + newVolume);
        }

        public override void InputMoveHorizontal(float value, UdonInputEventArgs args)
        {
            if (state) _DespawnMenu(true);
        }

        public override void InputMoveVertical(float value, UdonInputEventArgs args)
        {
            if (state) _DespawnMenu(true);
        }

        public void _ToggleSound()
        {
            if (enableLogging) _sendDebugLog("Sound feedback toggle event (" + !useAudioFeedback + ")");
            useAudioFeedback = !useAudioFeedback;
        }

        public void _spawnMenu(bool isVR, bool useSound)
        {
            _sendDebugLog("Canvas offsets are: x " + CanvasOffset.x + " y " + CanvasOffset.y + " z " + CanvasOffset.z + ""); // remove later
            if (enableLogging) _sendDebugLog("Spawn menu event");
            if (!isValidRefs) return;
            state = true;
            HeadPosition.transform.SetPositionAndRotation(playerApi.GetBonePosition(HumanBodyBones.Head), playerApi.GetBoneRotation(HumanBodyBones.Head));
            if (resetTabOnExit && defaultMenuTab <= 4) _ChangeMenuTo(defaultMenuTab, true);
            if (isVR)
            {
                HandPosition.transform.SetPositionAndRotation(playerApi.GetBonePosition(HumanBodyBones.RightHand), playerApi.GetBoneRotation(HumanBodyBones.RightHand));
                //MainCanvas.transform.SetPositionAndRotation((HandPosition.transform.position + CanvasOffset), HeadPosition.transform.rotation);
                MainCanvas.transform.localPosition = HandPosition.transform.position + CanvasOffset;
                MainCanvas.transform.localRotation = HeadPosition.transform.rotation;
            }
            else
            {
                DesktopTargetPosition.position = HeadPosition.transform.position;
                DesktopTargetPosition.localPosition = new Vector3(0f, 0.04f * detectedScale, 0.35f * detectedScale);
                MainCanvas.transform.SetPositionAndRotation(DesktopTargetPosition.transform.position, HeadPosition.transform.rotation);
            }
            popupIndicator.SetActive(false);
            MainCanvas.GetComponent<Canvas>().enabled = true;
            MainCanvas.GetComponent<GraphicRaycaster>().enabled = true;
            MainCanvas.GetComponent<BoxCollider>().enabled = true;
            if (MenusList[SelectedMenu] && MenusList[SelectedMenu].GetComponent<Canvas>())
            {
                MenusList[SelectedMenu].GetComponent<Canvas>().enabled = true;
                MenusList[SelectedMenu].GetComponent<GraphicRaycaster>().enabled = true;
                MenusList[SelectedMenu].GetComponent<BoxCollider>().enabled = true;
            }
            if (useAudioFeedback && useSound) AudioFeedbackSource.PlayOneShot(AudioclipMenuOpen);
        }

        public void _DespawnMenu(bool useSound)
        {
            if (enableLogging) _sendDebugLog("Despawn menu event");
            if (!isValidRefs) return;
            if (useAudioFeedback && MainCanvas.GetComponent<Canvas>().enabled && useSound) AudioFeedbackSource.PlayOneShot(AudioclipMenuClose);
            MainCanvas.GetComponent<Canvas>().enabled = false;
            MainCanvas.GetComponent<GraphicRaycaster>().enabled = false;
            MainCanvas.GetComponent<BoxCollider>().enabled = false;
            if (MenusList[SelectedMenu] && MenusList[SelectedMenu].GetComponent<Canvas>())
            {
                MenusList[SelectedMenu].GetComponent<Canvas>().enabled = false;
                MenusList[SelectedMenu].GetComponent<GraphicRaycaster>().enabled = false;
                MenusList[SelectedMenu].GetComponent<BoxCollider>().enabled = false;
            }
            state = false;
        }

        public void _ChangeMenuTo(int menuSelection, bool overrideAudio)
        {
            if (enableLogging) _sendDebugLog("Menu change event to " + menuSelection);
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
                if (useAudioFeedback && overrideAudio) AudioFeedbackSource.PlayOneShot(AudioclipMenuChange);
            }
            else
            {
                _sendDebugError("No Menus Found");
            }
        }

        private void _sendDebugLog(string log) => Debug.Log("<b> [Reava_/UwUtils/PortableWorldMenu.cs]: " + log + " on: " + gameObject.name + ".</b> ", gameObject);
        private void _sendDebugWarning(string errorReported) => Debug.LogWarning("<color=white> [Reava_/UwUtils/PortableWorldMenu.cs]:<color=orange> <b>" + errorReported + "</b></color>, this will be ignored when functioning. <color=orange>Check References <color=white>/</color> Settings</color> on: " + gameObject.name + ".</color>", gameObject);
        private void _sendDebugError(string errorReported) => Debug.LogError("<color=white> [Reava_/UwUtils/PortableWorldMenu.cs]:<color=red> <b>" + errorReported + "</b></color>, please review <color=orange>References <color=white>/</color> Settings</color> on: " + gameObject.name + ".</color>", gameObject);
        private void _disableSelf() { this.gameObject.SetActive(false); } //Shuts the entire system off to prevent things from running for nothing
    }
}
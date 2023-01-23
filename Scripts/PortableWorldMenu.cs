using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using UnityEngine.UI;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class PortableWorldMenu : UdonSharpBehaviour
{
    [Space]
    [Header("Settings")]
    [Tooltip("Resets to the default tab of the menu when the menu closes")]
    [SerializeField] private bool resetTabOnExit = false;
    [SerializeField] private int defaultMenuTab = 0;
    [Range(0f, 5f)]
    [SerializeField] private float holdTimeSeconds = 1.5f;
    [Space]
    [Header("References")]
    [SerializeField] private Image ProgressIndicator;
    [SerializeField] private GameObject popupIndicator;
    [SerializeField] private GameObject CanvasTargetPosition;
    [SerializeField] private GameObject CanvasTargetRotation;
    [SerializeField] private GameObject UIContainer;
    [SerializeField] private GameObject UI_ActiveIndicator;
    [SerializeField] private GameObject[] MenusList;
    private float defaultIndicatorPos = 13.75f;
    private GameObject SelectedMenuCanvas;
    private GameObject ListedMenuCanvas;
    private GameObject Canvas;
    private bool state = false;
    private float currentHeld;
    private float detectedScale = 1f;

    void Start()
    {
        if (!ProgressIndicator || !popupIndicator || !CanvasTargetPosition || !UIContainer || !CanvasTargetRotation) _sendDebugError();
        UIContainer.SetActive(false);
        popupIndicator.SetActive(false);
        Canvas = UIContainer.transform.GetChild(0).gameObject;
    }

    public void OnPlayerRespawn(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            _DespawnMenu();
            popupIndicator.SetActive(false);
        }
    }

    public void _scaleChange(float scale)
    {
        detectedScale = scale;
        //do smth
    }

    public void Update()
    {
        if (Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryThumbstickVertical") < -0.95f || Input.GetKey(KeyCode.Keypad3))
        {
            if (currentHeld == 0)
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
        if (Input.GetKeyDown(KeyCode.E))
        {
            if(!state) _spawnMenu(); else _DespawnMenu();
        }
        if (Input.GetButton("Oculus_CrossPlatform_Button4") || Input.GetButton("Oculus_CrossPlatform_Button2") || Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryThumbstickHorizontal") != 0 || Input.GetButton("Horizontal") || Input.GetButton("Vertical") || Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryThumbstickVertical") > 0.95f)
        {
            _DespawnMenu();
        }
    }

    public void _spawnMenu()
    {
        state = true;
        if (resetTabOnExit) _ChangeMenuTo(defaultMenuTab);
        UIContainer.transform.SetPositionAndRotation(CanvasTargetPosition.transform.position, CanvasTargetRotation.transform.rotation);
        popupIndicator.SetActive(false);
        Canvas.GetComponent<Canvas>().enabled = true;
        Canvas.GetComponent<GraphicRaycaster>().enabled = true;
        Canvas.GetComponent<BoxCollider>().enabled = true;
    }

    public void _DespawnMenu()
    {
        state = false;
        Canvas.GetComponent<Canvas>().enabled = false;
        Canvas.GetComponent<GraphicRaycaster>().enabled = false;
        Canvas.GetComponent<BoxCollider>().enabled = false;
    }

    public void _ChangeMenuTo(int menuSelection)
    {
        if (MenusList.Length != 0)
        {
            foreach (GameObject menu in MenusList)
            {
                //menu.SetActive(false);
                menu.GetComponent<Canvas>().enabled = false;
                menu.GetComponent<GraphicRaycaster>().enabled = false;
                menu.GetComponent<BoxCollider>().enabled = false;
            }
            //MenusList[menuSelection].SetActive(true);
            MenusList[menuSelection].GetComponent<Canvas>().enabled = true;
            MenusList[menuSelection].GetComponent<GraphicRaycaster>().enabled = true;
            MenusList[menuSelection].GetComponent<BoxCollider>().enabled = true;
            UI_ActiveIndicator.transform.localPosition = new Vector3(-3.5f, defaultIndicatorPos - (7 * menuSelection), 0f);
        }
        else
        {
            _sendDebugError();
        }
    }

    private void _sendDebugError() => Debug.LogError("Reava_UwUtils:<color=red> <b>Invalid references</b></color>, please review <color=orange>references</color> on: " + gameObject + ".", gameObject);
}
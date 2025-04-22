using UdonSharp;
using UnityEngine;
using UwUtils;
using VRC.SDKBase;

namespace UwUtils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PortableMenuSelector : UdonSharpBehaviour
    {
        [SerializeField] private PortableWorldMenu portableMenuSystem;
        [Space]
        [SerializeField] private bool isMenuSelector = true;
        [SerializeField] private int menu = 0;
        [SerializeField] private bool allowAudioFeedback = true;
        [Space]
        [Header("Send an additional event?")]
        [SerializeField] private bool sendOptionalEvent = false;
        [SerializeField] private string optionalEventName = "";
        [Range(0f, 10f)]
        [SerializeField] private float eventDelay = 0.1f;
        [Space]
        [SerializeField] private bool enableLogging = true;
        public override void Interact()
        {
            if (!Utilities.IsValid(portableMenuSystem)) return;
            if (enableLogging) Debug.Log("[Reava/UwUtils/PortableMenuSelector.cs]: Interact triggered, Parameters: isMenuSelector = " + isMenuSelector + " - menu = " + menu + " - sendOptionalEvent = " + sendOptionalEvent + " - optionalEventName = " + optionalEventName + " - eventDelay = " + eventDelay + " on: <b>" + gameObject.name + ".</b>", gameObject);
            if (isMenuSelector) portableMenuSystem._ChangeMenuTo(menu, allowAudioFeedback);
            if (sendOptionalEvent && optionalEventName != null) portableMenuSystem.SendCustomEventDelayedSeconds(optionalEventName, eventDelay);
        }
    }
}
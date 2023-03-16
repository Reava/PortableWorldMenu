using UdonSharp;
using UnityEngine;
using UwUtils;

namespace UwUtils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PortableMenuSelector : UdonSharpBehaviour
    {
        [SerializeField] private PortableWorldMenu portableMenuSystem;
        [Space]
        [SerializeField] private bool isMenuSelector = true;
        [Range(0, 4)]
        [SerializeField] private int menu = 0;
        [Header("Send an additional event?")]
        [SerializeField] private bool sendOptionalEvent = false;
        [SerializeField] private string optionalEventName = "";
        [Range(0f, 10f)]
        [SerializeField] private float eventDelay = 0.1f;
        [Space]
        [SerializeField] private bool enableLogging = true;
        public override void Interact()
        {
            if (!portableMenuSystem) return;
            if (enableLogging) Debug.Log("<color=white> | Reava_UwUtils: Interact triggered, Parameters: isMenuSelector = " + isMenuSelector + " - menu = " + menu + " - sendOptionalEvent = " + sendOptionalEvent + " - optionalEventName = " + optionalEventName + " - eventDelay = " + eventDelay + "</color> on: <b>" + gameObject.name + ".</b>", gameObject);
            if (isMenuSelector) portableMenuSystem._ChangeMenuTo(menu);
            if (sendOptionalEvent && optionalEventName != null) portableMenuSystem.SendCustomEventDelayedSeconds(optionalEventName, eventDelay);
        }
    }
}
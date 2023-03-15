using UdonSharp;
using UnityEngine;

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
    public override void Interact()
    {
        if (isMenuSelector) portableMenuSystem._ChangeMenuTo(menu);
        if (sendOptionalEvent && optionalEventName != null) SendCustomEventDelayedSeconds(nameof(optionalEventName), eventDelay);
    }
}
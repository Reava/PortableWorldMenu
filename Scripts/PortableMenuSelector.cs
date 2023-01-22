using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class PortableMenuSelector : UdonSharpBehaviour
{
    [SerializeField] private PortableWorldMenu portableMenuSystem;
    [SerializeField] private int menu = 0;
    public override void Interact()
    {
        portableMenuSystem._ChangeMenuTo(menu);
    }
}

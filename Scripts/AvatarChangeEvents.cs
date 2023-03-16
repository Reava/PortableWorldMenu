using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UwUtils;

namespace Superbstingray
{
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class AvatarChangeEvents : UdonSharp.UdonSharpBehaviour
	{
		private float CameraScale;
		[SerializeField] private PortableWorldMenu PortableWorldMenuSystem;

		public void OnTriggerEnter(Collider onTriggerEnterOther)
		{
			if (onTriggerEnterOther.name == "ColliderEnter")
			{
				_AvatarChangeEvent();
			}
		}

		public void OnTriggerExit(Collider onTriggerExitOther)
		{
			if (onTriggerExitOther.name == "ColliderExit")
			{
				_AvatarChangeEvent();
			}
		}

		public void _AvatarChangeEvent()
		{
			CameraScale = (1F / transform.localScale.y);
			transform.parent.GetChild(1).localPosition = new Vector3(0, transform.localScale.y / 2F, 0);
			PortableWorldMenuSystem._scaleChange(CameraScale);
		}

		public void OnPlayerRespawn(VRCPlayerApi player)
		{
			if (player.isLocal)
			{
				_AvatarChangeEvent();
			}
		}
	}
}
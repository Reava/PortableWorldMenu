using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Superbstingray
{
	[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
	public class AvatarChangeEvents : UdonSharp.UdonSharpBehaviour
	{
		private float CameraScale;

		public void OnTriggerEnter(Collider onTriggerEnterOther)
		{
			if (onTriggerEnterOther.name == "ColliderEnter")
			{
				SendCustomEvent("_AvatarChangeEvent");
				AvatarSmallerEvent();
			}
		}

		public void OnTriggerExit(Collider onTriggerExitOther)
		{
			if (onTriggerExitOther.name == "ColliderExit")
			{
				SendCustomEvent("_AvatarChangeEvent");
				AvatarLargerEvent();
			}
		}

		// Reposition trigger colliders for the new box collider scale
		public void _AvatarChangeEvent()
		{
			CameraScale = (1F / transform.localScale.y);
			transform.parent.GetChild(1).localPosition = new Vector3(0, transform.localScale.y / 2F, 0);
		}

		// Run custom event when avatar becomes smaller
		public void AvatarSmallerEvent()
		{
			Debug.Log(string.Format("Smaller Avatar {0}", (CameraScale)));
		}

		// Run custom event when avatar becomes larger
		public void AvatarLargerEvent()
		{
			Debug.Log(string.Format("Larger Avatar {0}", (CameraScale)));
		}
	}
}
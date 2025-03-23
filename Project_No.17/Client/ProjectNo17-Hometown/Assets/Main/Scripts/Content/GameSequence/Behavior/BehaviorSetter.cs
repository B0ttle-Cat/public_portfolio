using Sirenix.OdinInspector;

using Unity.Behavior;

using UnityEngine;

namespace TFContent
{
	public abstract class BehaviorSetter : MonoBehaviour
	{
		[SerializeField, InlineButton("Setup")]
		protected BehaviorGraphAgent agent;

		public abstract void Setup();
	}
}

using System;

using Sirenix.OdinInspector;

using UnityEngine;

namespace TFContent
{
	[Serializable]
	public abstract class ScenarioObjectData : ScriptableObject
	{
		[ShowInInspector, HideLabel, DisplayAsString, PropertyOrder(-9999), PropertySpace(SpaceAfter = 10)]
		[InlineButton("ThisScript"), InlineButton("OnValidate"), EnableGUI]
		private string ThisEpisodeObjectType => $"TypeName: {GetType().Name}";

#if UNITY_EDITOR
		private void ThisScript()
		{
			BC.Base.PingAndOpenScript.PingScript(GetType(), true);
		}
#endif

		public virtual void OnValidate()
		{

		}
	}
}

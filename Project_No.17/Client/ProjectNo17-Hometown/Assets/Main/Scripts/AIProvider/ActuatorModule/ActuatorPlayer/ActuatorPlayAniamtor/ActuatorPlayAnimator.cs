using System;
using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector;

using UnityEditor;


#if UNITY_EDITOR
using UnityEditor.Animations;

#endif
using UnityEngine;

using Object = UnityEngine.Object;


namespace BC.Actuator
{
	public class ActuatorPlayAnimator : ScriptableObject
	{
		[TitleGroup("PlayAnimator")]
		[TabGroup("PlayAnimator/A", "Animator")]
		[InlineButton("CreateAnimator","Create", ShowIf ="@playAnimatorController==null")]
		[InlineButton("RemoveAnimator","Remove", ShowIf ="@playAnimatorController!=null")]
		[LabelText("Play Animator")]
		public RuntimeAnimatorController playAnimatorController;
		[HorizontalGroup("PlayAnimator/A/Animator/B"), LabelText("Enter"), SuffixLabel("s ", overlay: true), MinValue(0)]
		public float enterBlandTime = 0.25f;
		[HorizontalGroup("PlayAnimator/A/Animator/B"), LabelText(" Exit"), SuffixLabel("s ", overlay: true), MinValue(0)]
		public float exitBlandTime = 0.25f;
#if UNITY_EDITOR
		[TabGroup("PlayAnimator/A", "InitParameters"), ShowInInspector]
		[InlineButton("CopyParameters", "Copy", ShowIf = "@playAnimatorController!=null")]
		private AnimatorController copyParametersTarget { get; set; }
#endif
		[TabGroup("PlayAnimator/A", "Parameters")]
		public InitParameters[] initParameters = new InitParameters[0];
		[Serializable]
		public struct InitParameters : IAnimatorParameter
		{
			[HideLabel, HorizontalGroup(width: 0.4f)]
			[LabelText("Name"), LabelWidth(35)]
			[ValueDropdown("GetAnimatorParameterNames", IsUniqueList = true)]
			[OnValueChanged("GetAnimatorParameterType")]
			public string name;
			[HideLabel, HorizontalGroup, ReadOnly]
			public AnimatorControllerParameterType type;
			private bool ShowInt => type == AnimatorControllerParameterType.Int;
			private bool ShowFloat => type == AnimatorControllerParameterType.Float;
			private bool ShowBool => type == AnimatorControllerParameterType.Bool || type == AnimatorControllerParameterType.Trigger;
			[LabelText("Value"), LabelWidth(35), HorizontalGroup(width: 0.4f), ShowIf("ShowInt")]
			public float floatValue;
			[LabelText("Value"), LabelWidth(35), HorizontalGroup(width: 0.4f), ShowIf("ShowFloat")]
			public int intValue;
			[LabelText("Value"), LabelWidth(35), HorizontalGroup(width: 0.4f), ShowIf("ShowBool")]
			public bool boolValue;

			string IAnimatorParameter.Name => name;
			AnimatorControllerParameterType IAnimatorParameter.Type => type;
			float IAnimatorParameter.FloatValue => floatValue;
			int IAnimatorParameter.IntValue => intValue;
			bool IAnimatorParameter.BoolValue => boolValue;

#if UNITY_EDITOR
			private ValueDropdownList<string> GetAnimatorParameterNames()
			{

				ValueDropdownList<string> list = new ValueDropdownList<string>();

				RuntimeAnimatorController playAnimatorController = null;

				if(Selection.activeObject == null)
				{
					return list;
				}
				else if(Selection.activeObject is ActuatorPlayAnimator actuatorBasic)
				{
					playAnimatorController = actuatorBasic.playAnimatorController;
				}
				else if(Selection.activeObject is ActuatorModule actuatorPlayerModule)
				{
					playAnimatorController = actuatorPlayerModule.Animator.playAnimatorController;
				}
				else
				{
					return list;
				}
				if(playAnimatorController != null && playAnimatorController is AnimatorController ac)
				{
					foreach(var parameter in ac.parameters)
					{
						list.Add(parameter.name);
					}
				}
				return list;
			}

			private void GetAnimatorParameterType(string name)
			{
				RuntimeAnimatorController playAnimatorController = null;

				if(Selection.activeObject == null)
				{
					return;
				}
				else if(Selection.activeObject is ActuatorPlayAnimator actuatorBasic)
				{
					playAnimatorController = actuatorBasic.playAnimatorController;
				}
				else if(Selection.activeObject is ActuatorModule actuatorPlayerModule)
				{
					playAnimatorController = actuatorPlayerModule.Animator.playAnimatorController;
				}
				else
				{
					return;
				}
				if(playAnimatorController is AnimatorController ac)
				{
					var param = ac.parameters.FirstOrDefault(p => p.name == name);
					type =  param?.type ?? type;
				}
			}
#endif
		}

		[TabGroup("PlayAnimator/A", "OrderParameters"), ShowInInspector]
		public OrderParameters[] orderEnterParameters = new OrderParameters[0];
		[TabGroup("PlayAnimator/A", "OrderParameters"), ShowInInspector]
		public OrderParameters[] orderExitParameters = new OrderParameters[0];
		[Serializable]
		public struct OrderParameters : IAnimatorParameter
		{
			[HideLabel, HorizontalGroup(width: 0.4f)]
			[LabelText("Name"), LabelWidth(35)]
			public string name;
			[HideLabel, HorizontalGroup]
			public AnimatorControllerParameterType type;
			private bool ShowInt => type == AnimatorControllerParameterType.Int;
			private bool ShowFloat => type == AnimatorControllerParameterType.Float;
			private bool ShowBool => type == AnimatorControllerParameterType.Bool || type == AnimatorControllerParameterType.Trigger;
			[LabelText("Value"), LabelWidth(35), HorizontalGroup(width: 0.4f), ShowIf("ShowInt")]
			public float floatValue;
			[LabelText("Value"), LabelWidth(35), HorizontalGroup(width: 0.4f), ShowIf("ShowFloat")]
			public int intValue;
			[LabelText("Value"), LabelWidth(35), HorizontalGroup(width: 0.4f), ShowIf("ShowBool")]
			public bool boolValue;

			string IAnimatorParameter.Name => name;
			AnimatorControllerParameterType IAnimatorParameter.Type => type;
			float IAnimatorParameter.FloatValue => floatValue;
			int IAnimatorParameter.IntValue => intValue;
			bool IAnimatorParameter.BoolValue => boolValue;
		}
		public interface IAnimatorParameter
		{
			public string Name { get; }
			public AnimatorControllerParameterType Type { get; }
			public float FloatValue { get; }
			public int IntValue { get; }
			public bool BoolValue { get; }
		}

#if UNITY_EDITOR
		private void CreateAnimator()
		{
			if(playAnimatorController != null) return;

			var selected = Selection.activeObject;
			if(selected == null)
			{
				Debug.LogError("ScriptableObject가 선택되지 않았습니다.");
				return;
			}

			// 상위 에셋 가져오기
			var path = AssetDatabase.GetAssetPath(selected);
			if(string.IsNullOrEmpty(path))
			{
				Debug.LogError("선택된 오브젝트의 경로를 찾을 수 없습니다.");
				return;
			}


			// 선택된 오브젝트가 서브 에셋인지 확인
			Object mainAsset = AssetDatabase.LoadMainAssetAtPath(path);

			if(mainAsset == null)
			{
				Debug.LogError("최상위 에셋을 찾을 수 없습니다.");
				return;
			}

			// 최상위 에셋 경로 기준으로 생성
			// AnimatorController 인스턴스 생성 (파일로 저장하지 않음)
			var controller = new AnimatorController
			{
				name = $"{mainAsset.name}_Animator",
			};

			// ScriptableObject의 서브에셋으로 추가
			AssetDatabase.AddObjectToAsset(controller, mainAsset);
			AssetDatabase.ImportAsset(path); // 반드시 다시 import 해줘야 에셋 연결됨

			playAnimatorController = controller;

			EditorUtility.SetDirty(controller);
			EditorUtility.SetDirty(this);
			EditorUtility.SetDirty(mainAsset);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
		private void RemoveAnimator()
		{
			if(playAnimatorController == null) return;

			var selected = Selection.activeObject;
			if(selected == null)
			{
				Debug.LogError("ScriptableObject가 선택되지 않았습니다.");
				return;
			}

			string path = AssetDatabase.GetAssetPath(selected);
			if(string.IsNullOrEmpty(path)) return;

			Object mainAsset = AssetDatabase.LoadMainAssetAtPath(path);

			// 해당 컨트롤러가 이 ScriptableObject의 서브에셋인지 확인
			var subAssets = AssetDatabase.LoadAllAssetsAtPath(path);
			List<Object> deleteSubAssets = new List<Object>();

			foreach(var asset in subAssets)
			{
				if(asset is AnimatorController animatorController)
				{
					deleteSubAssets.Add(animatorController);
				}
				if(asset is AnimatorStateMachine animatorStateMachine)
				{
					deleteSubAssets.Add(animatorStateMachine);
				}
			}

			foreach(var item in deleteSubAssets)
			{
				Object.DestroyImmediate(item, true);
			}

			playAnimatorController = null;
			EditorUtility.SetDirty(this);
			EditorUtility.SetDirty(mainAsset);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
		private void CopyParameters()
		{
			if(copyParametersTarget == null)
			{
				Debug.LogWarning("복사할 대상 AnimatorController가 비어 있습니다.");
				return;
			}

			if(playAnimatorController is not AnimatorController targetController)
			{
				Debug.LogWarning("할당된 playAnimatorController가 AnimatorController 타입이 아닙니다.");
				return;
			}

			// 중복 방지용 이름 리스트
			var existingNames = new HashSet<string>();
			foreach(var param in targetController.parameters)
				existingNames.Add(param.name);

			// 복사
			foreach(var param in copyParametersTarget.parameters)
			{
				if(existingNames.Contains(param.name))
				{
					Debug.Log($"이미 존재하는 파라미터 건너뜀: {param.name}");
					continue;
				}

				targetController.AddParameter(param);
				Debug.Log($"파라미터 복사됨: {param.name}");
			}

			EditorUtility.SetDirty(targetController);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
#endif
	}

}

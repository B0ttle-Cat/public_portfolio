using System;
using System.Collections.Generic;

using BC.ODCC;

using Sirenix.OdinInspector;

using UniHumanoid;

using UnityEngine;

using UniVRM10;

namespace BC.CMC
{
	public class CharacterModelController : ComponentBehaviour
	{
		[TitleGroup("VrmModel"), HorizontalGroup("VrmModel/H"), SerializeField, ReadOnly]
		private Humanoid humanoid;
		[TitleGroup("VrmModel"), HorizontalGroup("VrmModel/H"), SerializeField, ReadOnly]
		Vrm10Instance vrmInstance;

		[SerializeField,HideLabel, FoldoutGroup("LookTarget")]
		private LookTargetController lookTargetController;
		[SerializeField,HideLabel, FoldoutGroup("FaceExpression")]
		private FaceExpressionController faceExpressionController;

		public LookTargetController LookTarget => lookTargetController;
		public FaceExpressionController FaceExpression => faceExpressionController;


		[Serializable]
		public class LookTargetController
		{
			[TitleGroup("LookAt")]
			[ReadOnly, HorizontalGroup("LookAt/H1"), LabelText("Head"), LabelWidth(50), HideLabel, PropertyOrder(0)]
			public Transform headBone;
			[ReadOnly, HorizontalGroup("LookAt/H1"), LabelText(" Offset"), LabelWidth(50), DisplayAsString, PropertyOrder(0)]
			public Vector3 headOffset;
#if UNITY_EDITOR
			[ShowInInspector, HorizontalGroup("LookAt/H2", width: 60), HideLabel, EnableGUI, DisplayAsString, PropertyOrder(1)]
			private string EyeLabel => "Left/Right";
#endif
			[ReadOnly, HorizontalGroup("LookAt/H2"), HideLabel, PropertyOrder(2)]
			public Transform leftEyeBone;
			[ReadOnly, HorizontalGroup("LookAt/H2"), HideLabel, PropertyOrder(2)]
			public Transform rightEyeBone;
			[HorizontalGroup("LookAt/H2", width:0.4f), LabelText(" LookAt"), LabelWidth(50), PropertyOrder(2)]
			public Transform lookAtTarget;

			[TitleGroup("ClampAngle")]
			[HorizontalGroup("ClampAngle/Angle1")]
			public float outerAngle;
			[HorizontalGroup("ClampAngle/Angle1")]
			public float innerAngle;
			[HorizontalGroup("ClampAngle/Angle2")]
			public float downAngle;
			[HorizontalGroup("ClampAngle/Angle2")]
			public float upAngle;


			public void LootTargetUpdate()
			{

			}
		}

		[Serializable]
		public class FaceExpressionController
		{
			[ReadOnly, HideLabel]
			public SkinnedMeshRenderer faceMesh;
			[ValueDropdown("ExpressionNameList")]
			public string defaultClipName;
			[ValueDropdown("ExpressionNameList")]
			public string blinkClipName;
			[ReadOnly]
			public FaceBlendShapes[] blendShapesList;
			public ExpressionItem[] customExpressionList;
			[ReadOnly]
			public ExpressionItem[] expressionList;

			private ValueDropdownList<string> ExpressionNameList()
			{
				ValueDropdownList<string> list = new ValueDropdownList<string>();
				if(customExpressionList != null)
				{
					for(int i = 0 ; i < customExpressionList.Length ; i++)
					{
						if(customExpressionList[i].clip != null)
							list.Add(customExpressionList[i].clip.name);
					}
				}
				if(expressionList != null)
				{
					for(int i = 0 ; i < expressionList.Length ; i++)
					{
						if(expressionList[i].clip != null)
							list.Add(expressionList[i].clip.name);
					}
				}
				return list;
			}
		}
		[Serializable]
		public struct FaceBlendShapes
		{
			[HorizontalGroup(width:0.05f), HideLabel]
			public int index;
			[HorizontalGroup, HideLabel]
			public string name;
			[HorizontalGroup(width:0.3f), HideLabel, Range(0f,100f)]
			public float weight;
		}
		[Serializable]
		public struct ExpressionItem
		{
			[InlineEditor(DrawPreview = true, PreviewAlignment = PreviewAlignment.Left, PreviewWidth = 150)]
			public VRM10Expression clip;
		}

		protected override void BaseValidate(in bool isPrefab = false)
		{
			base.BaseValidate(isPrefab);
			if(isPrefab) return;
			InitVRMCharacter();
		}

		public void InitVRMCharacter()
		{
			Humanoid humanoid = GetComponent<Humanoid>();
			if(humanoid != null)
			{
				this.humanoid = humanoid;
			}
			else
			{
				humanoid = GetComponentInParent<Humanoid>();
				if(humanoid != null)
				{
					this.humanoid = humanoid;
				}
				else
				{
					this.humanoid = null;
				}
			}

			Vrm10Instance vrmInstance = GetComponent<Vrm10Instance>();
			if(vrmInstance != null)
			{
				this.vrmInstance = vrmInstance;
			}
			else
			{
				vrmInstance = GetComponentInParent<Vrm10Instance>();
				if(vrmInstance != null)
				{
					this.vrmInstance = vrmInstance;
				}
				else
				{
					this.vrmInstance = null;
				}
			}
		}

		public void SetLookTarget(Transform headBone, Vector3 headOffset, float outer, float inner, float down, float up, Transform leftEyeBone, Transform rightEyeBone, Transform lookAtTarget = null)
		{
			lookTargetController ??= new LookTargetController();
			lookTargetController.headBone = headBone;
			lookTargetController.headOffset = headOffset;
			lookTargetController.leftEyeBone = leftEyeBone;
			lookTargetController.rightEyeBone = rightEyeBone;
			lookTargetController.lookAtTarget = lookAtTarget;

			lookTargetController.outerAngle = outer;
			lookTargetController.innerAngle = inner;
			lookTargetController.downAngle = down;
			lookTargetController.upAngle = up;
		}

		public void SetExpressionList(SkinnedMeshRenderer faceMesh, List<FaceBlendShapes> blendShapesList, List<ExpressionItem> expressionList)
		{
			faceExpressionController??= new FaceExpressionController();
			faceExpressionController.faceMesh = faceMesh;
			faceExpressionController.blendShapesList = blendShapesList.ToArray();
			faceExpressionController.expressionList = expressionList.ToArray();
		}
	}
}

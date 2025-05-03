using Sirenix.OdinInspector;

#if UNITY_EDITOR
#endif

using UnityEngine;

namespace BC.Actuator
{
	[CreateAssetMenu(fileName = "ActuatorBasic", menuName = "Scriptable Objects/ActuatorBasic")]
	public class ActuatorBasic : ScriptableObject
	{
		[TitleGroup("Title", subtitle:"활성화 상태 / 고유 이름", alignment: TitleAlignments.Split)]
		[HorizontalGroup("Title/T", width:100), ToggleLeft]
		public bool isActive = true;
		[HorizontalGroup("Title/T"), LabelWidth(40), LabelText("Name")]
		public string ActuatorName;

		[TitleGroup("Cost", subtitle:"각 요소별 필요한 비용", alignment: TitleAlignments.Split)]
		[HorizontalGroup("Cost/A"), HideLabel, SuffixLabel("Shield", overlay: true), MinValue(0)]
		public int shieldCost;
		[HorizontalGroup("Cost/A"), HideLabel, SuffixLabel("Material", overlay: true), MinValue(0)]
		public int materialCost;
		[HorizontalGroup("Cost/A"), HideLabel, SuffixLabel("Battery", overlay: true), MinValue(0)]
		public int batteryCost;
		[HorizontalGroup("Cost/A"), HideLabel, SuffixLabel("Ammunition", overlay: true), MinValue(0)]
		public int ammoCost;

		[TitleGroup("Time", subtitle: "각 요소별 필요한 시간", alignment: TitleAlignments.Split)]
		[SuffixLabel(" s", overlay: true), MinValue(0)]
		public float cooldown = 0f;

		[TitleGroup("Helper",subtitle:"기타 연산에 도움이 될 여러 값 모음", alignment: TitleAlignments.Split)]
		[ToggleLeft,LabelText("유저의 컨트롤을 받는 UnitAI가 해당 Actuator를 자동으로 사용 할 수 있는가?")]
		[Tooltip("오직 사용자가 수동으로만 활성화 할수 있다면 false\n상대 유닛은 이 옵션을 무시함\n(기본값 true)")]
		public bool UserUnitAI_CanUseAuto = true;

		[TitleGroup("Helper")]
		[ToggleLeft,LabelText("동작 중에 취소 할 수 있음 (기본값 true)")]
		public bool IsCanCancelInPlay = true;

		[TitleGroup("Helper")]
		[ToggleLeft,LabelText("동작이 즉시 최소 되는지? (기본값 true)")]
		[Tooltip("true 일 경우 PlayAnimator에 지정된 Exit 시간을 사용")]
		[ShowIf("@IsCanCancelInPlay")]
		public bool IsImmediateCancel = true;
	}
}

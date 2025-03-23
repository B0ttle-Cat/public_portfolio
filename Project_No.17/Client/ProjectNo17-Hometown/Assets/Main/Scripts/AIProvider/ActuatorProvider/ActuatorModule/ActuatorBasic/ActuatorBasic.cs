using Sirenix.OdinInspector;

using UnityEngine;

namespace BC.AIProvider
{
	[CreateAssetMenu(fileName = "ActuatorBasic", menuName = "Scriptable Objects/ActuatorBasic")]
	public class ActuatorBasic : ScriptableObject
	{
		[TitleGroup("Cost", subtitle:"사용시 필요한 비용 / 지불 타이밍", alignment: TitleAlignments.Split)]
		[HorizontalGroup("Cost/A"), HideLabel, SuffixLabel("Shield ", overlay: true), MinValue(0)]
		public int shieldCost;
		[HorizontalGroup("Cost/A"), HideLabel, SuffixLabel("Material ", overlay: true), MinValue(0)]
		public int materialCost;
		[HorizontalGroup("Cost/A"), HideLabel, SuffixLabel("Battery ", overlay: true), MinValue(0)]
		public int batteryCost;
		[HorizontalGroup("Cost/A"), HideLabel, SuffixLabel("Ammunition ", overlay: true), MinValue(0)]
		public int ammoCost;

		[HorizontalGroup("Cost/B"), HideLabel, DisableIf("@shieldCost==0")]
		public TimingOfPayment shieldCostPay;
		[HorizontalGroup("Cost/B"), HideLabel, DisableIf("@materialCost==0")]
		public TimingOfPayment materialCostPay;
		[HorizontalGroup("Cost/B"), HideLabel, DisableIf("@batteryCost==0")]
		public TimingOfPayment batteryCostPay;
		[HorizontalGroup("Cost/B"), HideLabel, DisableIf("@ammoCost==0")]
		public TimingOfPayment ammoCostPay;

		public enum TimingOfPayment
		{
			[InspectorName("Player로 제어")]
			ManuallyWithCode = 0,

			[InspectorName("캐스팅 시작시")]
			StartCastingTime = 1,
			[InspectorName("실행 시작시")]
			StartActionTime = 2,
			[InspectorName("쿨다운 시작시")]
			StartCooldownTime = 3,
			[InspectorName("쿨다운 종료시")]
			EndedCooldownTime = 4,
		}

		[TitleGroup("Time&Anim",subtitle:"각 타이밍 별 초 단위 시간과 애니메이션", alignment: TitleAlignments.Split)]
		[TabGroup("Time&Anim/A", "Trigger")]
		public string[] castingTrigger = new string[0];
		[TabGroup("Time&Anim/A", "Trigger")]
		public string[] actionTrigger = new string[0];
		[TabGroup("Time&Anim/A", "Trigger")]
		public string[] cooldownTrigger = new string[0];
		[TabGroup("Time&Anim/A", "Original Clip"),  ReadOnly]
		public AnimationClip originalCastingClip;
		[TabGroup("Time&Anim/A", "Original Clip"),  ReadOnly]
		public AnimationClip originalActionClip;
		[TabGroup("Time&Anim/A", "Original Clip"),  ReadOnly]
		public AnimationClip originalCooldownClip;
		[TabGroup("Time&Anim/A", "Override Clip"), HideLabel,Header("CastingClip"), SuffixLabel("s ", overlay: true), InlineButton("CastingTimeFitClip","Fit Clip")]
		public float castingTime;
		[TabGroup("Time&Anim/A", "Override Clip"), HideLabel, AssetSelector(Paths = "Assets/Main/Resources/Animation|Assets/DownloadAssets")]
		public AnimationClip castingClip;
		[TabGroup("Time&Anim/A", "Override Clip"), HideLabel, Header("ActionClip"), SuffixLabel("s ", overlay: true), InlineButton("ActionTimeFitClip","Fit Clip")]
		public float actionTime;
		[TabGroup("Time&Anim/A", "Override Clip"), HideLabel, AssetSelector(Paths = "Assets/Main/Resources/Animation|Assets/DownloadAssets")]
		public AnimationClip actionClip;
		[TabGroup("Time&Anim/A", "Override Clip"), HideLabel, Header("CooldownClip"), SuffixLabel("s ", overlay: true), InlineButton("CooldownTimeFitClip","Fit Clip")]
		public float cooldownTime;
		[TabGroup("Time&Anim/A", "Override Clip"), HideLabel, AssetSelector(Paths = "Assets/Main/Resources/Animation|Assets/DownloadAssets")]
		public AnimationClip cooldownClip;

		[TabGroup("Time&Anim/A", "Clip Preview"), ShowInInspector, HideLabel, EnableGUI, HideReferenceObjectPicker, Header("CastingClip"), AnimationPreview]
		private AnimationClip previewCastingClip { get => castingClip; set => castingClip = value; }
		[TabGroup("Time&Anim/A", "Clip Preview"), ShowInInspector, HideLabel, EnableGUI, HideReferenceObjectPicker, Header("ActionClip"), AnimationPreview]
		private AnimationClip previewActionClip { get => actionClip; set => actionClip = value; }
		[TabGroup("Time&Anim/A", "Clip Preview"), ShowInInspector, HideLabel, EnableGUI, HideReferenceObjectPicker, Header("CooldownClip"), AnimationPreview]
		private AnimationClip previewCooldownClip { get => cooldownClip; set => cooldownClip = value; }

		private void CastingTimeFitClip() => castingTime = castingClip == null ? 0f : castingClip.length;
		private void ActionTimeFitClip() => actionTime = actionClip == null ? 0f : actionClip.length;
		private void CooldownTimeFitClip() => cooldownTime = cooldownClip == null ? 0f : cooldownClip.length;

		[TitleGroup("AI Helper",subtitle:"AI 연산에 도움이 될 여러 값 모음", alignment: TitleAlignments.Split)]
		[ToggleLeft,LabelText("유저가 컨트롤할 경우, UnitAI가 자동으로 사용 할 수 있는가?")]
		public bool UserUnitAI_CanUseAuto;
	}
}

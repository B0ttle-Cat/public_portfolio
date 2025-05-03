#if UNITY_EDITOR
using System;
using System.Collections.Generic;

using BC.AIProvider;
using BC.Base;
using BC.ODCC;
using BC.OdccBase;

using Newtonsoft.Json.Linq;

using Sirenix.OdinInspector;

using UnityEngine;

using Random = UnityEngine.Random;

namespace BC.Actuator
{
	public partial class ActuatorModule// ActuatorSimpleTester //
	{
		[TabGroup("ActuatorModule", "SampleTest", order: 999)]
		ActuatorFormulaSystem[] abilityEffectFormulaItemList;
		[Serializable, InlineProperty]
		private struct Sample
		{
			public List<AbilityValue> abilitys;

			public float GetValue(AbilityType abilityType)
			{
				int findIndex = abilitys.FindIndex(i => i.abilityType == abilityType);
				if(findIndex>=0) return abilitys[findIndex].abilityValue;
				else throw new Exception("셈플에 지정된 항목이 없습니다.");
			}

			internal void Add(AbilityValue abilityValue)
			{
				int findIndex = abilitys.FindIndex(i => i.abilityType == abilityValue.abilityType);
				if(findIndex<0)
				{
					abilitys.Add(abilityValue);
				}
			}
		}
		[TabGroup("ActuatorModule", "SampleTest")]
		[ShowInInspector, TextArea(0, 10), HideLabel, PropertyOrder(97)]
		private string FullFormulaText {
			get {
				if(UnityEditor.Selection.activeObject is not ActuatorFormulaItem thisEffect) return "";
				var list = thisEffect.GetAllFormulaItemArray();

				for(int i = 0 ; i < list.Length ; i++)
				{
					var effectInfo = list[i];
					if(effectInfo  != null && effectInfo.EffectType == SampleEffectType && effectInfo.FormulaSystem != null)
					{
						return effectInfo.FormulaSystem.ToString();
					}
				}
				return "해당하는 계산식을 찾을 수 없습니다.";
			}
		}

		[HorizontalGroup("ActuatorModule/SampleTest/TestSample"), SerializeField, ShowInInspector, PropertyOrder(98)]
		[Title("Order"), HideLabel]
		private Sample TestOrder;
		[HorizontalGroup("ActuatorModule/SampleTest/TestSample"), SerializeField, ShowInInspector, PropertyOrder(98)]
		[Title("Target"), HideLabel]
		private Sample TestTarget;

		[ButtonGroup("ActuatorModule/SampleTest/TestSampleA"), SerializeField, ShowInInspector, PropertyOrder(99)]
		private void InitAbilityType()
		{
			// ActuatorFormulaAbility
			// AbilityType ability;
			HashSet<AbilityType> allAbilityType = new HashSet<AbilityType>();
			for(int i = 0 ; i < abilityEffectFormulaItemList.Length ; i++)
			{
				string formulaJson = JsonUtility.ToJson(abilityEffectFormulaItemList[i].FormulaSystem, true);
				var token = JToken.Parse(formulaJson);

				FindAbilities(token, allAbilityType);
			}
			foreach(var abilityType in allAbilityType)
			{
				TestOrder.Add(new AbilityValue { abilityType = abilityType, abilityValue = Random.Range(0, 100) });
				TestTarget.Add(new AbilityValue { abilityType = abilityType, abilityValue = Random.Range(0, 100) });
			}

			static void FindAbilities(JToken token, HashSet<AbilityType> abilities)
			{
				if(token is JObject obj)
				{
					foreach(var property in obj.Properties())
					{
						if(property.Name == "ability")
						{
							var value = property.Value.ToString();
							if(Enum.TryParse(value, out AbilityType abilityType))
							{
								abilities.Add(abilityType);
							}
						}
						else
						{
							FindAbilities(property.Value, abilities);
						}
					}
				}
				else if(token is JArray arr)
				{
					foreach(var item in arr)
					{
						FindAbilities(item, abilities);
					}
				}
			}
		}

		[ButtonGroup("ActuatorModule/SampleTest/TestSampleB"), SerializeField, ShowInInspector, PropertyOrder(99)]
		private void OrderRandomValue()
		{
			var abilitys = TestOrder.abilitys;
			for(int i = 0 ; i < TestOrder.abilitys.Count ; i++)
			{
				var ability = TestOrder.abilitys[i];
				ability.abilityValue = Random.Range(0, 100);
				TestOrder.abilitys[i] = ability;
			}
			TestOrder.abilitys = abilitys;
		}
		[ButtonGroup("ActuatorModule/SampleTest/TestSampleB"), SerializeField, ShowInInspector, PropertyOrder(99)]
		private void TargetRandomValue()
		{
			var abilitys = TestTarget.abilitys;
			for(int i = 0 ; i < abilitys.Count ; i++)
			{
				var ability = abilitys[i];
				ability.abilityValue = Random.Range(0, 100);
				abilitys[i] = ability;
			}
			TestTarget.abilitys = abilitys;
		}

		[HorizontalGroup("ActuatorModule/SampleTest/TestSample2"), ShowInInspector, PropertyOrder(100), HideLabel]
		private string SampleResult { get; set; }

		[HorizontalGroup("ActuatorModule/SampleTest/TestSample2"), HideLabel, InlineButton("TestFormula"), ShowInInspector, PropertyOrder(101)]
		ActuatorEffectType SampleEffectType;
		public void TestFormula()
		{
			if(UnityEditor.Selection.activeObject is not ActuatorFormulaItem thisEffect) return;

			var list = thisEffect.GetAllFormulaItemArray();

			HashSet<ActuatorEffectType> isCalculatedType = new HashSet<ActuatorEffectType>();
			isCalculatedType.Add(SampleEffectType);

			for(int i = 0 ; i < list.Length ; i++)
			{
				if(list[i].EffectType == SampleEffectType)
				{
					_TestFormula(list[i]);
					return;
				}
			}
			SampleResult = $"Can Not Found {SampleEffectType}";
			void _TestFormula(ActuatorFormulaSystem effectInfo)
			{
				if(effectInfo.TryCalculate(_GetAbility, _GetEffectType, out float result))
				{
					SampleResult = result.ToString();
				}
				else
				{
					SampleResult = "False";
				}
			}
			float _GetAbility(ActuatorFormulaGetterType getter, AbilityType ability) => getter switch {
				ActuatorFormulaGetterType.Order => TestOrder.GetValue(ability),
				ActuatorFormulaGetterType.Target => TestTarget.GetValue(ability),
				_ => 0
			};
			float _GetEffectType(ActuatorEffectType externalEffect)
			{
				if(isCalculatedType.Add(externalEffect))
				{
					for(int i = 0 ; i < list.Length ; i++)
					{
						var effectInfo = list[i];
						if(effectInfo != null && effectInfo.EffectType == externalEffect)
						{
							return effectInfo.Calculate(_GetAbility, _GetEffectType);
						}
					}
				}
				return 0;
			}
		}
		[FoldoutGroup("ActuatorModule/SampleTest/RepeatHistogram"), ShowInInspector, PropertyOrder(200), InlineButton("GenerateHistogram")]private int numberOfSamples = 10000;
		[FoldoutGroup("ActuatorModule/SampleTest/RepeatHistogram"), ShowInInspector, PropertyOrder(200)] private bool minIsZero;
		[FoldoutGroup("ActuatorModule/SampleTest/RepeatHistogram"), ShowInInspector, PropertyOrder(200)] private Vector2Int minMaxCount = Vector2Int.up;
		private Vector2Int minMaxCountRange = Vector2Int.up;
		[FoldoutGroup("ActuatorModule/SampleTest/RepeatHistogram"), ShowInInspector, PropertyOrder(200), LabelText("@nameBar0"), ProgressBar("@minMaxCountRange.x", "@minMaxCountRange.y") ]private int countBar0 = 0; private string nameBar0 = "0";
		[FoldoutGroup("ActuatorModule/SampleTest/RepeatHistogram"), ShowInInspector, PropertyOrder(200), LabelText("@nameBar1"), ProgressBar("@minMaxCountRange.x", "@minMaxCountRange.y") ]private int countBar1 = 0; private string nameBar1 = "1";
		[FoldoutGroup("ActuatorModule/SampleTest/RepeatHistogram"), ShowInInspector, PropertyOrder(200), LabelText("@nameBar2"), ProgressBar("@minMaxCountRange.x", "@minMaxCountRange.y") ]private int countBar2 = 0; private string nameBar2 = "2";
		[FoldoutGroup("ActuatorModule/SampleTest/RepeatHistogram"), ShowInInspector, PropertyOrder(200), LabelText("@nameBar3"), ProgressBar("@minMaxCountRange.x", "@minMaxCountRange.y") ]private int countBar3 = 0; private string nameBar3 = "3";
		[FoldoutGroup("ActuatorModule/SampleTest/RepeatHistogram"), ShowInInspector, PropertyOrder(200), LabelText("@nameBar4"), ProgressBar("@minMaxCountRange.x", "@minMaxCountRange.y") ]private int countBar4 = 0; private string nameBar4 = "4";
		[FoldoutGroup("ActuatorModule/SampleTest/RepeatHistogram"), ShowInInspector, PropertyOrder(200), LabelText("@nameBar5"), ProgressBar("@minMaxCountRange.x", "@minMaxCountRange.y") ]private int countBar5 = 0; private string nameBar5 = "5";
		[FoldoutGroup("ActuatorModule/SampleTest/RepeatHistogram"), ShowInInspector, PropertyOrder(200), LabelText("@nameBar6"), ProgressBar("@minMaxCountRange.x", "@minMaxCountRange.y") ]private int countBar6 = 0; private string nameBar6 = "6";
		[FoldoutGroup("ActuatorModule/SampleTest/RepeatHistogram"), ShowInInspector, PropertyOrder(200), LabelText("@nameBar7"), ProgressBar("@minMaxCountRange.x", "@minMaxCountRange.y") ]private int countBar7 = 0; private string nameBar7 = "7";
		[FoldoutGroup("ActuatorModule/SampleTest/RepeatHistogram"), ShowInInspector, PropertyOrder(200), LabelText("@nameBar8"), ProgressBar("@minMaxCountRange.x", "@minMaxCountRange.y") ]private int countBar8 = 0; private string nameBar8 = "8";
		[FoldoutGroup("ActuatorModule/SampleTest/RepeatHistogram"), ShowInInspector, PropertyOrder(200), LabelText("@nameBar9"), ProgressBar("@minMaxCountRange.x", "@minMaxCountRange.y") ]private int countBar9 = 0; private string nameBar9 = "9";
		[FoldoutGroup("ActuatorModule/SampleTest/RepeatHistogram"), ShowInInspector, PropertyOrder(200)] private Vector2 minMaxValue = Vector2.up;
		[FoldoutGroup("ActuatorModule/SampleTest/RepeatHistogram"), ShowInInspector, PropertyOrder(200), LabelText("전체 평균"), ProgressBar("@minMaxValue.x", "@minMaxValue.y",BackgroundColorGetter = "#FFFFFF")] private float resultAverage = 0f;
		private void GenerateHistogram()
		{
			// 기존 카운트를 초기화
			countBar0 = countBar1 = countBar2 = countBar3 = countBar4 =
			countBar5 = countBar6 = countBar7 = countBar8 = countBar9 = 0;

			nameBar0 = nameBar1 = nameBar2 = nameBar3 = nameBar4 =
			nameBar5 = nameBar6 = nameBar7 = nameBar8 = nameBar9 = "";

			minMaxCountRange = minMaxCount = Vector2Int.up;
			minMaxValue = Vector2.up;

			// 1. 랜덤 샘플 생성 및 최대값 계산
			float[] samples = new float[numberOfSamples];
			float minValue = float.MaxValue;
			float maxValue = float.MinValue;
			for(int i = 0 ; i < numberOfSamples ; i++)
			{
				// 0 ~ 100 범위의 랜덤값 생성
				if(!RandomTestFormula(out float sample)) return;
				samples[i] = sample;
				if(sample > maxValue) maxValue = sample;
				if(sample < minValue) minValue = sample;
			}
			if(maxValue < minValue) return;
			// 2. 최대값을 기준으로 10개의 구간으로 나눕니다.
			// maxValue가 0인 경우를 방지하기 위해 binSize를 1로 설정합니다.
			float delta = maxValue - minValue;
			if(delta < float.Epsilon) return;
			float binSize = (delta / 10f);
			// 3. 각 샘플이 어느 구간에 속하는지 계산하여 해당 countBar 변수를 증가시킵니다.


			nameBar0 = $"{minValue:0.00}~{minValue+binSize*1:0.00}";
			nameBar1 = $"{minValue+binSize*1:0.00}~{minValue+binSize*2:0.00}";
			nameBar2 = $"{minValue+binSize*2:0.00}~{minValue+binSize*3:0.00}";
			nameBar3 = $"{minValue+binSize*3:0.00}~{minValue+binSize*4:0.00}";
			nameBar4 = $"{minValue+binSize*4:0.00}~{minValue+binSize*5:0.00}";
			nameBar5 = $"{minValue+binSize*5:0.00}~{minValue+binSize*6:0.00}";
			nameBar6 = $"{minValue+binSize*6:0.00}~{minValue+binSize*7:0.00}";
			nameBar7 = $"{minValue+binSize*7:0.00}~{minValue+binSize*8:0.00}";
			nameBar8 = $"{minValue+binSize*8:0.00}~{minValue+binSize*9:0.00}";
			nameBar9 = $"{minValue+binSize*9:0.00}~{maxValue:0.00}";

			int countAverage = 0;
			resultAverage = 0f;
			minMaxValue = new Vector2(minValue, maxValue);

			foreach(float sample in samples)
			{
				// 구간 인덱스 계산
				int bin = (int)((sample - minValue) / binSize);

				// 만약 sample이 maxValue와 같아서 bin이 10이 되는 경우 마지막 구간(9)으로 보정
				//if(bin >= 10)
				//	bin = 9;
				countAverage++;
				resultAverage += (sample - resultAverage) / countAverage;

				switch(bin)
				{
					case -1: countBar0++; break;
					case 0: countBar0++; break;
					case 1: countBar1++; break;
					case 2: countBar2++; break;
					case 3: countBar3++; break;
					case 4: countBar4++; break;
					case 5: countBar5++; break;
					case 6: countBar6++; break;
					case 7: countBar7++; break;
					case 8: countBar8++; break;
					case 9: countBar9++; break;
					case 10: countBar9++; break;
				}
			}
			int maxCount = 0;
			if(maxCount < countBar0) maxCount = countBar0;
			if(maxCount < countBar1) maxCount = countBar1;
			if(maxCount < countBar2) maxCount = countBar2;
			if(maxCount < countBar3) maxCount = countBar3;
			if(maxCount < countBar4) maxCount = countBar4;
			if(maxCount < countBar5) maxCount = countBar5;
			if(maxCount < countBar6) maxCount = countBar6;
			if(maxCount < countBar7) maxCount = countBar7;
			if(maxCount < countBar8) maxCount = countBar8;
			if(maxCount < countBar9) maxCount = countBar9;
			int minCount = maxCount;
			if(minCount > countBar0) minCount = countBar0;
			if(minCount > countBar1) minCount = countBar1;
			if(minCount > countBar2) minCount = countBar2;
			if(minCount > countBar3) minCount = countBar3;
			if(minCount > countBar4) minCount = countBar4;
			if(minCount > countBar5) minCount = countBar5;
			if(minCount > countBar6) minCount = countBar6;
			if(minCount > countBar7) minCount = countBar7;
			if(minCount > countBar8) minCount = countBar8;
			if(minCount > countBar9) minCount = countBar9;
			minMaxCount = new Vector2Int(minIsZero ? 0 : minCount, maxCount);
			maxCount = (int)((float)maxCount * 1.1f);
			minCount = (int)((float)minCount * 0.9f);
			minMaxCountRange = new Vector2Int(minIsZero ? 0 : minCount, maxCount);
			bool RandomTestFormula(out float value)
			{
				value = 0f;
				if(UnityEditor.Selection.activeObject is not ActuatorFormulaItem thisEffect) return false;

				var list = thisEffect.GetAllFormulaItemArray();

				HashSet<ActuatorEffectType> isCalculatedType = new HashSet<ActuatorEffectType>();
				isCalculatedType.Add(SampleEffectType);

				for(int i = 0 ; i < list.Length ; i++)
				{
					if(list[i].EffectType == SampleEffectType)
					{
						if(_TestFormula(list[i], out value))
						{

							return true;
						}
					}
				}
				return false;
				bool _TestFormula(ActuatorFormulaSystem effectInfo, out float value)
				{
					if(effectInfo.TryCalculate(_GetAbility, _GetEffectType, out float result))
					{
						value = result;
						return true;
					}
					else
					{
						value = 0;
						return false;
					}
				}

				float _GetAbility(ActuatorFormulaGetterType getter, AbilityType ability) => getter switch {
					ActuatorFormulaGetterType.Order => TestOrder.GetValue(ability),
					ActuatorFormulaGetterType.Target => TestTarget.GetValue(ability),
					_ => 0
				};
				float _GetEffectType(ActuatorEffectType externalEffect)
				{
					if(isCalculatedType.Add(externalEffect))
					{
						for(int i = 0 ; i < list.Length ; i++)
						{
							var effectInfo = list[i];
							if(effectInfo != null && effectInfo.EffectType == externalEffect)
							{
								return effectInfo.Calculate(_GetAbility, _GetEffectType);
							}
						}
					}
					return 0;
				}
			}
		}




#if UNITY_EDITOR
		[FoldoutGroup("ActuatorModule/SampleTest/CanTarget")]
		[Button("Test Can ActuatorTarget (Walk Only Play Mode)")]
		private bool TestCanActuatorTarget(ObjectBehaviour order, ObjectBehaviour target, UnitRelationType unitRelationType)
		{

			if(order == null || target == null) return false;

			var orderIFF = order.GetComponentInChildren<IFFProvider>(true);
			if(orderIFF == null) return false;
			var orderAbility = order.GetComponentInChildren<AbilityProvider>(true);
			if(orderAbility == null) return false;

			var targetIFF = target.GetComponentInChildren<IFFProvider>(true);
			if(targetIFF == null) return false;
			var targetAbility = target.GetComponentInChildren<AbilityProvider>(true);
			if(targetAbility == null) return false;

			var reactionTarget = target.GetComponentInChildren<ReactionTarget>(true);
			if(reactionTarget != null && !Target.hitTargetLayerMask.HasLayer(reactionTarget.Layer)) return false;

			if(UnityEditor.EditorApplication.isPlaying)
			{
				unitRelationType = IFFProvider.IFFMatching.GetUnitRelation(orderIFF, targetIFF);
			}
			return IsCanActuatorTarget(orderAbility, targetAbility, unitRelationType, reactionTarget.Layer);
		}
#endif
	}
}

#endif
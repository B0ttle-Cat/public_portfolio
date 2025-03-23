using System;
using System.Collections.Generic;
using System.Linq;

using BC.ODCC;

using Sirenix.OdinInspector;

using UnityEngine;

namespace BC.OdccBase
{
	public interface IAbilityStruct : IOdccComponent
	{
		public bool HasAbility(AbilityType abilityType);
		public bool HasNotAbility(AbilityType abilityType) => !HasAbility(abilityType);
		public bool TryGetAbility(AbilityType abilityType, out float value);
		public bool TrySetAbility(AbilityType abilityType, in float value) { return false; }
		public void RemoveAbility(AbilityType abilityType) { }
	}

	[Serializable, HideLabel, HideReferenceObjectPicker]
	public class AbilityStruct
	{
		[SerializeField]
		[ListDrawerSettings(ShowPaging = false)]
		private List<AbilityValue> abilityValueList;

		public AbilityStruct()
		{
			this.abilityValueList = new List<AbilityValue>();
		}
		public AbilityStruct(List<AbilityValue> abilityValueList)
		{
			this.abilityValueList = abilityValueList ?? new List<AbilityValue>();
		}

		public bool HasAbility(AbilityType abilityType)
		{
			int length = abilityValueList.Count;
			for(int i = 0 ; i < length ; i++)
			{
				AbilityValue abilityValue = abilityValueList[i];
				if(abilityValue.abilityType == abilityType)
				{
					return true;
				}
			}
			return false;
		}
		public bool HasNotAbility(AbilityType abilityType)
		{
			return !HasAbility(abilityType);
		}
		public bool TryGetAbility(AbilityType abilityType, out float value)
		{
			int length = abilityValueList.Count;
			for(int i = 0 ; i < length ; i++)
			{
				AbilityValue abilityValue = abilityValueList[i];
				if(abilityValue.abilityType == abilityType)
				{
					value = abilityValue.abilityValue;
					return true;
				}
			}
			value = 0;
			return false;
		}
		public bool TrySetAbility(AbilityType abilityType, in float value)
		{
			int length = abilityValueList.Count;
			for(int i = 0 ; i < length ; i++)
			{
				AbilityValue abilityValue = abilityValueList[i];
				if(abilityValue.abilityType == abilityType)
				{
					abilityValue.abilityValue = value;
					abilityValueList[i] = abilityValue;
					return true;
				}
			}
			return false;
		}
		public void RemoveAbility(AbilityType abilityType)
		{
			int length = abilityValueList.Count;
			for(int i = 0 ; i < length ; i++)
			{
				AbilityValue abilityValue = abilityValueList[i];
				if(abilityValue.abilityType == abilityType)
				{
					abilityValueList.RemoveAt(i);
					return;
				}
			}
		}

		public void SetAbility(AbilityStruct abilityStruct)
		{
			abilityValueList = abilityStruct.abilityValueList;
		}

		public static float GetAbilityMergeValue(AbilityType abilityType, params AbilityStruct[] abilityStructList)
		{
			float value = 0f;
			int length = abilityStructList.Length;
			for(int i = 0 ; i < length ; i++)
			{
				var ability = abilityStructList[i];
				if(ability.TryGetAbility(abilityType, out float _value))
				{
					value += _value;
				}
			}
			return value;
		}
		public static float GetAbilityMergeValue(AbilityType abilityType, params IEnumerable<AbilityValue>[] abilityValueList)
		{
			float value = 0f;
			int length = abilityValueList.Length;
			for(int i = 0 ; i < length ; i++)
			{
				var abilityList = abilityValueList[i];
				if(_TryGetAbility(abilityType, abilityList, out float _value))
				{
					value += _value;
				}
			}
			return value;

			bool _TryGetAbility(AbilityType abilityType, IEnumerable<AbilityValue> abilityValueList, out float value)
			{
				foreach(var abilityValue in abilityValueList)
				{
					if(abilityValue.abilityType == abilityType)
					{
						value = abilityValue.abilityValue;
						return true;
					}
				}
				value = 0;
				return false;
			}
		}
		public AbilityStruct NewCopy()
		{
			AbilityStruct abilityStruct = new AbilityStruct();
			abilityStruct.abilityValueList.AddRange(this.abilityValueList);
			return abilityStruct;
		}
		//		[Button, PropertyOrder(-1)]
		public void AddMissingType()
		{
			// 현재 존재하는 AbilityType 목록 추출
			var existingTypes = abilityValueList.Select(av => av.abilityType).ToHashSet();

			// 모든 AbilityType을 순회하며 누락된 항목 추가
			foreach(AbilityType type in Enum.GetValues(typeof(AbilityType)))
			{
				if(!existingTypes.Contains(type))
				{
					abilityValueList.Add(new AbilityValue { abilityType = type, abilityValue = 0f });
				}
			}

			// AbilityType 순서대로 정렬
			abilityValueList = abilityValueList.OrderBy(av => av.abilityType).ToList();
		}
	}
}

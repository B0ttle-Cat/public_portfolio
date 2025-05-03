using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;

namespace BC.OdccBase
{
    public enum AbilitySetupFilter
    {
        None = 0,
        Character,
    }

    [CreateAssetMenu(fileName = "AbilityTable", menuName = "BC/Scriptable Objects/AbilityTable")]
    public class AbilityTable : ScriptableObject
    {
        public AbilitySetupFilter abilitySetupFilter;

        [Title("Ability Settings")]
        [Tooltip("1레벨부터 시작하여 각 레벨에 대한 능력치 정보입니다.")]
        [SerializeField, ListDrawerSettings(ShowPaging = false)]
        private List<AbilityStruct> abilityStructList = new List<AbilityStruct>();

        // 최대 레벨 반환
        public int MaxLevel
        {
            get => abilityStructList.Count;
            set
            {
                if (value < 1) value = 1;
                if (value == abilityStructList.Count) return;
                if (value > abilityStructList.Count)
                {
                    AbilityStruct[] addList = new AbilityStruct[value - abilityStructList.Count];
                    int addCount = addList.Length;
                    for (int i = 0; i < addCount; i++)
                    {
                        addList[i] = new AbilityStruct();
                    }
                    abilityStructList.AddRange(addList);
                }
                else
                {
                    abilityStructList.RemoveRange(value, abilityStructList.Count - value);
                }
            }
        }

        public List<int> requiredExperiencePoints;

        public void AddAbilityType(AbilityType type, float value = 0f)
        {
            foreach (var item in abilityStructList)
            {
                if (item.TryGetAbility(type, out float _value))
                {
                    value = _value;
                }
                item.AddAbility(type, value);
            }
        }
        public void RemoveAbilityType(AbilityType type)
        {
            foreach (var item in abilityStructList)
            {
                item.RemoveAbility(type);
            }
        }

        public AbilityValue GetAbilityValueWithLevel(AbilityType abilityType, int selectLevel)
        {
            if (MaxLevel > 0)
            {
                int selectIndex = selectLevel - 1;
                if (selectIndex < 0) selectIndex = 0;
                else if (selectIndex >= MaxLevel) selectIndex = MaxLevel - 1;
                if (abilityStructList[selectIndex].TryGetAbility(abilityType, out float value))
                {
                    return new AbilityValue(abilityType, value);
                }
            }
            return new AbilityValue(abilityType, 0f);
        }

        public void SetAbilityValueWithLevelList(List<List<AbilityValue>> allLvelList)
        {
            abilityStructList = new List<AbilityStruct>(allLvelList.Count);
            for (int i = 0; i < allLvelList.Count; i++)
            {
                abilityStructList.Add(new AbilityStruct(allLvelList[i]));
            }
        }
    }
}

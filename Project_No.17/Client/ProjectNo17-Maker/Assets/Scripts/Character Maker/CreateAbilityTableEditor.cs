using System.Collections.Generic;
using System.Linq;

using BC.OdccBase;

using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

using UnityEditor;

using UnityEngine;

using static UnityEngine.Rendering.DebugUI;

public enum AbilityOutRangeLevelComputeLogic
{
    Last, Loop, Zero,
}

public partial class CreateAbilityTableEditor : ScriptableObject
{
    [HideIf("@true")]
    public CreateAbilityTableEditorWindow editorWindow;

    // [HideIf("@true")]
    public List<AbilityTableEditor> abilityTableEditorList;

    [HideIf("@true")]
    private int maxLevel;

    public void OnValidate()
    {
        Debug.Log("OnValidate");
        int length = abilityTableEditorList == null ? 0 : abilityTableEditorList.Count;
        for (int i = 0; i < length; i++)
        {
            if (abilityTableEditorList[i] != null)
            {
                abilityTableEditorList[i].config = this;
                abilityTableEditorList[i].OnValidate();
            }
        }
    }
}

public partial class CreateAbilityTableEditor // Toolbar
{
    public void CreateNewAbilityTableEditor(AbilityTable abilityTable)
    {

        string tablePath = AssetDatabase.GetAssetPath(this);
        Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(tablePath);
        foreach (var asset in subAssets)
        {
            if (asset is AbilityTableEditor _asset)
            {
                if (_asset.name == abilityTable.name)
                {
                    _asset.config = this;
                    _asset.abilityTable = abilityTable;
                    OnValidate();
                    return;
                }
            }
        }

        var editor = ScriptableObject.CreateInstance<AbilityTableEditor>();
        editor.name = abilityTable.name;
        editor.config = this;
        editor.abilityTable = abilityTable;
        AssetDatabase.AddObjectToAsset(editor, this);

        abilityTableEditorList.Add(editor);
        OnValidate();
    }
    internal void LoadAbilityTableEditor(AbilityTable abilityTable)
    {
        string tablePath = AssetDatabase.GetAssetPath(this);
        Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(tablePath);

        AbilityTableEditor editor = null;
        foreach (var asset in subAssets)
        {
            if (asset is AbilityTableEditor)
            {
                editor = asset as AbilityTableEditor;
                break;
            }
        }

        if (editor == null)
        {
            editor = ScriptableObject.CreateInstance<AbilityTableEditor>();
            editor.name = abilityTable.name;
            editor.config = this;
            editor.abilityTable = abilityTable;
            AssetDatabase.AddObjectToAsset(editor, this);
        }

        if (!abilityTableEditorList.Contains(editor))
        {
            abilityTableEditorList.Add(editor);
        }
        OnValidate();
    }
    public void DuplicateSelectedEditor(AbilityTable abilityTable)
    {
        var editor = ScriptableObject.CreateInstance<AbilityTableEditor>();
        editor.name = abilityTable.name;
        editor.config = this;
        editor.abilityTable = abilityTable;
        AssetDatabase.AddObjectToAsset(editor, this);
        abilityTableEditorList.Add(editor);
        OnValidate();
    }
    public void DeleteSelected(AbilityTableEditor editor)
    {
        abilityTableEditorList.Remove(editor);
        AssetDatabase.RemoveObjectFromAsset(editor);

    }

    public static float RoundToDigits(float value, int decimalPlaces)
    {
        float multiplier = Mathf.Pow(10, decimalPlaces);
        return Mathf.Round(value * multiplier) / multiplier;
    }
    /// <summary>지정 자릿수까지 올림</summary>
    public static float CeilToDigits(float value, int decimalPlaces)
    {
        float extendedMultiplier = Mathf.Pow(10f, decimalPlaces + 1);
        float roundedExtendedValue = Mathf.Round(value * extendedMultiplier);
        float valueAtBaseScale = roundedExtendedValue / 10f;
        float baseMultiplier = Mathf.Pow(10f, decimalPlaces);

        return Mathf.Ceil(valueAtBaseScale) / baseMultiplier;
    }

    /// <summary>지정 자릿수까지 내림</summary>
    public static float FloorToDigits(float value, int decimalPlaces)
    {
        float extendedMultiplier = Mathf.Pow(10f, decimalPlaces + 1);
        float roundedExtendedValue = Mathf.Round(value * extendedMultiplier);
        float valueAtBaseScale = roundedExtendedValue / 10f;
        float baseMultiplier = Mathf.Pow(10f, decimalPlaces);

        return Mathf.Floor(valueAtBaseScale) / baseMultiplier;
    }

    public static List<AbilityType> GetAllAbilityType()
    {
        List<AbilityType> list = new List<AbilityType>();
        foreach (AbilityType type in System.Enum.GetValues(typeof(AbilityType)))
        {
            list.Add(type);
        }

        list = list
            .Where(i => !(i.ToString().StartsWith("_")))
            .Where(i => !(i is >= AbilityType._DefaultPoint and < AbilityType._DefaultMaxPoint))
            .OrderBy(i => (int)i)
            .ToList();

        return list;
    }
    public static List<AbilityType> GetAllAbilityType(AbilitySetupFilter abilitySetupFilter)
    {
        return GetAbilitySetupType(abilitySetupFilter).Select(s => s.type).ToList();
    }

    public static float GetAbilitySetupType(AbilitySetupFilter abilitySetupType, AbilityType abilityType)
    {
        return GetAbilitySetupType(abilitySetupType).FirstOrDefault(i => i.abilityType == abilityType).value;
    }
    public static List<AbilityValue> GetAbilitySetupType(AbilitySetupFilter abilitySetupType)
    {
        return abilitySetupType switch
        {
            AbilitySetupFilter.Character => Character(),
            _ => new List<AbilityValue>()
        };

        static List<AbilityValue> Character()
        {
            List<AbilityValue> abilityTypes = new List<AbilityValue>(){
				//new AbilityValue(AbilityType.ShieldPoint,           200f),	// 각 항목의 현재 보유량
				//new AbilityValue(AbilityType.MaterialPoint,         50f),	//
				//new AbilityValue(AbilityType.BatteryPoint,          30f),	//
				//new AbilityValue(AbilityType.AmmoCount,             10f),	//
	
				new AbilityValue(AbilityType.ShieldMaxPoint,        200f),	// 각 항목의 최대 보유량
				new AbilityValue(AbilityType.MaterialMaxPoint,      50f),	//
				new AbilityValue(AbilityType.BatteryMaxPoint,       30f),	//
				new AbilityValue(AbilityType.AmmoMaxCount,          10f),	//
	
				new AbilityValue(AbilityType.MaterialRecovery,      3f),	// 효율이 100%일 경우, 초당 N 만큼 회복됨 
				new AbilityValue(AbilityType.BatteryRecovery,       3f),	// 효율이 100%일 경우, 초당 N 만큼 회복됨 
	
				new AbilityValue(AbilityType.MaterialEfficiency,    100f),	//효율 값: Recovery * N % 비율로 회복되며, 사용시 사용량 * (100 - N)% 의 낭비가 발생함. 100% 이상의 수치는 적용 되지 않음
				new AbilityValue(AbilityType.BatteryEfficiency,     100f),	//효율 값: Recovery * N % 비율로 회복되며, 사용시 사용량 * (100 - N)% 의 낭비가 발생함. 100% 이상의 수치는 적용 되지 않음
	
				// 각 항목의 적용 포인트
				new AbilityValue(AbilityType.AttackPoint,           15f),	// 공격력
				new AbilityValue(AbilityType.AttackDefensePoint,    10f),	// 방어력
				new AbilityValue(AbilityType.CriticalPoint,         5f),	// 치명타 공격점수
				new AbilityValue(AbilityType.CriticalDefensePoint,  5f),	// 치명타 방어점수
				new AbilityValue(AbilityType.BlockingPassPoint,     50f),	// 엄패 적용시 관통력 점수  // 관통률 == Clamp((관통력/피관통), 0, 1) // 관통 데미지 = 공격 * 관통률
				new AbilityValue(AbilityType.BlockingCoverPoint,    100f),	// 엄패 적용시 피관통 점수 
	
				// 각 항목의 적용 확률  // 명중/회피 계산 관계 : 명중 적용 == (랜덤(0 ~ 내 명중) > 랜덤(0 ~ 적 회피))
				new AbilityValue(AbilityType.AttackHitChance,       85f),	// 명중률
				new AbilityValue(AbilityType.AttackDodgeChance,     20f),	// 명중 회피율
				new AbilityValue(AbilityType.CriticalHitChance,     10f),	// 크리티컬 확률
				new AbilityValue(AbilityType.CriticalDodgeChance,   5f),	// 크리티컬 회피 확률
				new AbilityValue(AbilityType.BlockingPassChance,    20f),	// 엄패 무시 확률
				new AbilityValue(AbilityType.BlockingCoverChance,   10f),	// 엄패 적용 확률 
	
				new AbilityValue(AbilityType.VisibleRange,          15f),	// 거리 관련 적용 수치 (시야가 보이는 거리)
				new AbilityValue(AbilityType.ReactionRange,         0f),	// 목표물에 반응(공격,이동 등) 하는거리 (각 수치는 스킬 옵션에 붙어있고 이곳의 값은 보너스 개념)

                new AbilityValue(AbilityType.RequiredEXP,           100f),	// 현재 경험치
			};

            return abilityTypes;
        }
    }
    public static float GetMaxAbilityValue(AbilityType abilityType, float maxValue)
    {
        float defaultMax = DefaultMaxAbilityValue(abilityType);
        float stapMax = DefaultRangeStapAbilityValue(abilityType);

        if (maxValue < defaultMax)
            return defaultMax;

        // maxValue를 포함할 수 있도록 defaultMax를 stapMax 단위로 증가
        int steps = Mathf.CeilToInt((maxValue - defaultMax) / stapMax);
        return defaultMax + stapMax * (steps);
    }
    public static float GetMinAbilityValue(AbilityType abilityType, float maxValue)
    {
        float defaultMin = DefaultMinAbilityValue(abilityType);
        return maxValue > defaultMin ? defaultMin : maxValue;
    }
    // Bar 에 적용될 최대값;
    public static float DefaultMaxAbilityValue(AbilityType abilityType)
    {
        return abilityType switch
        {
            AbilityType.ShieldMaxPoint => 500,
            AbilityType.MaterialMaxPoint => 200,
            AbilityType.BatteryMaxPoint => 200,
            AbilityType.AmmoMaxCount => 100,

            AbilityType.MaterialRecovery => 50,
            AbilityType.BatteryRecovery => 50,

            AbilityType.MaterialEfficiency => 100,
            AbilityType.BatteryEfficiency => 100,

            AbilityType.AttackPoint => 100,
            AbilityType.AttackDefensePoint => 100,
            AbilityType.CriticalPoint => 100,
            AbilityType.CriticalDefensePoint => 100,
            AbilityType.BlockingPassPoint => 100,
            AbilityType.BlockingCoverPoint => 100,

            AbilityType.AttackHitChance => 100,
            AbilityType.AttackDodgeChance => 100,
            AbilityType.CriticalHitChance => 100,
            AbilityType.CriticalDodgeChance => 100,
            AbilityType.BlockingPassChance => 100,
            AbilityType.BlockingCoverChance => 100,

            AbilityType.VisibleRange => 10,
            AbilityType.ReactionRange => 10,

            AbilityType.RequiredEXP => 100,
            _ => 100,
        };
    }
    public static float DefaultMinAbilityValue(AbilityType abilityType)
    {
        return abilityType switch
        {
            _ => 0,
        };
    }
    // Bar의 값이 초고되면, 최대값에 추가죌 증가수치;
    public static float DefaultRangeStapAbilityValue(AbilityType abilityType)
    {
        return abilityType switch
        {
            AbilityType.ShieldMaxPoint => 100,
            AbilityType.MaterialMaxPoint => 100,
            AbilityType.BatteryMaxPoint => 100,
            AbilityType.AmmoMaxCount => 10,

            AbilityType.MaterialRecovery => 10,
            AbilityType.BatteryRecovery => 10,

            AbilityType.MaterialEfficiency => 10,
            AbilityType.BatteryEfficiency => 10,

            AbilityType.AttackPoint => 20,
            AbilityType.AttackDefensePoint => 20,
            AbilityType.CriticalPoint => 20,
            AbilityType.CriticalDefensePoint => 20,
            AbilityType.BlockingPassPoint => 20,
            AbilityType.BlockingCoverPoint => 20,

            AbilityType.AttackHitChance => 20,
            AbilityType.AttackDodgeChance => 20,
            AbilityType.CriticalHitChance => 20,
            AbilityType.CriticalDodgeChance => 20,
            AbilityType.BlockingPassChance => 20,
            AbilityType.BlockingCoverChance => 20,

            AbilityType.VisibleRange => 10,
            AbilityType.ReactionRange => 10,

            AbilityType.RequiredEXP => 50,
            _ => 100,
        };
    }

    public static bool IsFloatAbilityType(AbilityType value)
    {
        if (value >= AbilityType._Range)
        {
            return true;
        }
        return false;
    }
}
public partial class CreateAbilityTableEditor // OdinMenuTree
{
    [ShowInInspector]
    public int MaxLevel { get => maxLevel <= 10 ? 10 : maxLevel; set => maxLevel = (value <= 10 ? 10 : value); }

    [Button]
    public void RefreshAbilityTableList()
    {
        abilityTableEditorList = new List<AbilityTableEditor>();

        // "t:AbilityTable" 타입 필터로 모든 에셋 검색
        string tablePath = AssetDatabase.GetAssetPath(this);
        Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(tablePath);
        foreach (var asset in subAssets)
        {
            if (asset is AbilityTableEditor table)
            {
                abilityTableEditorList.Add(table);
            }
        }
        abilityTableEditorList = abilityTableEditorList.OrderBy(i => i.name).ToList();
    }

    internal void BuildMenuTree(OdinMenuTree tree)
    {
        RefreshAbilityTableList();
        for (int i = 0; i < abilityTableEditorList.Count; i++)
        {
            var editor = abilityTableEditorList[i];
            //if (editor.abilityTable == null) continue;
            var group = editor.abilitySetupFilter.ToString();
            var name = editor.name;
            tree.Add($"AbilityTables/{group}/{name}", editor);
        }
    }

    internal void RebuildMenuTree()
    {
        editorWindow.RebuildMenuTree();
    }
    internal void RepaintMenuItem()
    {
        editorWindow.RepaintMenuItem();
    }
}
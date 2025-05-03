using System;
using System.Collections.Generic;
using System.Linq;

using BC.OdccBase;

using Sirenix.OdinInspector;

using UnityEditor;

using UnityEngine;


[Serializable]
public partial class AbilityTableEditor : ScriptableObject
{
    private const int limutLevel = 300;

    public CreateAbilityTableEditor config { get; internal set; }

    [HorizontalGroup("AbilityTable", order: -2), HideLabel, ReadOnly, ShowIf("@!isChangeName"), EnableGUI]
    [InlineButton("StartChangeName", "이름 바꾸기")]
    public AbilityTable abilityTable;
    [HorizontalGroup("AbilityTable"), ShowInInspector, HideLabel, ShowIf("@isChangeName")]
    [InlineButton("OnChangeName", "변경 적용")]
    [InlineButton("NoChangeName", "변경 취소")]
    public string changeName { get; set; }
    public bool isChangeName { get; set; } = false;

    [HorizontalGroup("AbilityTable", width: 300), ShowInInspector, HideLabel, InlineButton("SetDefaultAbility")]
    public AbilitySetupFilter abilitySetupFilter { get => abilityTable.abilitySetupFilter; set => abilityTable.abilitySetupFilter = value; }


    [ValueDropdown("SelectAbilityValueDropdown"), PropertyOrder(0)]
    public int selectType = -1;

    [SerializeField, BoxGroup("AbilityView"), ShowIf("IsSelectAll"), HideLabel, PropertyOrder(1)]
    private AbilityMainView abilityMainView;

    [SerializeField, HideIf("@true")]
    private List<AbilitySelectView> abilitySelectViewList = new List<AbilitySelectView>();
    [ShowInInspector, BoxGroup("AbilityView"), ShowIf("IsSelectType"), HideLabel, EnableGUI, PropertyOrder(1)]
    private AbilitySelectView abilitySelectView
    {
        get
        {
            if (_oldASelectView == selectType && _abilitySelectView != null) return _abilitySelectView;
            _oldASelectView = selectType;

            int findIndex = abilitySelectViewList.FindIndex(i => i.abilityType == SelectType);
            if (findIndex < 0)
            {
                findIndex = abilitySelectViewList.Count;
                abilitySelectViewList.Add(new AbilitySelectView(this, SelectType));
            }
            _abilitySelectView = abilitySelectViewList[findIndex];
            return _abilitySelectView;
        }
        set
        {
            _oldASelectView = selectType;
            _abilitySelectView = value;
        }
    }
    private AbilitySelectView _abilitySelectView;
    private int _oldASelectView;

    public AbilityType SelectType => (AbilityType)selectType;
    private bool IsSelectType => CreateAbilityTableEditor.GetAllAbilityType(abilitySetupFilter).Contains(SelectType);
    private bool IsSelectAll => !IsSelectType;
}
public partial class AbilityTableEditor
{
    internal void OnSelect(CreateAbilityTableEditor config)
    {
        this.config = config;
        isChangeName = false;
        _abilitySelectView = null;
        SetupAbilityListUsingFilter();
        OnValidate();
        AssetDatabase.SaveAssets();

        void SetupAbilityListUsingFilter()
        {
            if (abilitySelectViewList == null)
                abilitySelectViewList = new List<AbilitySelectView>();

            var hasEnums = CreateAbilityTableEditor.GetAllAbilityType(abilitySetupFilter);
            int length = hasEnums.Count;
            for (int i = 0; i < length; i++)
            {
                var hasEnum = hasEnums[i];

                int findIndex = abilitySelectViewList.FindIndex(i => i.abilityType == hasEnum);
                if (findIndex < 0)
                {
                    findIndex = abilitySelectViewList.Count;
                    abilitySelectViewList.Add(new AbilitySelectView(this, hasEnum));
                }
            }
        }
    }

    internal void OnValidate()
    {
        if (abilityMainView == null)
            abilityMainView = new AbilityMainView();
        abilityMainView.editor = this;

        if (abilitySelectViewList == null)
            abilitySelectViewList = new List<AbilitySelectView>();

        int length = abilitySelectViewList.Count;
        for (int i = 0; i < length; i++)
        {
            abilitySelectViewList[i].editor = this;
        }

        for (int i = 0; i < length; i++)
        {
            abilitySelectViewList[i].OnValidate();
        }

        abilityMainView.OnValidate();

        EditorUtility.SetDirty(this);
        EditorUtility.SetDirty(abilityTable);
    }
}
public partial class AbilityTableEditor : ScriptableObject
{

    [Serializable, InlineProperty, HideReferenceObjectPicker]
    public class AbilityMainView
    {
        public AbilityTableEditor editor { get; set; }

        [TitleGroup("Level", order: 1)]
        [SerializeField, HorizontalGroup("Level/H"), LabelText("Select Level"), LabelWidth(100), PropertyRange(1, "@MaxLevel")]
        private int selectLevel = 1;
        [SerializeField, HorizontalGroup("Level/H", width: 15), HideLabel]
        private bool usingCustomLevel;
        [SerializeField, HorizontalGroup("Level/H", 200), LabelWidth(30), LabelText("Max"), ShowIf("@usingCustomLevel")]
        [InlineButton("SetDefaultData", "Default"), InlineButton("OnLoadData", "Load"), InlineButton("OnSaveData", "Save")]
        private int customMaximumLevel = 1;
        public int SelectLevel { get => selectLevel < 1 ? 1 : selectLevel; set => selectLevel = value; }
        [ShowInInspector, HorizontalGroup("Level/H", 200), LabelWidth(30), LabelText("Max"), DisplayAsString, EnableGUI, HideIf("@usingCustomLevel")]
        [InlineButton("SetDefaultData", "Default"), InlineButton("OnLoadData", "Load"), InlineButton("OnSaveData", "Save")]
        public int MaxLevel
        {
            get
            {
                var level = (!usingCustomLevel || customMaximumLevel <= 0) ? editor.config.MaxLevel : (int)customMaximumLevel;
                if (level > limutLevel) level = limutLevel;
                return level;
            }
        }
        [TitleGroup("Ability", order: 3)]
        [ListDrawerSettings(ShowFoldout = false, HideAddButton = true, HideRemoveButton = true, DraggableItems = false, ShowPaging = false, ShowIndexLabels = false), ShowInInspector]
        public List<AbilityBar> abilityBarList
        {
            get
            {
                return GetAbilityBarWithLevel(SelectLevel);
            }
        }

        internal void OnValidate()
        {

        }
        [TitleGroup("Data", order: 3)]
        public void OnLoadData()
        {
            editor.abilitySelectViewList.ForEach(f =>
            {
                f.OnLoadData();
            });
        }
        public void OnSaveData()
        {
            int length = MaxLevel;
            List<List<AbilityValue>> allLvelList = new List<List<AbilityValue>>(length);
            for (int i = 0; i < length; i++)
            {
                var levelList = new List<AbilityValue>();
                levelList = GetAbilityValueWithLevel(i);
                allLvelList.Add(levelList);
            }

            editor.abilityTable.SetAbilityValueWithLevelList(allLvelList);
        }
        public void SetDefaultData()
        {
            var defaultData = CreateAbilityTableEditor.GetAbilitySetupType(editor.abilitySetupFilter);
            editor.abilitySelectViewList.ForEach(f =>
            {
                int index = defaultData.FindIndex(a => a.abilityType == f.abilityType);
                if (index >= 0)
                {
                    f.startValue = defaultData[index].abilityValue;
                }
            });
        }
        public List<AbilityBar> GetAbilityBarWithLevel(int level)
        {
            var hasEnums = CreateAbilityTableEditor.GetAllAbilityType(editor.abilitySetupFilter);
            List<AbilityBar> abilityBarList = new List<AbilityBar>();
            editor.abilitySelectViewList.ForEach(f =>
            {
                if (!hasEnums.Contains(f.abilityType)) return;
                var abilityBar = f.GetAbilityBarWithLevel(level);
                abilityBar.isShowMain = true;
                abilityBar.editor = editor;
                abilityBarList.Add(abilityBar);
            });
            return abilityBarList;
        }
        public List<AbilityValue> GetAbilityValueWithLevel(int level)
        {
            var hasEnums = CreateAbilityTableEditor.GetAllAbilityType(editor.abilitySetupFilter);
            List<AbilityValue> abilityValueList = new List<AbilityValue>();
            editor.abilitySelectViewList.ForEach(f =>
            {
                if (!hasEnums.Contains(f.abilityType)) return;
                var abilityValue = f.GetAbilityValueWithLevel(level);
                abilityValueList.Add(abilityValue);
            });
            return abilityValueList;
        }
    }
    [Serializable, InlineProperty, HideReferenceObjectPicker]
    public class AbilitySelectView
    {
        [HideIf("@true")]
        public AbilityType abilityType;
        [InlineButton("SetDefaultData", "Default")]
        [InlineButton("OnLoadData_Repaint", "Load")]
        public float startValue;
        public int customMaximumLevel;
        public bool showOutRangeLevelPreview;
        public AbilityOutRangeLevelComputeLogic outRangeLevelComputeLogic;

        [ListDrawerSettings(ShowFoldout = false, HideAddButton = true, HideRemoveButton = true, DraggableItems = false, ShowPaging = false, ShowIndexLabels = false)]
        public List<AbilityBar> abilityBarList = new List<AbilityBar>();

        [ListDrawerSettings(ShowFoldout = false, HideAddButton = true, HideRemoveButton = true, DraggableItems = false, ShowPaging = false, ShowIndexLabels = false)]
        [ShowInInspector, ReadOnly, ShowIf("@showOutRangeLevelPreview")]
        private List<AbilityBar> outRangeLevelBarList = new List<AbilityBar>();

        public AbilityTableEditor editor { get; set; }
        public int MaxLevel
        {
            get
            {
                var level = (customMaximumLevel <= 0) ? editor.abilityMainView.MaxLevel : (int)customMaximumLevel;
                if (level > limutLevel) level = limutLevel;
                return level;
            }
        }
        public AbilitySelectView(AbilityTableEditor editor, AbilityType selectType)
        {
            this.editor = editor;
            this.abilityType = selectType;
            abilityBarList = null;
        }

        private void OnLoadData_Repaint()
        {
            OnLoadData();
            editor._abilitySelectView = null;
        }
        public void OnLoadData()
        {
            if (editor == null) return;
            abilityBarList = new List<AbilityBar>();

            List<StepValue> stepValueList = GetStepValieList();

            int length = MaxLevel;
            for (int i = 0; i < length; i++)
            {
                int level = i + 1;
                var stepIndex = stepValueList.FindIndex(f => f.level == level);
                AbilityBar abilityBar = new AbilityBar(false)
                {
                    level = level,
                    editor = editor,
                    abilityType = abilityType,
                    nextStepValue = stepIndex < 0 ? float.NaN : stepValueList[stepIndex].nextStepValue,
                };
                abilityBarList.Add(abilityBar);
            }

            startValue = GetStartValueInTable();
        }

        public void SetDefaultData()
        {
            startValue = CreateAbilityTableEditor.GetAbilitySetupType(editor.abilitySetupFilter, abilityType);
        }
        public List<StepValue> GetStepValieList()
        {
            List<StepValue> stepValueList = new List<StepValue>();
            List<(int, float)> listValue = new List<(int, float)>();

            int length = editor.abilityTable.MaxLevel;
            for (int i = 0; i < length; i++)
            {
                AbilityValue value = editor.abilityTable.GetAbilityValueWithLevel(abilityType, i + 1);
                listValue.Add((i, value.value));
            }
            float lastDelta = 0;
            for (int i = 0; i < length; i++)
            {
                float valueA = CreateAbilityTableEditor.RoundToDigits(listValue[i + 0].Item2, 2);
                if (i + 1 >= length)
                {
                    stepValueList.Add(new StepValue(i, valueA, float.NaN));
                }
                else
                {

                    float valueB = CreateAbilityTableEditor.RoundToDigits(listValue[i + 1].Item2, 2);

                    float stepAtoB = valueB - valueA;
                    float stopDelta = Mathf.Abs(lastDelta - stepAtoB);
                    lastDelta = stopDelta;
                    if (stopDelta >= 0.001f)
                    {
                        stepValueList.Add(new StepValue(i, valueA, stepAtoB));
                    }
                }
            }
            return stepValueList;
        }
        public float GetStartValueInTable()
        {
            return editor.abilityTable.GetAbilityValueWithLevel(abilityType, 1).value;
        }
        public void OnValidate()
        {
            if (editor == null) return;
            if (abilityBarList == null)
            {
                OnLoadData();
            }
            int length = MaxLevel;
            if (abilityBarList.Count > length)
            {
                abilityBarList.RemoveRange(length, abilityBarList.Count - length);
            }
            else if (abilityBarList.Count < length)
            {
                List<StepValue> tempStepValueList = GetStepValieList();
                startValue = GetStartValueInTable();
                int addCount = length - abilityBarList.Count;
                for (int i = 0; i < addCount; i++)
                {
                    int level = abilityBarList.Count + 1;
                    var stepIndex = tempStepValueList.FindIndex(f => f.level == level);

                    var addAbilityBar = new AbilityBar(false)
                    {
                        level = level,
                        editor = editor,
                        abilityType = abilityType,
                        nextStepValue = stepIndex < 0 ? float.NaN : tempStepValueList[stepIndex].nextStepValue,
                    };
                    abilityBarList.Add(addAbilityBar);
                }
            }

            float currentValue = startValue;
            float stepValue = 0f;
            float minValue = currentValue;
            float maxValue = currentValue;
            //length = editor.abilityMainView.MaxLevel;

            for (int i = 0; i < length; i++)
            {
                currentValue = currentValue + stepValue;
                var abilityBar = abilityBarList[i];
                {
                    abilityBar.abilityType = abilityType;
                    abilityBar.value = currentValue;
                    abilityBar.editor = editor;
                }
                abilityBarList[i] = abilityBar;
                if (minValue > currentValue) minValue = currentValue;
                else if (maxValue < currentValue) maxValue = currentValue;

                if (!float.IsNaN(abilityBar.nextStepValue))
                {
                    stepValue = abilityBar.nextStepValue;
                }
            }

            int _outRangeLevelPreviewCount = length;
            if (_outRangeLevelPreviewCount * 2 < editor.abilityMainView.MaxLevel)
            {
                _outRangeLevelPreviewCount = editor.abilityMainView.MaxLevel - length;
            }
            if (!showOutRangeLevelPreview)
            {
                outRangeLevelBarList = null;
                _outRangeLevelPreviewCount = 0;
            }
            else
            {
                if (outRangeLevelBarList == null)
                    outRangeLevelBarList = new List<AbilityBar>();
                else
                    outRangeLevelBarList.Clear();

                for (int i = length; i < length + _outRangeLevelPreviewCount; i++)
                {
                    var previewBar = GetAbilityBarWithLevel(i + 1);
                    outRangeLevelBarList.Add(previewBar);
                    float _currentValue = previewBar.value;
                    if (minValue > _currentValue) minValue = _currentValue;
                    else if (maxValue < _currentValue) maxValue = _currentValue;
                }
            }

            minValue = CreateAbilityTableEditor.GetMinAbilityValue(abilityType, minValue);
            maxValue = CreateAbilityTableEditor.GetMaxAbilityValue(abilityType, maxValue);

            for (int i = 0; i < length; i++)
            {
                var abilityBar = abilityBarList[i];
                {
                    abilityBar.min = minValue;
                    abilityBar.max = maxValue;
                }
                abilityBarList[i] = abilityBar;
            }

            for (int i = 0; i < _outRangeLevelPreviewCount; i++)
            {
                var previewBar = outRangeLevelBarList[i];
                {
                    previewBar.min = minValue;
                    previewBar.max = maxValue;
                }
                outRangeLevelBarList[i] = previewBar;
            }
        }
        public AbilityBar GetAbilityBarWithLevel(int level)
        {
            if (abilityBarList == null || abilityBarList.Count == 0) return default;
            int abilityBarListCount = abilityBarList.Count;
            int selectIndex = level - 1;
            if (selectIndex < 0) selectIndex = 0;
            else if (selectIndex < abilityBarListCount)
            {
                return abilityBarList[selectIndex];
            }
            var lastBar = abilityBarList[^1];
            lastBar.level = level;
            if (outRangeLevelComputeLogic == AbilityOutRangeLevelComputeLogic.Last)
            {
                int outStepCount = (selectIndex - abilityBarListCount) + 1;
                float stepValue = abilityBarList.Select(f => f.nextStepValue).LastOrDefault(f => !float.IsNaN(f));
                float lastvalue = lastBar.value;
                lastBar.value = lastvalue + (stepValue * outStepCount);
            }
            else if (outRangeLevelComputeLogic == AbilityOutRangeLevelComputeLogic.Loop)
            {
                float lastvalue = lastBar.value;
                float stepValue = abilityBarList.Select(f => f.nextStepValue).LastOrDefault(f => !float.IsNaN(f));
                float loopNext = lastBar.nextStepValue;
                int outStepCount = (selectIndex - abilityBarListCount) + 1;

                for (int i = 0; i < outStepCount; i++)
                {
                    lastvalue += stepValue;
                    int outStepIndex = i % abilityBarListCount;
                    var loopBar = abilityBarList[outStepIndex];
                    loopNext = loopBar.nextStepValue;
                    if (!float.IsNaN(loopNext))
                    {
                        stepValue = loopNext;
                    }
                }
                lastBar.value = lastvalue;
                lastBar.nextStepValue = loopNext;
            }
            return lastBar;
        }
        public AbilityValue GetAbilityValueWithLevel(int level)
        {
            AbilityBar abilityBar = GetAbilityBarWithLevel(level);
            return new AbilityValue(abilityBar.abilityType, abilityBar.value);
        }
    }
    [Serializable]
    public struct AbilityBar
    {
        [HideIf("@true")]
        public bool isShowMain;

        public AbilityTableEditor editor { get; set; }

        [HorizontalGroup(width: 200), ShowIf("@isShowMain"), HideLabel, DisplayAsString, EnableGUI]
        [InlineButton("GoToAbilityView", ">>")]
        public AbilityType abilityType;
        [HideIf("@true")]
        public int level;
        [HideIf("@true")]
        public float min;
        [HideIf("@true")]
        public float value;
        [HideIf("@true")]
        public float max;
        [HideIf("@true")]
        public float nextStepValue;

        public AbilityBar(bool isShowMain) : this()
        {
            this.isShowMain = isShowMain;
        }

        [HorizontalGroup(width: 50), ShowInInspector, HideLabel, ReadOnly, DisplayAsString, EnableGUI, HideIf("@isShowMain")]
        [InlineButton("GoToMainView", "<<")]
        private int Level => level;
        private float Min => min;
        [HorizontalGroup, ShowInInspector, HideLabel, ProgressBar("Min", "Max")]
        private float ValueInBar
        {
            get
            {
                int decimalPlaces = CreateAbilityTableEditor.IsFloatAbilityType(abilityType) ? 1 : 0;
                return CreateAbilityTableEditor.FloorToDigits(value, decimalPlaces);
            }
        }
        private float Max => max;
        [HorizontalGroup(width: 50), ShowInInspector, HideLabel, ReadOnly, EnableGUI, DisplayAsString]
        private float Value => CreateAbilityTableEditor.FloorToDigits(value, 2);

        [HorizontalGroup(width: 60), ShowInInspector, HideLabel, HideIf("@isShowMain")]
        [GUIColor("GetStepColor")]
        private float NextStepValue { get => nextStepValue; set => nextStepValue = value; }

        private void GoToMainView()
        {
            if (editor == null) return;
            editor.selectType = -1;
            editor.abilityMainView.SelectLevel = level;
        }
        private void GoToAbilityView()
        {
            if (editor == null) return;
            editor.selectType = (int)abilityType;
        }

        private Color GetStepColor()
        {
            if (float.IsNaN(nextStepValue)) return Color.gray;
            return Color.white;
        }
    }
    [Serializable]
    public struct ExpBar
    {
        public int level;
        [ProgressBar(0, "@max")]
        public float value;
        [HideIf("@true")]
        public float max;
    }
    public struct StepValue
    {
        public int level;
        public float value;
        public float nextStepValue;

        public StepValue(int level, float value, float stepValue) : this()
        {
            this.level = level;
            this.value = value;
            this.nextStepValue = stepValue;
        }
    }

}

public partial class AbilityTableEditor //Editor Func
{
    private void StartChangeName()
    {
        if (abilityTable == null) return;
        changeName = abilityTable.name;
        isChangeName = true;
    }
    private void OnChangeName()
    {
        if (abilityTable == null) return;
        if (string.IsNullOrWhiteSpace(changeName)) return;
        if (changeName == abilityTable.name) return;

        string path = AssetDatabase.GetAssetPath(abilityTable);
        string log = AssetDatabase.RenameAsset(path, changeName);
        if (!string.IsNullOrWhiteSpace(log))
        {
            Debug.LogError(log);
            return;
        }
        this.name = abilityTable.name;


        changeName = "";
        isChangeName = false;

        EditorUtility.SetDirty(abilityTable);
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    private void NoChangeName()
    {
        changeName = "";
        isChangeName = false;
    }


    ValueDropdownList<int> SelectAbilityValueDropdown()
    {
        ValueDropdownList<int> list = new ValueDropdownList<int>();

        var allEnums = CreateAbilityTableEditor.GetAllAbilityType();
        var hasEnums = CreateAbilityTableEditor.GetAllAbilityType(abilitySetupFilter);
        list.Add("==== Main ====", (-1));
        foreach (AbilityType item in allEnums)
        {
            if (hasEnums.Contains(item))
            {
                list.Add(item.ToString(), (int)item);
            }
        }

        return list;
    }

    void SetDefaultAbility(AbilitySetupFilter value)
    {
        List<AbilityValue> abilityValues = CreateAbilityTableEditor.GetAbilitySetupType(value);

        int length = abilityValues.Count;
        for (int i = 0; i < length; i++)
        {
            var abilityValue = abilityValues[i];
            int length2 = abilitySelectViewList.Count;
            for (int j = 0; j < length2; j++)
            {
                var abilitySelectView = abilitySelectViewList[j];
                if (abilitySelectView.abilityType == abilityValue.type)
                {
                    abilitySelectView.startValue = abilityValue.value;
                    break;
                }
            }
        }
        EditorUtility.SetDirty(this);
        EditorUtility.SetDirty(abilityTable);
    }
}
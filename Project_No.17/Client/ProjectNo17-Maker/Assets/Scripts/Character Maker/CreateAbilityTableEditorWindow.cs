using System.IO;
using System.Linq;

using BC.OdccBase;

using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;

using UnityEditor;

using UnityEngine;

public class CreateAbilityTableEditorWindow : OdinMenuEditorWindow
{
    private const string ConfigAssetPath = "Assets/Editor/CreateAbilityTableEditor.asset";
    private const string DefaultAbilityTablePath = "Assets/Main/Resources/AbilityTable";

    [SerializeField]
    private CreateAbilityTableEditor editorTable;

    private AbilityTableEditor lastSelectedEditor;

    [SerializeField]
    private CharacterMaker characterMaker;

    [MenuItem("Tools/Create AbilityStructTable Window")]
    public static void ShowWindow(CharacterMaker characterMaker)
    {
        var window = GetWindow<CreateAbilityTableEditorWindow>();
        window.titleContent = new GUIContent("AbilityStructTable Window");
        window.minSize = new Vector2(800, 600);
        window.characterMaker = characterMaker;
        window.TryLoadOrCreateEditorTable();
        window.lastSelectedEditor = null;
    }

    private void TryLoadOrCreateEditorTable()
    {
        editorTable = AssetDatabase.LoadAssetAtPath<CreateAbilityTableEditor>(ConfigAssetPath);

        if (editorTable == null)
        {
            if (!Directory.Exists("Assets/Editor"))
            {
                Directory.CreateDirectory("Assets/Editor");
            }

            editorTable = ScriptableObject.CreateInstance<CreateAbilityTableEditor>();
            AssetDatabase.CreateAsset(editorTable, ConfigAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("CreateAbilityTableEditorConfig 생성 완료.");
        }

        editorTable.editorWindow = this;
    }


    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree(supportsMultiSelect: false);
        tree.Add("Home", this);

        if (editorTable != null)
        {
            tree.Add($"AbilityTables", editorTable);
            editorTable.BuildMenuTree(tree);
        }

        return tree;
    }
    public void RebuildMenuTree()
    {
        ForceMenuTreeRebuild();
    }
    public void RepaintMenuItem()
    {
        OnImGUI();
    }
    protected override void OnBeginDrawEditors()
    {
        SirenixEditorGUI.BeginHorizontalToolbar();

        AbilityTableEditor selectedEditor = null;
        if (this.MenuTree.Selection.Count > 0)
        {
            var selected = this.MenuTree.Selection.First().Value;
            if (selected is AbilityTableEditor editor)
            {
                selectedEditor = editor;
            }
        }
        if (SirenixEditorGUI.ToolbarButton("새로고침"))
        {
            ForceMenuTreeRebuild();
        }
        if (selectedEditor != null)
        {
            GUILayout.Label($"선택 됨: {selectedEditor.name}", EditorStyles.boldLabel);
            selectedEditor.config = editorTable;

            if (lastSelectedEditor != selectedEditor)
            {
                lastSelectedEditor = selectedEditor;
                lastSelectedEditor.OnSelect(editorTable);
            }

            if (SirenixEditorGUI.ToolbarButton("복제"))
            {
                DuplicateSelected();
            }

            if (SirenixEditorGUI.ToolbarButton("삭제"))
            {
                DeleteSelected();
            }

            if (SirenixEditorGUI.ToolbarButton("적용"))
            {
                ApplySelected();
            }
        }
        else
        {
            if (lastSelectedEditor != null)
            {
                lastSelectedEditor = null;
                AssetDatabase.SaveAssets();
            }

            if (SirenixEditorGUI.ToolbarButton("새로 만들기"))
            {
                CreateNewAbilityTable();
            }
            else if (SirenixEditorGUI.ToolbarButton("기존 항목 가져오기"))
            {
                LoadAbilityTable();
            }
        }

        SirenixEditorGUI.EndHorizontalToolbar();
    }


    private void CreateNewAbilityTable()
    {
        var abilityTable = ScriptableObject.CreateInstance<AbilityTable>();
        string path = EditorUtility.SaveFilePanelInProject("Save AbilityTable", "NewAbilityTable", "asset", "AbilityTable을 저장할 위치를 선택하세요.", DefaultAbilityTablePath);
        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(abilityTable, path);
            editorTable.CreateNewAbilityTableEditor(abilityTable);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ForceMenuTreeRebuild();
        }
    }
    private void LoadAbilityTable()
    {
        var abilityTable = ScriptableObject.CreateInstance<AbilityTable>();
        string path = EditorUtility.OpenFilePanel("AbilityTable 불러오기", DefaultAbilityTablePath, "asset");
        if (!string.IsNullOrEmpty(path))
        {
            string relativePath = "Assets" + path.Substring(Application.dataPath.Length);
            // 에셋 로드
            AbilityTable table = AssetDatabase.LoadAssetAtPath<AbilityTable>(relativePath);
            editorTable.LoadAbilityTableEditor(table);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ForceMenuTreeRebuild();
        }
    }

    private void DuplicateSelected()
    {
        if (lastSelectedEditor?.abilityTable == null) return;

        string originalPath = AssetDatabase.GetAssetPath(lastSelectedEditor.abilityTable);
        string duplicatedPath = AssetDatabase.GenerateUniqueAssetPath(originalPath);

        if (AssetDatabase.CopyAsset(originalPath, duplicatedPath))
        {
            var duplicatedTable = AssetDatabase.LoadAssetAtPath<AbilityTable>(duplicatedPath);
            editorTable.DuplicateSelectedEditor(duplicatedTable);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        ForceMenuTreeRebuild();
    }

    private void DeleteSelected()
    {
        if (lastSelectedEditor == null) return;

        if (lastSelectedEditor.abilityTable != null)
        {
            string path = AssetDatabase.GetAssetPath(lastSelectedEditor.abilityTable);
            AssetDatabase.DeleteAsset(path);
        }

        editorTable.DeleteSelected(lastSelectedEditor);
        lastSelectedEditor = null;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        ForceMenuTreeRebuild();
    }

    private void ApplySelected()
    {
        if (lastSelectedEditor?.abilityTable == null) return;

        var _characterMaker = characterMaker == null ? FindAnyObjectByType<CharacterMaker>() : characterMaker;
        if (_characterMaker != null)
        {
            _characterMaker.ApplyAbilityTable(lastSelectedEditor.abilityTable);
            Debug.Log($"{lastSelectedEditor.abilityTable.name} 이(가) CharacterMaker에 적용되었습니다.");
        }
        else
        {
            Debug.LogWarning("CharacterMaker가 씬에 존재하지 않습니다.");
        }
    }
}

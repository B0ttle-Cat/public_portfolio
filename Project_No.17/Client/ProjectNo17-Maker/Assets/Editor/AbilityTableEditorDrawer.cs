//using System.Collections.Generic;

//using BC.OdccBase;

//using Sirenix.OdinInspector.Editor;
//using Sirenix.Utilities.Editor;

//using UnityEditor;

//using UnityEngine;


////public class AbilityTableEditorDrawer : OdinValueDrawer<AbilityTableEditor>
//{
//	protected override void DrawPropertyLayout(GUIContent label)
//	{
//		var editor = this.ValueEntry.SmartValue;
//		if(editor == null)
//		{
//			SirenixEditorGUI.ErrorMessageBox("TableEditor is null.");
//			return;
//		}
//		if(editor.abilityTable == null)
//		{
//			SirenixEditorGUI.ErrorMessageBox("AbilityTable is null.");
//			return;
//		}

//		SirenixEditorGUI.InfoMessageBox("AbilityTable Drawer");

//		GUILayout.Space(10);
//		LevelSelectView(editor);
//		DrawTabs(editor);
//		GUILayout.Space(10);

//		// 2. 현재 선택된 탭에 따른 뷰어 보여주기
//		if(editor.selectedTab == null)
//		{
//			DrawMainAbilityViewer(editor);
//		}
//		else
//		{
//			DrawSingleAbilityViewer(editor, editor.selectedTab.Value);
//		}

//	}
//	private void LevelSelectView(AbilityTableEditor editor)
//	{
//		int maxLevel = editor.MaxLevel;
//		int selectLevel = editor.selectLevel;
//		GUILayout.BeginHorizontal();
//		Rect rect = EditorGUILayout.GetControlRect();

//		selectLevel = EditorGUI.IntSlider(rect, "레벨 선택", selectLevel, 1, maxLevel);
//		//selectLevel = SirenixEditorFields.RangeIntField(rect, selectLevel, 1, maxLevel);

//		GUILayout.EndHorizontal();
//	}

//	private void DrawTabs(AbilityTableEditor editor)
//	{
//		GUILayout.BeginHorizontal();

//		// "All" 탭
//		DrawSingleTabButton(editor, "All", null);
//		// 능력치별 탭
//		List<AbilityType> hasAbilityTypes = editor.GetHasAbilityTypes();
//		for(int i = 0 ; i < hasAbilityTypes.Count ; i++)
//		{
//			var type = hasAbilityTypes[i];
//			DrawSingleTabButton(editor, type.ToString(), type);
//		}

//		GUILayout.FlexibleSpace();

//		// - 버튼
//		if(GUILayout.Button("-", GUILayout.Width(20)))
//		{
//			ShowAbilityTypeRemoveMenu(editor, hasAbilityTypes);
//		}
//		// + 버튼
//		if(GUILayout.Button("+", GUILayout.Width(30)))
//		{
//			ShowAbilityTypeAddMenu(editor, hasAbilityTypes);
//		}

//		GUILayout.EndHorizontal();
//	}
//	private void DrawSingleTabButton(AbilityTableEditor editor, string label, AbilityType? tabType)
//	{
//		bool isSelected = (editor.selectedTab == tabType);

//		if(SirenixEditorGUI.ToolbarTab(isSelected, label))
//		{
//			editor.selectedTab = tabType;
//		}
//	}

//	private void ShowAbilityTypeRemoveMenu(AbilityTableEditor editor, List<AbilityType> hasAbilityTypes)
//	{
//		GenericMenu menu = new GenericMenu();
//		foreach(var type in hasAbilityTypes)
//		{
//			menu.AddItem(new GUIContent(type.ToString()), false, () => {
//				editor.AddAbilityType(type);
//			});
//		}

//		if(menu.GetItemCount() == 0)
//		{
//			menu.AddDisabledItem(new GUIContent("삭제 가능한 능력치 없음"));
//		}

//		menu.ShowAsContext();
//	}
//	private void ShowAbilityTypeAddMenu(AbilityTableEditor editor, List<AbilityType> hasAbilityTypes)
//	{
//		GenericMenu menu = new GenericMenu();
//		var getAllAbilityType = AbilityTableEditor.GetAllAbilityType();
//		foreach(var type in getAllAbilityType)
//		{
//			if(hasAbilityTypes.Contains(type)) continue;

//			menu.AddItem(new GUIContent(type.ToString()), false, () => {
//				editor.AddAbilityType(type);
//			});
//		}

//		if(menu.GetItemCount() == 0)
//		{
//			menu.AddDisabledItem(new GUIContent("추가 가능한 능력치 없음"));
//		}

//		menu.ShowAsContext();
//	}

//	private void DrawMainAbilityViewer(AbilityTableEditor editor)
//	{
//		SirenixEditorGUI.Title("메인 능력치 뷰어", null, TextAlignment.Left, true);

//		// 여기에 All 탭일 때 전체 능력치 보여주는 내용을 그리면 된다
//		GUILayout.Label("여기에 전체 능력치 테이블 편집 UI를 추가합니다.", EditorStyles.helpBox);
//	}

//	private void DrawSingleAbilityViewer(AbilityTableEditor editor, AbilityType type)
//	{
//		SirenixEditorGUI.Title($"{type} 뷰어", null, TextAlignment.Left, true);

//		// 여기에 단일 능력치별 상세 편집 UI를 그리면 된다
//		GUILayout.Label($"여기에 {type}에 대한 레벨별 수치 편집 UI를 추가합니다.", EditorStyles.helpBox);
//	}

//}
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;

using UnityEditor;

using UnityEngine;

public class GameObjectPreviewDrawer : OdinAttributeDrawer<GameObjectPreviewAttribute, GameObject>
{
	public GameObject worldObj = null;
	private Editor gameObjectEditor = null;
	private GameObject lastObj = null;
	protected override void DrawPropertyLayout(GUIContent label)
	{
		// 애니메이션 클립 선택 필드
		if(label == null) label = new GUIContent();

		this.ValueEntry.SmartValue = (GameObject)EditorGUILayout.ObjectField(label, this.ValueEntry.SmartValue, typeof(GameObject), false);

		// 현재 클립 정보 표시
		if(this.ValueEntry.SmartValue != null)
		{
			GameObject obj = this.ValueEntry.SmartValue;
			if(lastObj != null && lastObj != obj)
			{
				if(gameObjectEditor != null) Object.DestroyImmediate(gameObjectEditor);
				gameObjectEditor = null;
			}
			if(gameObjectEditor == null && obj != null)
			{
				lastObj = obj;
				gameObjectEditor = Editor.CreateEditor(ValueEntry.SmartValue);
				gameObjectEditor.HasPreviewGUI();
				gameObjectEditor.OnPreviewSettings();
			}

			if(gameObjectEditor != null)
			{
				gameObjectEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(0, 200), EditorStyles.whiteLabel);
			}
		}
	}
}
#endif
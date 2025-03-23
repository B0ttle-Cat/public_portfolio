#if UNITY_EDITOR
using System.Reflection;

using Sirenix.OdinInspector.Editor;

using UnityEditor;

using UnityEngine;

public class AnimationPreviewDrawer : OdinAttributeDrawer<AnimationPreviewAttribute, AnimationClip>
{
	private Editor animationEditor = null;
	private AnimationClip lastClip = null;
	protected override void DrawPropertyLayout(GUIContent label)
	{
		// 애니메이션 클립 선택 필드
		if(label == null) label = new GUIContent();

		this.ValueEntry.SmartValue = (AnimationClip)EditorGUILayout.ObjectField(label, this.ValueEntry.SmartValue, typeof(AnimationClip), false);

		// 현재 클립 정보 표시
		if(this.ValueEntry.SmartValue != null)
		{
			AnimationClip clip = this.ValueEntry.SmartValue;
			if(lastClip != null && lastClip != clip)
			{
				if(animationEditor != null) Object.DestroyImmediate(animationEditor);
				animationEditor = null;
			}
			if(animationEditor == null && clip != null)
			{
				lastClip = clip;
				animationEditor = Editor.CreateEditor(ValueEntry.SmartValue);
				animationEditor.OnInspectorGUI();
				animationEditor.OnPreviewSettings();
				FixPreviewEditorForAnimation(animationEditor);
			}

			if(animationEditor != null)
			{
				animationEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(0, 300), EditorStyles.whiteLabel);
			}
		}
	}
	private static FieldInfo _cachedAvatarPreviewFieldInfo;
	private static FieldInfo _cachedTimeControlFieldInfo;
	private static FieldInfo _cachedStopTimeFieldInfo;

	private static void FixPreviewEditorForAnimation(Editor editor)
	{
		if(!(editor.target is AnimationClip clip)) return;
		if(_cachedAvatarPreviewFieldInfo != null && _cachedTimeControlFieldInfo != null && _cachedStopTimeFieldInfo != null)
		{
			var value = _cachedAvatarPreviewFieldInfo.GetValue(editor);
			var subValue = _cachedTimeControlFieldInfo.GetValue(value);
			_cachedStopTimeFieldInfo.SetValue(subValue, clip.length);
		}
		else
		{
			_cachedAvatarPreviewFieldInfo ??= editor.GetType().GetField("m_AvatarPreview", BindingFlags.NonPublic | BindingFlags.Instance);
			if(_cachedAvatarPreviewFieldInfo == null) return;
			var value = _cachedAvatarPreviewFieldInfo.GetValue(editor);
			if(value == null) return;
			_cachedTimeControlFieldInfo ??= value.GetType().GetField("timeControl", BindingFlags.Public | BindingFlags.Instance);
			if(_cachedTimeControlFieldInfo == null) return;
			var subValue = _cachedTimeControlFieldInfo.GetValue(value);
			if(subValue == null) return;
			_cachedStopTimeFieldInfo ??= subValue.GetType().GetField("stopTime", BindingFlags.Public | BindingFlags.Instance);
			if(_cachedStopTimeFieldInfo == null) return;
			_cachedStopTimeFieldInfo.SetValue(subValue, clip.length);
		}

	}
}
#endif
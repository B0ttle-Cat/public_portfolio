//using System.Collections.Generic;

//using Sirenix.OdinInspector.Editor;
//using Sirenix.Utilities.Editor;

//using UnityEditor;

//using UnityEngine;

//public class LayerMaskFilterAttributeDrawer : OdinAttributeDrawer<LayerMaskFilterAttribute, LayerMask>
//{
//	protected override void DrawPropertyLayout(GUIContent label)
//	{
//		// 정확한 API 사용 예시
//		var filterLayerProperty = Property.Parent.FindChild(
//			prop => prop.Name == Attribute.FilterLayerMember.TrimStart('@'),
//			includeSelf: false
//		);

//		if(filterLayerProperty == null)
//		{
//			SirenixEditorGUI.ErrorMessageBox($"필터 LayerMask 필드 '{Attribute.FilterLayerMember}'를 찾을 수 없습니다.");
//			CallNextDrawer(label);
//			return;
//		}

//		var filterLayerValue = filterLayerProperty.ValueEntry.WeakSmartValue;
//		if(filterLayerValue is not LayerMask filterLayer)
//		{
//			SirenixEditorGUI.ErrorMessageBox($"'{Attribute.FilterLayerMember}' 필드는 LayerMask 타입이어야 합니다.");
//			CallNextDrawer(label);
//			return;
//		}

//		// 필터된 Layer 이름 배열 얻기
//		string[] filteredLayerNames = GetFilteredLayerNames(filterLayer);

//		// 현재 선택된 LayerMask를 Popup에 대응하는 형태로 변환
//		int currentSelectedMask = InternalMaskToIndices(ValueEntry.SmartValue, filteredLayerNames);

//		// Popup 필드 그리기
//		int newSelectedMask = EditorGUILayout.MaskField(label, currentSelectedMask, filteredLayerNames);

//		// 선택한 결과를 다시 LayerMask 형태로 저장
//		ValueEntry.SmartValue = IndicesToInternalMask(newSelectedMask, filteredLayerNames);
//	}


//	private string[] GetFilteredLayerNames(LayerMask filterMask)
//	{
//		var layers = new List<string>();
//		for(int i = 0 ; i < 32 ; i++)
//		{
//			if((filterMask.value & (1 << i)) != 0)
//			{
//				string layerName = LayerMask.LayerToName(i);
//				if(!string.IsNullOrEmpty(layerName))
//				{
//					layers.Add(layerName);
//				}
//			}
//		}
//		return layers.ToArray();
//	}

//	private int InternalMaskToIndices(LayerMask mask, string[] filteredLayers)
//	{
//		int result = 0;
//		for(int i = 0 ; i < filteredLayers.Length ; i++)
//		{
//			int layerIdx = LayerMask.NameToLayer(filteredLayers[i]);
//			if((mask.value & (1 << layerIdx)) != 0)
//			{
//				result |= (1 << i);
//			}
//		}
//		return result;
//	}

//	private LayerMask IndicesToInternalMask(int selectedMask, string[] filteredLayers)
//	{
//		int result = 0;
//		for(int i = 0 ; i < filteredLayers.Length ; i++)
//		{
//			if((selectedMask & (1 << i)) != 0)
//			{
//				int layerIdx = LayerMask.NameToLayer(filteredLayers[i]);
//				result |= (1 << layerIdx);
//			}
//		}
//		return result;
//	}
//}

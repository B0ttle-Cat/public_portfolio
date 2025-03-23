using System;

using Sirenix.OdinInspector;

using UnityEngine;

namespace BC.Shape
{
	[Serializable]
	public abstract class BCShape
	{
#if UNITY_EDITOR
		[PropertyOrder(-101), ShowInInspector, DisplayAsString(EnableRichText = true), EnableGUI, HideLabel]
		[InlineButton("PingThisDataScript", " This Script ")]
		private string Editor_Title_Text => $"<b><color=white>Shape({GetType().Name})</color></b>";
		private double Editor_LastClickTime = -1; // 마지막 클릭 시간을 기록
		private const double Editor_ClickInterval = 0.25; // 클릭 간격
		private void PingThisDataScript()
		{
			double currentTime = UnityEditor.EditorApplication.timeSinceStartup;
			if(Editor_LastClickTime > 0 && (currentTime - Editor_LastClickTime) <= Editor_ClickInterval)
			{
				// 파일 열기
				OpenThisDataScript();
			}
			else
			{
				// 현재 컴포넌트의 이름을 기준으로 스크립트 검색
				string scriptName = GetType().Name;
				string scriptFileName = $"{GetType().Name}.cs";

				string[] guids = UnityEditor.AssetDatabase.FindAssets($"{scriptName} t:Script");

				foreach(string guid in guids)
				{
					// 첫 번째 검색 결과를 기준으로 Asset 경로 가져오기
					string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
					if(System.IO.Path.GetFileName(path) == scriptFileName)
					{
						UnityEngine.Object scriptAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);

						// 에디터에서 해당 스크립트를 강조
						UnityEditor.EditorGUIUtility.PingObject(scriptAsset);
						break;
					}
				}
			}
			Editor_LastClickTime = currentTime;
		}
		private void OpenThisDataScript()
		{
			string scriptName = GetType().Name;
			string scriptFileName = $"{scriptName}.cs";

			string[] guids = UnityEditor.AssetDatabase.FindAssets($"{scriptName} t:Script");

			foreach(string guid in guids)
			{
				string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
				if(System.IO.Path.GetFileName(path) == scriptFileName)
				{
					// 에디터에서 해당 파일 열기
					UnityEditor.AssetDatabase.OpenAsset(UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path));
				}
			}
		}
#endif



		protected enum AnchorType
		{
			PivotPos, TargetPos,
			MiddleOfPos,
			MinRange, MaxRange,
			MiddleOfRange,
		}
		protected enum ShapeDraw
		{
			Hide = -1,

			Area,
			Line,
			DashLine,

			AreaAndLine,
			AreaAndDashLine,
		}
		public abstract void DrawShape(Vector3 pivot, Vector3 target, Vector2 minMaxRange, float yAxisRotate);
		public abstract void OverlapCharacter(Vector3 pivot, Vector3 target, Vector2 minMaxRange, float yAxisRotate, Collider[] colliders, float yHeightRange = 50f, int layerMask = -1, QueryTriggerInteraction trigger = QueryTriggerInteraction.UseGlobal);
		public abstract Collider[] OverlapCharacter(Vector3 pivot, Vector3 target, Vector2 minMaxRange, float yAxisRotate, float yHeightRange = 50f, int layerMask = -1, QueryTriggerInteraction trigger = QueryTriggerInteraction.UseGlobal);
		public abstract bool CheckCharacter(Vector3 pivot, Vector3 target, Vector2 minMaxRange, float yAxisRotate, float yHeightRange = 50f, int layerMask = -1, QueryTriggerInteraction trigger = QueryTriggerInteraction.UseGlobal);
		protected Vector3 AnchorPos(AnchorType anchorType, Vector3 pivot, Vector3 target, Vector2 minMaxRange)
		{
			return anchorType switch {
				AnchorType.PivotPos => pivot,
				AnchorType.TargetPos => target,
				AnchorType.MiddleOfPos => Vector3.Lerp(pivot, target, 0.5f),
				AnchorType.MinRange => pivot + (target-pivot).normalized * minMaxRange.x,
				AnchorType.MaxRange => pivot + (target-pivot).normalized * minMaxRange.y,
				AnchorType.MiddleOfRange => pivot + (target-pivot).normalized * Mathf.Lerp(minMaxRange.x, minMaxRange.y, 0.5f),
				_ => pivot,
			};
		}
	}
}

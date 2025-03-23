using System;
using System.Collections.Generic;
using System.Linq;

using BC.Shape;

using Shapes;

using Sirenix.OdinInspector;

using UnityEngine;

namespace BC.AIProvider
{
	public class ActuatorHitShape : ScriptableObject, IShapeRenderer_EditorScriptableObject
	{
#if UNITY_EDITOR
		[TabGroup("Preview","InEditor Preview"), SerializeField] private bool showScriptablePreview;
		[TabGroup("Preview","InEditor Preview"), SerializeField, EnableIf("@showScriptablePreview"), InlineProperty] private PreviewShowOption previewScriptableShowOption;
		[TabGroup("Preview","InEditor Preview"), SerializeField, EnableIf("@showScriptablePreview")] private Vector3 pivotPosition;
		[TabGroup("Preview","InEditor Preview"), SerializeField, EnableIf("@showScriptablePreview")] private Vector3 targetPosition;
		[TabGroup("Preview","InEditor Preview"), SerializeField, EnableIf("@showScriptablePreview")] public float testPivotRadius;

		[TabGroup("Preview","PlayMode Preview"), SerializeField] private bool showPlayModeRangePreview;
		[TabGroup("Preview","PlayMode Preview"), SerializeField, EnableIf("@showPlayModeRangePreview"), InlineProperty] private PreviewShowOption previewPlayModeShowOption;
		[Serializable]
		private struct PreviewShowOption
		{
			[SerializeField,HorizontalGroup("R"), ToggleLeft] public bool showRange;
			[SerializeField,HorizontalGroup("R"), ToggleLeft] public bool showMinMaxRange;
			[SerializeField,HorizontalGroup("S"), ToggleLeft] public bool showShape;
			[SerializeField,HorizontalGroup("S"), ToggleLeft] public bool showTarget;
		}
#endif

		[TitleGroup("AttackRange", subtitle:"최소 <= 적정 <= 최대")]
		[SerializeField, HorizontalGroup("AttackRange/H",60), HideLabel, PropertyOrder(0), SuffixLabel("Min ", true)]
		private float targetingMinRange;
#if UNITY_EDITOR
		[HorizontalGroup("AttackRange/H", 30), HideLabel, PropertyOrder(1), ShowInInspector, DisplayAsString]
		private string Text1 => " <= ";
#endif
		[HorizontalGroup("AttackRange/H"), HideLabel, PropertyOrder(2)]
		[SerializeField, MinMaxSlider("@targetingMinRange", "@targetingMaxRange", true)]
		private Vector2 targetingRange;
#if UNITY_EDITOR
		[HorizontalGroup("AttackRange/H", 30), HideLabel, PropertyOrder(3), ShowInInspector, DisplayAsString]
		private string Text2 => " <= ";
#endif
		[SerializeField, HorizontalGroup("AttackRange/H",60), HideLabel, PropertyOrder(4), SuffixLabel("Max ",true)]
		private float targetingMaxRange;
		[SerializeField, HorizontalGroup("AttackRange/C"), HideLabel, PropertyOrder(5)]
		private Color minRangeColor, rangeColor ,maxRangeColor;
		[SerializeField, PropertyOrder(6)]
		private float rangeThickness = 0.1f;
		public Vector2 Range => targetingRange;
		public float MinRange => targetingMinRange;
		public float MaxRange => targetingMaxRange;

		[TitleGroup("AttackShape"),PropertyOrder(50)]
		public List<HitShape> shapes;
		public bool HasShape => shapes.FindIndex(i => i.isHit) >= 0;
		[Serializable]
		public class HitShape
		{
			[HorizontalGroup,ToggleLeft] public bool isShow;
			[HorizontalGroup,ToggleLeft] public bool isHit;
			[SerializeReference, HideLabel, HideReferenceObjectPicker]
			public BCShape shape;

			public void DrawShape(Vector3 pivotPosition, Vector3 targetPosition, Vector2 minMaxRange, float yAxisRotate)
			{
				if(!isShow || shape == null) return;
				shape.DrawShape(pivotPosition, targetPosition, minMaxRange, yAxisRotate);
			}
			public void OverlapCharacter(Vector3 pivotPosition, Vector3 targetPosition, Vector2 minMaxRange, float yAxisRotate, Collider[] colliders, float yHeightRange = 50f, int layerMask = -1, QueryTriggerInteraction trigger = QueryTriggerInteraction.UseGlobal)
			{
				if(!isHit || shape == null) return;
				shape.OverlapCharacter(pivotPosition, targetPosition, minMaxRange, yAxisRotate, colliders, yHeightRange, layerMask, trigger);
			}
			public Collider[] OverlapCharacter(Vector3 pivotPosition, Vector3 targetPosition, Vector2 minMaxRange, float yAxisRotate, float yHeightRange = 50f, int layerMask = -1, QueryTriggerInteraction trigger = QueryTriggerInteraction.UseGlobal)
			{
				if(!isHit || shape == null) return new Collider[0];
				return shape.OverlapCharacter(pivotPosition, targetPosition, minMaxRange, yAxisRotate, yHeightRange, layerMask, trigger);
			}
			public bool CheckCharacter(Vector3 pivotPosition, Vector3 targetPosition, Vector2 minMaxRange, float yAxisRotate, float yHeightRange = 50f, int layerMask = -1, QueryTriggerInteraction trigger = QueryTriggerInteraction.UseGlobal)
			{
				if(!isHit || shape == null) return false;
				return shape.CheckCharacter(pivotPosition, targetPosition, minMaxRange, yAxisRotate, yHeightRange, layerMask, trigger);
			}
		}

#if UNITY_EDITOR
		void IShapeRenderer_EditorScriptableObject.Editor_OnDrawShapes()
		{
			if(!showScriptablePreview) return;
			if(UnityEditor.EditorApplication.isPlaying) return;
			DrawShape(pivotPosition, targetPosition, testPivotRadius, previewScriptableShowOption.showShape, previewScriptableShowOption.showMinMaxRange, previewScriptableShowOption.showRange, previewScriptableShowOption.showMinMaxRange, previewScriptableShowOption.showTarget);
		}
		public void Editor_ShowPlayModeRangePreview(Vector3 pivotPosition, float pivotRadius)
		{
			if(!showPlayModeRangePreview) return;
			if(!UnityEditor.EditorApplication.isPlaying) return;
			DrawRangeOnly(pivotPosition, pivotPosition, pivotRadius, previewPlayModeShowOption.showRange, previewPlayModeShowOption.showMinMaxRange);
		}
#endif
		public void DrawRangeOnly(Vector3 pivotPosition, Vector3 targetPosition, float pivotRadius, bool drawRange = true, bool drawMinMaxRange = true)
		{
			DrawShape(pivotPosition, targetPosition, pivotRadius, false, drawMinMaxRange, drawRange, drawMinMaxRange, false);
		}
		public void DrawShape(Vector3 pivotPosition, Vector3 targetPosition, float pivotRadius, bool drawShape = true, bool drawMinRange = false, bool drawRange = false, bool drawMaxRange = true, bool drawTargetLine = false)
		{
			targetPosition.y = pivotPosition.y;
			Vector3 targetDirection = targetPosition - pivotPosition;
			float range = targetDirection.magnitude;
			if(range < float.Epsilon) targetDirection = Vector3.forward;
			targetDirection = targetDirection.normalized;
			float MinRange = this.MinRange + pivotRadius;
			float MaxRange = this.MaxRange + pivotRadius;
			if(MinRange > MaxRange)
			{
				float temp = MaxRange;
				MaxRange = MinRange;
				MinRange = temp;
			}

			float NearRange = targetingRange.x + pivotRadius;
			float FarRange = targetingRange.y + pivotRadius;

			float yAxisRotate = Vector3.SignedAngle(Vector3.forward,targetDirection, Vector3.up);
			Vector2 minMaxRange = new Vector2(MinRange,MaxRange);
			targetPosition = pivotPosition + targetDirection * (range < MinRange ? MinRange : range > MaxRange ? MaxRange : range);

			Draw.ResetAllDrawStates();
			Quaternion rot = Quaternion.Euler(-90f,-90f, 0f);
			Draw.UseDashes = true;
			if(drawRange)
			{
				Draw.Ring(pivotPosition, rot, NearRange, rangeThickness, rangeColor);
				Draw.Ring(pivotPosition, rot, FarRange, rangeThickness, rangeColor);
			}

			if(drawTargetLine) Draw.Line(pivotPosition, pivotPosition+targetDirection*range, rangeThickness, range < MinRange ? minRangeColor : range > MaxRange ? maxRangeColor : rangeColor);
			Draw.UseDashes = false;
			if(drawMaxRange) Draw.Ring(pivotPosition, rot, MaxRange, rangeThickness, maxRangeColor);
			if(drawMinRange && MinRange > float.Epsilon) Draw.Ring(pivotPosition, rot, MinRange, rangeThickness, minRangeColor);

			Draw.ResetAllDrawStates();
			if(drawShape)
			{
				int length = shapes.Count;
				for(int i = 0 ; i < length ; i++)
				{
					shapes[i].DrawShape(pivotPosition, targetPosition, minMaxRange, yAxisRotate);
				}
			}
		}
		public void OverlapCharacter(Vector3 pivotPosition, Vector3 targetPosition, float pivotRadius, Collider[] colliders, float yHeightRange = 50f, int layerMask = -1, QueryTriggerInteraction trigger = QueryTriggerInteraction.UseGlobal)
		{
			targetPosition.y = pivotPosition.y;
			Vector3 targetDirection = targetPosition - pivotPosition;
			float range = targetDirection.magnitude;
			if(range < float.Epsilon) targetDirection = Vector3.forward;
			targetDirection = targetDirection.normalized;
			float MinRange = this.MinRange + pivotRadius;
			float MaxRange = this.MaxRange + pivotRadius;
			if(MinRange > MaxRange)
			{
				float temp = MaxRange;
				MaxRange = MinRange;
				MinRange = temp;
			}

			float yAxisRotate = Vector3.SignedAngle(Vector3.forward,targetDirection, Vector3.up);
			Vector2 minMaxRange = new Vector2(MinRange,MaxRange);
			targetPosition = pivotPosition + targetDirection * (range < MinRange ? MinRange : range > MaxRange ? MaxRange : range);

			int limitCount = 25;
			HashSet<Collider> hashColliders = new HashSet<Collider>(10);
			int length = shapes.Count;
			for(int i = 0 ; i < length ; i++)
			{
				var hitShapes = shapes[i];
				if(!hitShapes.isHit || hitShapes.shape == null) continue;

				Collider[] shapesColliders = new Collider[limitCount];
				hitShapes.OverlapCharacter(pivotPosition, targetPosition, minMaxRange, yAxisRotate, yHeightRange, layerMask, trigger);
				if(AddHashSet(hashColliders, shapesColliders, limitCount))
				{
					break;
				}
			}
			HashToResultArray(hashColliders, colliders);
			bool AddHashSet(HashSet<Collider> hashColliders, Collider[] colliders, int limitCount)
			{
				int length = colliders.Length;
				for(int i = 0 ; i < length ; i++)
				{
					if(colliders[i] != null && hashColliders.Add(colliders[i]))
					{
						if(hashColliders.Count == limitCount)
						{
							return true;
						}
					}
				}
				return false;
			}
			void HashToResultArray(HashSet<Collider> hashColliders, Collider[] colliders)
			{
				int index = 0;
				foreach(var item in hashColliders)
				{
					colliders[index++] = item;
				}
			}
		}
		public Collider[] OverlapCharacter(Vector3 pivotPosition, Vector3 targetPosition, float pivotRadius, float yHeightRange = 50f, int layerMask = -1, QueryTriggerInteraction trigger = QueryTriggerInteraction.UseGlobal)
		{
			targetPosition.y = pivotPosition.y;
			Vector3 targetDirection = targetPosition - pivotPosition;
			float range = targetDirection.magnitude;
			if(range < float.Epsilon) targetDirection = Vector3.forward;
			targetDirection = targetDirection.normalized;
			float MinRange = this.MinRange + pivotRadius;
			float MaxRange = this.MaxRange + pivotRadius;
			if(MinRange > MaxRange)
			{
				float temp = MaxRange;
				MaxRange = MinRange;
				MinRange = temp;
			}

			float yAxisRotate = Vector3.SignedAngle(Vector3.forward,targetDirection, Vector3.up);
			Vector2 minMaxRange = new Vector2(MinRange,MaxRange);
			targetPosition = pivotPosition + targetDirection * (range < MinRange ? MinRange : range > MaxRange ? MaxRange : range);

			int limitCount = 25;
			HashSet<Collider> hashColliders = new HashSet<Collider>(10);
			int length = shapes.Count;
			for(int i = 0 ; i < length ; i++)
			{
				var hitShapes = shapes[i];
				if(!hitShapes.isHit || hitShapes.shape == null) continue;

				Collider[] shapesColliders = hitShapes.OverlapCharacter(pivotPosition, targetPosition, minMaxRange, yAxisRotate, yHeightRange, layerMask, trigger);
				if(AddHashSet(hashColliders, shapesColliders, limitCount))
				{
					break;
				}
			}
			return HashToResultArray(hashColliders);
			bool AddHashSet(HashSet<Collider> hashColliders, Collider[] colliders, int limitCount)
			{
				int length = colliders.Length;
				for(int i = 0 ; i < length ; i++)
				{
					if(colliders[i] != null && hashColliders.Add(colliders[i]))
					{
						if(hashColliders.Count == limitCount)
						{
							return true;
						}
					}
				}
				return false;
			}
			Collider[] HashToResultArray(HashSet<Collider> hashColliders)
			{
				return hashColliders.ToArray();
			}
		}
		public bool CheckCharacter(Vector3 pivotPosition, Vector3 targetPosition, float pivotRadius, float yHeightRange = 50f, int layerMask = -1, QueryTriggerInteraction trigger = QueryTriggerInteraction.UseGlobal)
		{
			targetPosition.y = pivotPosition.y;
			Vector3 targetDirection = targetPosition - pivotPosition;
			float range = targetDirection.magnitude;
			if(range < float.Epsilon) targetDirection = Vector3.forward;
			targetDirection = targetDirection.normalized;
			float MinRange = this.MinRange + pivotRadius;
			float MaxRange = this.MaxRange + pivotRadius;
			if(MinRange > MaxRange)
			{
				float temp = MaxRange;
				MaxRange = MinRange;
				MinRange = temp;
			}

			float yAxisRotate = Vector3.SignedAngle(Vector3.forward,targetDirection, Vector3.up);
			Vector2 minMaxRange = new Vector2(MinRange,MaxRange);
			targetPosition = pivotPosition + targetDirection * (range < MinRange ? MinRange : range > MaxRange ? MaxRange : range);

			int length = shapes.Count;
			for(int i = 0 ; i < length ; i++)
			{
				var hitShapes = shapes[i];
				if(hitShapes.CheckCharacter(pivotPosition, targetPosition, minMaxRange, yAxisRotate, yHeightRange, layerMask, trigger))
				{
					return true;
				}
			}
			return false;
		}
	}
}

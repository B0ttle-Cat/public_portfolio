using System;
using System.Collections.Generic;

using BC.Shape;

using Shapes;

using Sirenix.OdinInspector;

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;

using UnityEngine;

using Collider = UnityEngine.Collider;

namespace BC.AISensor
{
	public class SphereCollector : SensorCollector
	{
		[Serializable]
		public struct SphereData
		{
			public SphereShape Sphere;
			public LayerMask includeLayerMask;
		}
		public int hitCountMax = 50;

		public SphereData[] sphereDatas = new SphereData[0];

		[SerializeField, ToggleGroup("isDraw")]
		private bool isDraw;
		[SerializeField, ToggleGroup("isDraw")]
		private bool isDrawOnlyEditor;
		[SerializeField, ToggleGroup("isDraw")]
		private float drawYOffset;
		[SerializeField, ToggleGroup("isDraw")]
		private bool drawIsRing;
		[SerializeField, ToggleGroup("isDraw"), ShowIf("@drawIsRing")]
		private float ringInsideThickness;
		[SerializeField, ToggleGroup("isDraw"), ShowIf("@drawIsRing")]
		public ThicknessSpace thicknessSpace;
		[SerializeField, ToggleGroup("isDraw")]
		private IShapeRenderer.DrawStyle drawStyle;

		protected override void Init(SensorProvider self)
		{

		}

		protected override void SensorUpdate(SensorProvider self, ref HashSet<Collider> collectorList)
		{
			int dataLength = sphereDatas.Length;
			if(dataLength == 0) return;
			if(hitCountMax <= 0) return;
			NativeArray<OverlapSphereCommand> commands = new NativeArray<OverlapSphereCommand>(dataLength, Allocator.TempJob);
			NativeArray<ColliderHit> colliderHit = new NativeArray<ColliderHit>(hitCountMax, Allocator.TempJob);
			var sphereArray = new NativeArray<SphereData>(sphereDatas, Allocator.TempJob);
			for(int i = 0 ; i < dataLength ; i++)
			{
				var data = sphereArray[i];
				var sphere = data.Sphere;
				sphere.Center = self.transform.TransformPoint(sphere.Center);
				data.Sphere = sphere;
				sphereArray[i] = data;
			}


			var createCommandJob = new CreateOverlapSphereCommand(){
				commands = commands,
				sphereDatas = sphereArray,
			};
			var createCommandJobSchedule = createCommandJob.Schedule(dataLength, 10,default(JobHandle));

			var jobHandle = OverlapSphereCommand.ScheduleBatch(commands, colliderHit, 10, hitCountMax, createCommandJobSchedule);
			jobHandle.Complete();

			int length = colliderHit.Length;
			for(int i = 0 ; i < length ; i++)
			{
				Collider hitCollider = colliderHit[i].collider;
				if(hitCollider != null)
				{
					if(collectorList.Add(hitCollider))
					{
						//SensorManager.Sensor.AddDrawGizmos(() => {
						//	Gizmos.color = Color.red;
						//	if(hitCollider != null) Gizmos.DrawWireCube(hitCollider.bounds.center, hitCollider.bounds.size);
						//});
					}
				}
			}

			sphereArray.Dispose();
			colliderHit.Dispose();
			commands.Dispose();
		}

		protected override void Release(SensorProvider self)
		{

		}

		struct CreateOverlapSphereCommand : IJobParallelFor
		{

			[WriteOnly]
			public NativeArray<OverlapSphereCommand> commands;
			[Unity.Collections.ReadOnly]
			public NativeArray<SphereData> sphereDatas;
			public void Execute(int index)
			{
				SphereData data = sphereDatas[index];
				SphereShape sphere = data.Sphere;
				QueryParameters queryParameters = new QueryParameters(data.includeLayerMask, false, QueryTriggerInteraction.UseGlobal, false);
				float3 center = sphere.Center;
				float radius = sphere.Radius;

				commands[index] = new OverlapSphereCommand(center, radius, queryParameters);
			}
		}

		public override void OnDraw(Camera cam)
		{
			if(!isDraw) return;
#if !UNITY_EDITOR
			if(isDrawOnlyEditor)
			{
				return;
			}
#endif

			drawStyle.SetDrawStyle();
			Draw.ThicknessSpace = thicknessSpace;

			int length = sphereDatas.Length;
			for(int i = 0 ; i < length ; i++)
			{
				var shape = sphereDatas[i];
				if(drawIsRing)
				{
					shape.Sphere.OnDrawRing(ringInsideThickness, offsetY: drawYOffset);
				}
				else
				{
					shape.Sphere.OnDrawShapes(offsetY: drawYOffset);
				}
			}
		}
	}
}

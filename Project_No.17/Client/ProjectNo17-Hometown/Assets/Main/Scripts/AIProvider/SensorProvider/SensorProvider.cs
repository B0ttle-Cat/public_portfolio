//using BC.ODCC;

//namespace BC.AIProvider
//{
//	public class SensorProvider : ComponentBehaviour//, IShapeRenderer
//	{
//		/*
//		[Title("Collector")]
//		[SerializeReference]
//		public SensorCollector[] sensorCollectors = new SensorCollector[0];
//#if UNITY_EDITOR
//		[SerializeField, HideInEditorMode]
//		private int countCollectors;
//#endif
//		[Title("Filter")]
//		[SerializeReference]
//		public SensorFilter[] sensorFilters = new SensorFilter[0];
//		[Title("Result")]
//		[ShowInInspector, Sirenix.OdinInspector.ReadOnly]
//		public List<(ObjectBehaviour, SensoringData)> sensorResult = new List<(ObjectBehaviour, SensoringData)>();

//		private Collider[] selfCollider = null;

//		public PhysicsWorld physicsWorld { get; private set; }
//		public CollisionWorld collisionWorld { get; private set; }

//		protected override void BaseStart()
//		{
//			World world = World.DefaultGameObjectInjectionWorld;
//			var entityManager = world.EntityManager;
//			EntityQuery query = entityManager.CreateEntityQuery(ComponentType.ReadOnly<PhysicsWorldSingleton>());
//			PhysicsWorldSingleton physicsWorldSingleton = query.GetSingleton<PhysicsWorldSingleton>();
//			physicsWorld = physicsWorldSingleton.PhysicsWorld;
//			collisionWorld = physicsWorld.CollisionWorld;

//			sensorResult = new List<(ObjectBehaviour, SensoringData)>();

//			selfCollider = ThisContainer.ThisObject.GameObject.GetComponentsInChildren<Collider>(true);
//		}

//		public void SensorUpdate()
//		{
//			if(!isActiveAndEnabled) return;
//			if(sensorResult == null || selfCollider == null) return;

//			World world = World.DefaultGameObjectInjectionWorld;
//			var entityManager = world.EntityManager;
//			EntityQuery query = entityManager.CreateEntityQuery(ComponentType.ReadOnly<PhysicsWorldSingleton>());
//			PhysicsWorldSingleton physicsWorldSingleton = query.GetSingleton<PhysicsWorldSingleton>();
//			physicsWorld = physicsWorldSingleton.PhysicsWorld;
//			collisionWorld = physicsWorld.CollisionWorld;

//			HashSet<Collider> detectedList = new HashSet<Collider>();
//			{
//				int length = sensorCollectors == null ? 0 : sensorCollectors.Length;
//				for(int i = 0 ; i < length ; i++)
//				{
//					var sensorModule = sensorCollectors[i];
//					sensorModule.index = i;
//					sensorModule.OnSensorUpdate(this, ref detectedList);
//				}
//				length = selfCollider.Length;
//				for(int i = 0 ; i < length ; i++)
//				{
//					detectedList.Remove(selfCollider[i]);
//				}
//			}



//#if UNITY_EDITOR
//			countCollectors = detectedList.Count;
//#endif
//			if(detectedList.Count > 0)
//			{
//				List<Collider> colliderList = detectedList.ToList();
//				var tempColliderDataList = new NativeArray<SensoringData>(colliderList.Count, Allocator.TempJob);
//				for(int i = 0 ; i<colliderList.Count ; i++)
//				{
//					Collider item = colliderList[i];
//					tempColliderDataList[i] = new SensoringData(ThisTransform, item, i);
//				}


//				NativeSlice<SensoringData> dataList = new NativeSlice<SensoringData>(tempColliderDataList);

//				int length = sensorFilters == null ? 0 : sensorFilters.Length;
//				for(int i = 0 ; i < length ; i++)
//				{
//					var sensorModule = sensorFilters[i];
//					sensorModule.index = i;
//					sensorModule.OnSensorUpdate(this, in colliderList, ref dataList);
//				}

//				sensorResult.Clear();
//				length = dataList.Length;
//				for(int i = 0 ; i < length ; i++)
//				{
//					var data = dataList[i];
//					if(data.IsSafeFilter)
//					{
//						Collider collider = colliderList[data.Index];
//						if(collider.TryGetComponent<AttackingHelper>(out var attackingHelper))
//						{
//							if(attackingHelper.ThisObject.ThisContainer.TryGetComponent<IFFProvider>(out var iffProvider))
//							{
//								data.detectTeamID = iffProvider.TeamID;
//								data.detectUnitID = iffProvider.UnitID;
//							}
//							sensorResult.Add((attackingHelper.ThisObject, data));
//						}
//					}
//				}

//				colliderList.Clear();
//				tempColliderDataList.Dispose();
//			}
//			else
//			{
//				sensorResult.Clear();
//			}
//		}

//		private NativeArray<SensoringData> RunCalculateColliderJob(Transform referenceTransform, in List<Collider> Colliders)
//		{
//			// 콜라이더 위치 배열 생성
//			// 결과 저장 배열 생성
//			NativeArray<SensoringData> results = new NativeArray<SensoringData>(Colliders.Count, Allocator.TempJob);
//			for(int i = 0 ; i<Colliders.Count ; i++)
//			{
//				Collider item = Colliders[i];
//				results[i] = new SensoringData(referenceTransform, item, i);
//			}

//			return results;
//		}
//		[Obsolete]
//		public struct SensoringData
//		{
//			public int Index;                           // 원래 리스트에서 콜리더의 인덱스
//			public bool IsSafeFilter;                     // 필터 조건을 만족하면 true

//			public int detectTeamID;
//			public int detectUnitID;

//			public int colliderID;
//			public enum Type : byte
//			{
//				[InspectorName("구형")]
//				SphereCollider = 0,
//				[InspectorName("규브")]
//				BoxCollider = 1,
//				[InspectorName("미지원")]
//				Unsupported = 255
//			}
//			public Type ColliderType;

//			public float3 CenterPoint;                  // 중심 지점
//			public float3 ColliderSize;                 // 크기
//			public float ColliderRotateY;               // 회전
//			public float3 ClosestPoint;                 // 가장 가까운 지점
//			public float3 ClosestPointFar;              // 가장 먼 지점
//			public float3 ClosestPointRight;            // 가장 오른쪽 지점
//			public float3 ClosestPointLeft;             // 가장 왼쪽 지점
//			public float3 ClosestPointTop;              // 가장 위 지점
//			public float3 ClosestPointBottom;           // 가장 아래 지점


//			public float3 TransformPosition;            // 월드 기준 Transform 위치
//			public float TransformYAxis;                // 월드 기준 Transform Y 회전값
//			public float3 TransformForward;             // 월드 기준 Transform 정면 방향

//			public float Distance;                      // Transform => ClosestPoint 의 거리
//			public float3 Direction;                    // Transform => CenterPoint 의 방향의 백터
//			public float AngleYAxis;                    // TransformForward 과 Direction사이 각도 (-180 ~ 180)
//			public float AngleYAxis360 => AngleYAxis >= 0 ? AngleYAxis : 360f + AngleYAxis; // TransformForward 과 Direction사이 각도 (0 ~ 360)

//			public float DistanceXZ;
//			public float3 DirectionXZ;
//			public float3 DirectionXZnormalize;         // Direction 의 (XZ + normalize)
//			public float3 TransformForwardXZnormalize;  // TransformForward 의 (XZ + normalize)

//			public SensoringData(Transform transform, Collider collider, int index)
//			{
//				Index = index;
//				IsSafeFilter = true;

//				detectTeamID = -1;
//				detectUnitID = -1;

//				colliderID = collider.GetInstanceID();
//				CenterPoint = collider.bounds.center;
//				switch(collider)
//				{
//					case UnityEngine.SphereCollider sphere:
//					{
//						ColliderType = Type.SphereCollider;
//						ColliderSize = Vector3.one * sphere.radius;
//						ColliderRotateY = 0f;
//					}
//					break;
//					case UnityEngine.CapsuleCollider capsule:
//					{
//						ColliderType = Type.SphereCollider;
//						ColliderSize = Vector3.one * capsule.radius;
//						ColliderRotateY = 0f;
//					}
//					break;
//					case UnityEngine.BoxCollider box:
//					{
//						ColliderType = Type.BoxCollider;
//						ColliderSize = box.size;
//						ColliderRotateY = box.transform.rotation.y;
//					}
//					break;
//					default:
//					{
//						ColliderType = Type.Unsupported;
//						ColliderSize = Vector3.zero;
//						ColliderRotateY = 0f;
//					}
//					break;
//				}
//				Vector3 boundsCenter = collider.bounds.center;
//				float boundFarSize = collider.bounds.size.magnitude * 10f;
//				CenterPoint = boundsCenter;
//				ClosestPoint = collider.ClosestPoint(transform.position);
//				ClosestPointFar = collider.ClosestPoint(boundsCenter + transform.forward * boundFarSize);
//				ClosestPointRight = collider.ClosestPoint(boundsCenter + transform.right * boundFarSize);
//				ClosestPointLeft = collider.ClosestPoint(boundsCenter + -transform.right * boundFarSize);
//				ClosestPointTop = collider.ClosestPoint(boundsCenter + transform.up* boundFarSize);
//				ClosestPointBottom = collider.ClosestPoint(boundsCenter + -transform.up * boundFarSize);

//				TransformPosition = transform.position;
//				TransformForward = transform.forward;
//				TransformYAxis = transform.eulerAngles.y;

//				Direction = CenterPoint - TransformPosition;
//				DirectionXZ = new float3(Direction.x, 0, Direction.z);
//				Distance = math.length(Direction);
//				DistanceXZ = math.length(DirectionXZ);
//				DirectionXZnormalize = DirectionXZ;
//				TransformForwardXZnormalize = new float3(TransformForward.x, 0, TransformForward.z);
//				DirectionXZnormalize = math.normalize(DirectionXZnormalize);
//				TransformForwardXZnormalize = math.normalize(TransformForwardXZnormalize);
//				float dot = math.dot(TransformForwardXZnormalize, DirectionXZnormalize);
//				AngleYAxis = math.degrees(math.acos(dot));
//				float3 cross = math.cross(TransformForwardXZnormalize, DirectionXZnormalize);
//				if(cross.y < 0)
//				{
//					AngleYAxis = -AngleYAxis;
//				}
//			}
//		}

//		void IShapeRenderer.OnDrawShapes(Camera cam)
//		{
//			if(!isActiveAndEnabled) return;
//			Draw.Matrix = transform.localToWorldMatrix;
//			Vector3 thisPos = transform.position;
//			thisPos.y += 0.1f;
//			int count = 0;
//			count = sensorCollectors ==null ? 0 : sensorCollectors.Length;
//			for(int i = 0 ; i < count ; i++)
//			{
//				sensorCollectors[i]?.OnDraw(cam);
//			}

//			count = sensorFilters ==null ? 0 : sensorFilters.Length;
//			for(int i = 0 ; i < count ; i++)
//			{
//				sensorFilters[i]?.OnDraw(cam);
//			}


//			int resultCount = sensorResult == null ? 0 : sensorResult.Count;
//			Draw.ResetAllDrawStates();
//			Quaternion rot = Quaternion.Euler(-90f, 0f, 0f);
//			Color drawColor = Color.yellow;
//			drawColor.a = 0.5f;
//			Draw.Color = drawColor;
//			Color lineStartColor = Color.red;
//			lineStartColor.a = 0.5f;
//			for(int i = 0 ; i < resultCount ; i++)
//			{
//				var result = sensorResult[i];
//				if(result.Item1 == null) continue;
//				ReactionTarget reactionTarget =  result.Item1.GetComponentInChildren<ReactionTarget>();
//				if(reactionTarget == null) continue;
//				reactionTarget.UpdateCollider();
//				if(reactionTarget.SelectCollider == null) continue;
//				var collider = reactionTarget.SelectCollider;
//				var bounds = collider.bounds;
//				var boundCenter= bounds.center;
//				boundCenter.y = thisPos.y;
//				Draw.Line(thisPos, boundCenter, 0.1f, lineStartColor, drawColor);
//				Draw.RectangleBorder(boundCenter, rot, bounds.size.XZ(), 0.1f);
//			}
//		}
//		*/
//	}
//}
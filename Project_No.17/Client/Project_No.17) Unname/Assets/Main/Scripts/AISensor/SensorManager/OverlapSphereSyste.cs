namespace BC.AISensor
{
	//public struct OverlapSphereData : IComponentData
	//{
	//	public float3 Center;
	//	public float Radius;
	//	public QueryParameters queryParameters;
	//	//	public SensorCollector collector;
	//}
	//public struct CollectorData
	//{
	//	public SensorCollector collector;
	//}
	//public partial class OverlapSphereSystem : SystemBase
	//{
	//	private EntityManager entityManager;
	//	private PhysicsWorld physicsWorld;
	//	private CollisionWorld collisionWorld;
	//
	//	public int MaxHitCount;
	//	protected override void OnCreate()
	//	{
	//		base.OnCreate();
	//		Enabled = false;
	//
	//		MaxHitCount = 10;
	//		World world = World.DefaultGameObjectInjectionWorld;
	//		entityManager = world.EntityManager;
	//		EntityQuery query = entityManager.CreateEntityQuery(ComponentType.ReadOnly<PhysicsWorldSingleton>());
	//		PhysicsWorldSingleton physicsWorldSingleton = query.GetSingleton<PhysicsWorldSingleton>();
	//		physicsWorld = physicsWorldSingleton.PhysicsWorld;
	//		collisionWorld = physicsWorld.CollisionWorld;
	//	}
	//	protected override void OnUpdate()
	//	{
	//		// Step 1: OverlapSphereData 엔티티 수 계산
	//		Debug.Log("OverlapSphereSystem:Step 1");
	//		var overlapSphereQuery = GetEntityQuery(typeof(OverlapSphereData));
	//		int overlapSphereCount = overlapSphereQuery.CalculateEntityCount();
	//		if(overlapSphereCount == 0) return;
	//
	//		// Step 2: NativeArray 준비
	//		Debug.Log("OverlapSphereSystem:Step 2");
	//
	//		var collectors = new NativeArray<CollectorData>(overlapSphereCount, Allocator.TempJob);
	//		var commands = new NativeArray<OverlapSphereCommand>(overlapSphereCount, Allocator.TempJob);
	//		var results = new NativeArray<ColliderHit>(overlapSphereCount * MaxHitCount, Allocator.TempJob);
	//
	//		// Step 3: 명령 데이터 병렬로 준비
	//		Debug.Log("OverlapSphereSystem:Step 3");
	//		var commandJob = Entities
	//		.WithName("PrepareOverlapSphereCommands")
	//		.ForEach((int entityInQueryIndex, in OverlapSphereData data) =>
	//		{
	//			commands[entityInQueryIndex] = new OverlapSphereCommand
	//			{
	//				point = data.Center,
	//				radius = data.Radius,
	//				queryParameters = data.queryParameters
	//			};
	//			//collectors[entityInQueryIndex] = new CollectorData{
	//			//	collector = data.collector
	//			//};
	//		})
	//		.ScheduleParallel(Dependency);
	//
	//		// Step 4: OverlapSphereCommand 실행
	//		Debug.Log("OverlapSphereSystem:Step 4");
	//		var overlapJobHandle = OverlapSphereCommand.ScheduleBatch(commands, results, 10, MaxHitCount, commandJob);
	//
	//		// Step 5: 작업 완료 대기 및 결과 처리
	//		Debug.Log("OverlapSphereSystem:Step 5");
	//		var processJobHandle = Job.WithName("ProcessOverlapResults")
	//			.WithCode(() =>
	//			{
	//				for (int i = 0; i < results.Length; i++)
	//				{
	//					ColliderHit hit = results[i];
	//				}
	//			})
	//		.Schedule(overlapJobHandle);
	//
	//		// Step 6: NativeArray 해제
	//		Debug.Log("OverlapSphereSystem:Step 6");
	//		processJobHandle.Complete(); // 작업 완료 대기
	//		commands.Dispose();
	//		results.Dispose();
	//
	//		Dependency = processJobHandle; // 의존성 연결
	//	}
	//}



}

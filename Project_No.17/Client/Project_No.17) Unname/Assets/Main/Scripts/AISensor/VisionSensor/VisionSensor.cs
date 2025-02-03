using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine;

namespace BC.AISensor
{
	public class VisionSensor : MonoBehaviour
	{
		[Header("Raycast Configuration")]
		public float RayPerAngle = 1f; // 각도당 발사하는 레이 수
		[Range(0f,360f)]
		public float Angle = 45f; // 레이의 양끝을 기준으로 하는 총 각도
		public float RayLength = 10f; // 레이 길이
		public float EyeHeight = 1.5f;

		private int RayCount;
		[SerializeField] protected LayerMask ObstacleMask;


		private NativeArray<RaycastRequest> raycastRequests;
		private NativeArray<RaycastHit> raycastHits;

		private NativeArray<RaycastCommand> raycastCommand;

		JobHandle setRaycastRequestsSchedule;
		SetRaycastRequestsJob setRaycastRequestsJob;

		JobHandle setRaycastCommandSchedule;
		SetRaycastCommandJob setRaycastCommandJob;

		JobHandle raycastSchedule;
		public void Start()
		{
			RayCount = 0;

			setRaycastRequestsJob = new SetRaycastRequestsJob() {
				raycastRequests = raycastRequests,
			};

			setRaycastCommandJob = new SetRaycastCommandJob() {
				RayQueryParameters = new QueryParameters(ObstacleMask, false, QueryTriggerInteraction.UseGlobal, false),
				raycastRequests = raycastRequests,
				raycastCommand = raycastCommand,
			};
		}

		public void Update()
		{
			Vector3 startPosition = transform.position + Vector3.up * EyeHeight;
			int totalRayCount = Mathf.Abs(Mathf.RoundToInt(RayPerAngle * Angle));

			if(RayCount != totalRayCount)
			{
				DeleteNativeArray();
				RayCount = totalRayCount;
				InitNativeArray();
			}

			if(totalRayCount > 360*4) totalRayCount = 360*4;
			if(totalRayCount == 0) return;

			float halfAngle = Angle / 2f;
			float angleStep = (Angle / (totalRayCount));
			if(angleStep + Angle > 360f)
			{
				halfAngle = 360f / 2f;
				angleStep = 360f / (totalRayCount);
			}

			setRaycastRequestsJob.StartPosition = startPosition;
			setRaycastRequestsJob.RayLength = RayLength;
			setRaycastRequestsJob.FirstAngle = -halfAngle;
			setRaycastRequestsJob.AngleStep = angleStep;
			setRaycastRequestsJob.LookForward = transform.forward;
			setRaycastRequestsSchedule = setRaycastRequestsJob.Schedule(RayCount, 10, default(JobHandle));
			setRaycastRequestsSchedule.Complete();

			setRaycastCommandSchedule = setRaycastCommandJob.Schedule(RayCount, 10, setRaycastRequestsSchedule);
			setRaycastCommandSchedule.Complete();

			raycastSchedule = RaycastCommand.ScheduleBatch(raycastCommand, raycastHits, RayCount, setRaycastCommandSchedule);
			raycastSchedule.Complete();


			for(int i = 0 ; i < RayCount ; i++)
			{
				var rayHit = raycastHits[i];
				if(rayHit.collider != null)
				{
					Debug.DrawLine(startPosition, rayHit.point, Color.blue);
				}
				else
				{
					Debug.DrawLine(startPosition, raycastRequests[i].EndPosition, Color.red);
				}
			}
		}
		private void InitNativeArray()
		{
			raycastRequests = new NativeArray<RaycastRequest>(RayCount, Allocator.TempJob);
			raycastHits = new NativeArray<RaycastHit>(RayCount, Allocator.TempJob);
			raycastCommand = new NativeArray<RaycastCommand>(RayCount, Allocator.TempJob);

			setRaycastRequestsJob = new SetRaycastRequestsJob() {
				raycastRequests = raycastRequests,
			};

			setRaycastCommandJob = new SetRaycastCommandJob() {
				RayQueryParameters = new QueryParameters(ObstacleMask, false, QueryTriggerInteraction.UseGlobal, false),
				raycastRequests = raycastRequests,
				raycastCommand = raycastCommand,
			};
		}
		private void DeleteNativeArray()
		{
			if(raycastRequests.IsCreated) raycastRequests.Dispose();
			if(raycastHits.IsCreated) raycastHits.Dispose();
			if(raycastCommand.IsCreated) raycastCommand.Dispose();
		}

		[BurstCompile]
		struct SetRaycastRequestsJob : IJobParallelFor
		{
			public Vector3 StartPosition;
			public Vector3 LookForward;
			public float FirstAngle;
			public float AngleStep;
			public float RayLength;

			[WriteOnly]
			public NativeArray<RaycastRequest> raycastRequests;

			public void Execute(int index)
			{
				Vector3 Direction = Quaternion.Euler(0, FirstAngle + AngleStep * index , 0) * LookForward;

				Vector3 EndPosition = StartPosition + Direction * RayLength;

				raycastRequests[index] = new RaycastRequest() {
					StartPosition = StartPosition,
					EndPosition = EndPosition,
				};
			}
		}
		[BurstCompile]
		struct SetRaycastCommandJob : IJobParallelFor
		{
			public QueryParameters RayQueryParameters;

			[ReadOnly]
			public NativeArray<RaycastRequest> raycastRequests;
			[WriteOnly]
			public NativeArray<RaycastCommand> raycastCommand;

			public void Execute(int index)
			{
				var request = raycastRequests[index];
				float3 normal = math.normalizesafe(request.EndPosition - request.StartPosition);
				float distance = math.distance(request.EndPosition , request.StartPosition);

				raycastCommand[index] = new RaycastCommand(request.StartPosition, normal, RayQueryParameters, distance);
			}
		}
	}
}

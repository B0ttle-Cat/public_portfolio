using BC.ODCC;

using UnityEngine;
namespace TFContent
{
	public class ProjectileMovementCompute : ComponentBehaviour//, IOdccUpdate
	{
		private OdccQuerySystem moveTowardsQuery;
		private OdccQuerySystem moveTrackingQuery;

		private OdccQueryCollector moveTowardsCollector;
		private OdccQueryCollector moveTrackingCollector;

		protected override void BaseAwake()
		{
			moveTowardsQuery = OdccQueryBuilder.CreateQuery().WithAll<ProjectileObject, ProjectileLifeData, ProjectileMoveTowardsData>().Build();

			moveTrackingQuery = OdccQueryBuilder.CreateQuery().WithAll<ProjectileObject, ProjectileLifeData, ProjectileMoveTrackingData>().Build();

			moveTowardsCollector = OdccQueryCollector.CreateQueryCollector(moveTowardsQuery, this)
				.CreateFixedLooperEvent(nameof(MoveTowardsUpdate)).CallForeach<ProjectileObject, ProjectileLifeData, ProjectileMoveTowardsData>(MoveTowardsUpdate)
				.GetCollector();

			moveTrackingCollector = OdccQueryCollector.CreateQueryCollector(moveTrackingQuery, this)
				.CreateFixedLooperEvent(nameof(MoveTrackingUpdate)).CallForeach<ProjectileObject, ProjectileLifeData, ProjectileMoveTrackingData>(MoveTrackingUpdate)
				.GetCollector();
		}
		protected override void BaseDestroy()
		{
			moveTowardsQuery = null;
			if(moveTowardsCollector != null)
			{
				moveTowardsCollector.DeleteLooperEvent(nameof(MoveTowardsUpdate));
				moveTowardsCollector = null;
			}
			moveTrackingQuery = null;
			if(moveTrackingCollector != null)
			{
				moveTrackingCollector.DeleteLooperEvent(nameof(MoveTrackingUpdate));
				moveTrackingCollector = null;
			}
		}

		private void MoveTowardsUpdate(OdccQueryLooper.LoopInfo loopInfo, ProjectileObject projectile, ProjectileLifeData lifeData, ProjectileMoveTowardsData moveData)
		{
			if(!projectile.isActiveAndEnabled) return;
			if(lifeData.IsDead()) return;

			projectile.OnUpdate(moveData.Forward * moveData.moveSpeed, Time.fixedDeltaTime);
		}
		private void MoveTrackingUpdate(OdccQueryLooper.LoopInfo loopInfo, ProjectileObject projectile, ProjectileLifeData lifeData, ProjectileMoveTrackingData moveData)
		{
			if(!projectile.isActiveAndEnabled) return;
			if(lifeData.IsDead()) return;

			moveData.targetPosition = moveData.GetTrackingPosition();
			projectile.OnUpdate(moveData.Forward * moveData.moveSpeed, Time.fixedDeltaTime);
		}
	}
}
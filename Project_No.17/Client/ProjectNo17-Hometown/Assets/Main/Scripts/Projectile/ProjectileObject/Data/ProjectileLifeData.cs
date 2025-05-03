using System;

using Sirenix.OdinInspector;

using UnityEngine;
namespace BC.Projectile
{
	public class ProjectileLifeData : ProjectileData
	{
		public ProjectileLifeData() : base()
		{

		}

		public const float LimitLifeTime = 300f;
		public const int LimitHitCount = 10000;

		[SerializeField, ReadOnly]
		private bool isAlive;
		[SerializeField]
		private float lifeTime = 1f;
		[SerializeField]
		private int hitCount = 1;
		private Action onDead;

		public float LifeTime {
			get => lifeTime;
			set {
				lifeTime=value;
				if(IsDead() && isAlive)
				{
					isAlive = false;
					OnDead?.Invoke();
					OnDead = null;
				}
			}
		}
		public int HitCount {
			get => hitCount;
			set {
				hitCount=value;
				if(IsDead())
				{
					if(isAlive)
					{
						isAlive = false;
						OnDead?.Invoke();
						OnDead = null;
					}
				}
				else if(!isAlive)
				{
					isAlive = true;
				}
			}
		}
		public Action OnDead { get => onDead; set => onDead=value; }

		public bool IsAlive() => isAlive && LifeTime > 0f && HitCount > 0;
		public bool IsDead() => !IsAlive();

		public void InitLife(float lifeTime, int hitCount)
		{
			this.lifeTime = lifeTime;
			this.hitCount = hitCount;
			this.isAlive = lifeTime > 0f && hitCount > 0;
		}
		protected override void Disposing()
		{
			OnDead = null;
		}
	}
}
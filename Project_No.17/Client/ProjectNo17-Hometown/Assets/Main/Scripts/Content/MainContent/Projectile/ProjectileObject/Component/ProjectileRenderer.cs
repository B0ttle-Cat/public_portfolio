using BC.Base;
using BC.ODCC;

using UnityEngine;
using UnityEngine.VFX;
namespace TFContent
{
	public class ProjectileRenderer : ComponentBehaviour, IProjectileLifecycle
	{
		[SerializeField]
		private LayerMask rendererLayer;
		[SerializeField]
		private GameObject meshObject;
		[SerializeField]
		private VisualEffect tailObject;

		void IProjectileLifecycle.Fire()
		{
			if(meshObject != null)
			{
				meshObject.layer = rendererLayer.ToLayer();
				meshObject.SetActive(true);
			}
			if(tailObject != null)
			{
				tailObject.gameObject.layer = rendererLayer.ToLayer();
				tailObject.gameObject.SetActive(true);
				tailObject.Play();
			}
		}

		void IProjectileLifecycle.Update(float deltaTime)
		{
		}

		void IProjectileLifecycle.Dead()
		{
			if(meshObject != null)
			{
				meshObject.SetActive(false);
			}
			if(tailObject != null)
			{
				tailObject.Stop();
			}
		}
	}
}
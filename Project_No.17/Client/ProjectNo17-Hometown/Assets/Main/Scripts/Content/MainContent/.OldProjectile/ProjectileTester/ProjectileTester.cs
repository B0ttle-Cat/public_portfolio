using System;
using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector;

using Unity.Entities;

using UnityEngine;

using Random = UnityEngine.Random;

namespace TFContent
{
	public class ProjectileTester : MonoBehaviour
	{
		public Transform startPosition;
		public Transform endPosition;


		[SerializeField, HideInEditorMode]
		private List<ProjectileType> projectileTypeList;
		public ProjectileInstantiateData projectileInstantiateData;
		public int createCount;

		[Serializable, InlineProperty]
		private struct ProjectileType
		{
			[InlineButton("TestCreate"), HideLabel]
			public string typeName;

			public void TestCreate(string projectileType)
			{
				var activeGameObject = UnityEditor.Selection.activeGameObject;
				if(activeGameObject == null) return;
				if(ProjectileManager.Manager == null) return;
				if(activeGameObject.TryGetComponent<ProjectileTester>(out var tester))
				{
					Vector3 startPosition = tester.startPosition == null ? tester.transform.position : tester.startPosition.position; ;

					tester.projectileInstantiateData.startPosition = startPosition;
					if(tester.endPosition != null)
					{
						tester.projectileInstantiateData.trackingEntity = ProjectileManager.Manager.GetTrackingEntity(tester.endPosition);
						tester.projectileInstantiateData.targetPosition = tester.endPosition.position;
					}
					else
					{
						tester.projectileInstantiateData.trackingEntity = Entity.Null;
						var targetPosition = startPosition + Random.onUnitSphere;
						targetPosition.y = 0f;
						tester.projectileInstantiateData.targetPosition = targetPosition.normalized;
					}

					int createCount = tester.createCount;
					for(int i = 0 ; i < createCount ; i++)
					{
						ProjectileManager.Manager.Fire(projectileType, tester.projectileInstantiateData);
					}
				}
			}
		}

		private void Start()
		{
			projectileTypeList = ProjectileManager.Manager.projectileDataBakers.Select(i => new ProjectileType() { typeName = i.ProjectileName }).ToList();
		}
	}
}

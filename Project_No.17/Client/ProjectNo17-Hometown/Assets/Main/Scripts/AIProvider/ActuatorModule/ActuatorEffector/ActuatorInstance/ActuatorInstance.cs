using System;
using System.Collections.Generic;
using System.Linq;

using BC.Base;
using BC.ODCC;
using BC.OdccBase;

using Sirenix.OdinInspector;

using UnityEngine;

namespace BC.Actuator
{
	public class ActuatorInstance : ScriptableObject
	{
		[SerializeField]
		private InstanceObject[] instanceObjects;
		public enum InstanceType
		{
			[LabelText("EmptyType")]
			None,
			[LabelText("투사체")]
			Projectile,
			[LabelText("시각효과")]
			VFX,
			[LabelText("소환체")]
			Summon,
		}

		[Serializable]
		public class InstanceObject : IInstanceObject
		{
			[HorizontalGroup("T", width: 95)]
			[HideLabel, PreviewField(95, ObjectFieldAlignment.Left),PropertyOrder(-2),]
			public GameObject prefabObject = null;

			[HorizontalGroup("T/V/H", width: 100), ToggleLeft]
			public bool isActive = true;
			[HorizontalGroup("T/V/H"), HideLabel, SuffixLabel("Trigger ID",overlay: true)]
			public int triggerID = 0;
			[HorizontalGroup("T/V/H"), HideLabel]
			public InstanceType effectorType = InstanceType.None;

			[HorizontalGroup("T/V/HH"), LabelText("트리거 작동시 생성 요청 정보")]
			public IstanceRequest[] istanceRequests = new IstanceRequest[1] { new(0f, 1) };
			[Serializable]
			public struct IstanceRequest
			{
				[HorizontalGroup, HideLabel, SuffixLabel("요청 당 지연시간", overlay: true)]
				public float requestDelay;
				[HorizontalGroup, HideLabel, SuffixLabel("요청 당 생성 개수", overlay: true)]
				public int requestPerCount;
				public IstanceRequest(float requestDelay = 0, int requestPerCount = 1)
				{
					this.requestDelay=requestDelay;
					this.requestPerCount=requestPerCount;
				}
			}

			[SerializeReference, InfoBox("@GetDataListWarning()", InfoMessageType.Warning, VisibleIf = "@HasNonCopyableItem()")]
			public DataObject[] dataList;
			public ActuatorFormulaSystem[] overrideFormulas;

			public bool IsValidInterface => prefabObject == null || !prefabObject.TryGetComponent<ObjectBehaviour>(out ObjectBehaviour obj) ? false : effectorType switch {
				InstanceType.Projectile => prefabObject.TryGetComponent<IProjectileObject>(out _) ? true : false,
				InstanceType.Summon => prefabObject.TryGetComponent<IActuatorEffectInstantiate_Summon>(out _) ? true : false,
				InstanceType.VFX => prefabObject.TryGetComponent<IActuatorEffectInstantiate_VFX>(out _) ? true : false,
				_ => prefabObject.TryGetComponent<IActuatorEffectInstantiate>(out _) ? true : false,
			};

#if UNITY_EDITOR
			[InfoBox("@Error_IsValidInterface", InfoMessageType.Error, VisibleIf = "@IsNotValidInterface")]
			[VerticalGroup("T/V"), ShowInInspector, PropertyOrder(-1), DisplayAsString(EnableRichText = true), HideLabel, EnableGUI]
			string EffectorObjectName => $"<b>{(prefabObject == null ? "Null" : prefabObject.name)}</b>";

			private bool IsNotValidInterface => !IsValidInterface;
			private string Error_IsValidInterface => prefabObject == null ? "Null Object 가 할당 되어 있습니다." :
				!prefabObject.TryGetComponent<ObjectBehaviour>(out var _) ? "ObjectBehaviour이 할당 되어야 합니다." :
				effectorType switch {
					InstanceType.Projectile => $"이 오브젝트에는 {nameof(IProjectileObject)}를 구현한 컴퍼넌트가 있어야 합니다!",
					InstanceType.Summon => $"이 오브젝트에는 {nameof(IActuatorEffectInstantiate_Summon)}를 구현한 컴퍼넌트가 있어야 합니다!",
					InstanceType.VFX => $"이 오브젝트에는 {nameof(IActuatorEffectInstantiate_VFX)}를 구현한 컴퍼넌트가 있어야 합니다!",
					_ => $"이 오브젝트에는 {nameof(IActuatorEffectInstantiate)}를 구현한 컴퍼넌트가 있어야 합니다!"
				};
			private bool HasNonCopyableItem()
			{
				if(dataList == null) return false;
				foreach(var item in dataList)
				{
					if(item is not IDataCopy)
						return true;
				}
				return false;
			}
			private string GetDataListWarning()
			{
				return "dataList에는 IDataCopy를 구현하지 않은 항목이 포함되어 있습니다.";
			}
#endif
		}


		public List<InstanceObject> GetEffectorObject(int triggerID)
		{
			List<InstanceObject> triggering = new List<InstanceObject>();

			int length = instanceObjects == null ? 0 : instanceObjects.Length;
			for(int i = 0 ; i < length ; i++)
			{
				var effectObject = instanceObjects[i];
				if(effectObject.isActive && effectObject.triggerID == triggerID && effectObject.IsValidInterface)
				{
					triggering.Add(effectObject);
				}
			}

			return triggering;
		}

		public async void OnInstantiateEffectorObject(InstanceObject effectorObject,
			ObjectBehaviour orderObject, ReactionTargetInfo targetValue,
			Func<InstanceObject, ObjectBehaviour, InstanceType, bool> instantiateCallback)
		{
			var type = effectorObject.effectorType;
			var istanceRequests = effectorObject.istanceRequests;
			if(istanceRequests == null || istanceRequests.Length == 0)
			{
				istanceRequests = new InstanceObject.IstanceRequest[1] { new(0f, 1) };
			}

			int requestsLength = istanceRequests.Length;
			for(int r = 0 ; r < requestsLength ; r++)
			{
				var request = istanceRequests[r];
				await Awaitable.WaitForSecondsAsync(request.requestDelay);
				int requestPerCount = request.requestPerCount;
				if(requestPerCount <= 0) continue;
				var instantiateList = type switch
				{
					InstanceType.Projectile => await ProjectileInstantiate(effectorObject.prefabObject, requestPerCount),
					InstanceType.VFX => await VFXInstantiate(effectorObject.prefabObject, requestPerCount),
					InstanceType.Summon => await SummonInstantiate(effectorObject.prefabObject, requestPerCount),
					_ => await NoneInstantiate(effectorObject.prefabObject, requestPerCount),
				};
				foreach(var newInstantiate in instantiateList)
				{
					if(newInstantiate == null) continue;
					var newObject = newInstantiate.GameObject;
					newObject.SetActive(true);
					newObject.name = $"{effectorObject.prefabObject.name} ({orderObject.GameObject.name} => {targetValue.ToString()})";
					List<IOdccData> initDataList = new List<IOdccData>();
					int length = effectorObject.dataList.Length;
					for(int i = 0 ; i < length ; i++)
					{
						if(effectorObject.dataList[i] is IDataCopy dataCopy)
						{
							var copy = dataCopy.DataCopy();
							if(copy != null) initDataList.Add(copy);
						}
					}
					newInstantiate.ThisContainer.RemoveDatas(initDataList);
					newInstantiate.ThisContainer.AddDatas(initDataList);
					bool isValid = instantiateCallback.Invoke(effectorObject, newInstantiate, type);
					if(!isValid)
					{
						GameObject.Destroy(newObject);
					}
				}
			}
			async Awaitable<IEnumerable<ObjectBehaviour>> NoneInstantiate(GameObject prefab, int instantiateCount)
			{
				GameObject[] gameObjects = await GameObject.InstantiateAsync(prefab, instantiateCount, targetValue.Position, orderObject.ThisTransform.rotation);
				return gameObjects == null ? null : gameObjects.Select(i => i.GetComponent<ObjectBehaviour>());
			}
			async Awaitable<IEnumerable<ObjectBehaviour>> ProjectileInstantiate(GameObject prefab, int instantiateCount)
			{
				var projectileObject = await EventManager.Call<IProjectileManager, IProjectileObject[]>(async call =>
					call.SetPoolingProjectile(prefab) ? await call.InstanceProjectile<IProjectileObject>(prefab.name, instantiateCount, false) : null);
				return projectileObject == null ? null : projectileObject.Select(i => i.GameObject.GetComponent<ObjectBehaviour>());
			}
			async Awaitable<IEnumerable<ObjectBehaviour>> VFXInstantiate(GameObject prefab, int instantiateCount)
			{
				GameObject[] gameObjects = await GameObject.InstantiateAsync(prefab, instantiateCount, targetValue.Position, orderObject.ThisTransform.rotation);
				return gameObjects == null ? null : gameObjects.Select(i => i.GetComponent<ObjectBehaviour>());
			}
			async Awaitable<IEnumerable<ObjectBehaviour>> SummonInstantiate(GameObject prefab, int instantiateCount)
			{
				GameObject[] gameObjects = await GameObject.InstantiateAsync(prefab, instantiateCount, targetValue.Position, orderObject.ThisTransform.rotation);
				return gameObjects == null ? null : gameObjects.Select(i => i.GetComponent<ObjectBehaviour>());
			}
		}
	}
}

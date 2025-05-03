using System.Collections.Generic;

using BC.ODCC;

using UnityEngine;

namespace TFSystem.SaveLoad
{
	[DisallowMultipleComponent]
	public class SaveLoader : ComponentBehaviour
	{
		public SaveLoadKey GetSaveLoadKey()
		{
			return new SaveLoadKey(GetPath(ThisTransform), this);
			static string GetPath(Transform target)
			{
				Stack<string> path = new Stack<string>();
				while(target != null)
				{
					path.Push(target.name);
					target = target.parent;
				}
				return string.Join('/', path);
			}
		}

		public SaveLoadData OnSave()
		{
			SaveLoadData objectRoot = new SaveLoadData();
			List<ISaveLoadTarget> allSupporter =  ThisContainer.FindAllType<ISaveLoadTarget>();

			int length = allSupporter == null ? 0 : allSupporter.Count;
			for(int i = 0 ; i < length ; i++)
			{
				var supporter = allSupporter[i];
				if(supporter == null) continue;
				SaveLoadKey key = supporter.GetSaveLoadKey(this);
				//if(key.IsNotValid) continue; // 동일 Transform 의 경우 반값이기 때문에 이 검사를 진행하지 말아야 함
				SaveLoadData childData = new SaveLoadData();
				supporter.OnSave(ref childData);
				if(childData.IsEmpty) continue;
				objectRoot.SaveData(supporter.GetSaveLoadKey(this), childData);
			}

			for(int i = 0 ; i < length ; i++)
			{
				var supporter = allSupporter[i];
				if(supporter == null) continue;
				supporter.OnSaveComplete();
			}
			return objectRoot;
		}
		public void OnLoad(SaveLoadData parent)
		{
			var dataList = parent.SaveList;
			int length = dataList.Count;
			HashSet<ISaveLoadTarget> completedItems = new HashSet<ISaveLoadTarget>();
			for(int i = 0 ; i < length ; i++)
			{
				var keyValue = dataList[i];

				SaveLoadKey key = keyValue.key;
				//if(key.IsNotValid) continue; // 동일 Transform 의 경우 반값이기 때문에 이 검사를 진행하지 말아야 함
				SaveLoadData value = (SaveLoadData)keyValue.value;
				if(value.IsEmpty) continue;

				Transform find = string.IsNullOrWhiteSpace(key.path) ? ThisTransform : ThisTransform.Find(key.path);
				Component[] components = find.GetComponents(key.type);

				if(components == null || components.Length == 0)
				{
					// 새로 추가 해야 함.
					Component component = find.gameObject.AddComponent(key.type);
					if(component != null && component is ISaveLoadTarget supporter && completedItems.Add(supporter))
					{
						supporter.OnLoad(value);
					}
					continue;
				}
				int length2 = components.Length;
				for(int ii = 0 ; ii < length2 ; ii++)
				{
					var component = components[ii];
					if(component != null && component is ISaveLoadTarget supporter && completedItems.Add(supporter))
					{
						supporter.OnLoad(value);
						break;
					}
				}
			}

			foreach(var supporter in completedItems)
			{
				if(supporter == null) continue;
				supporter.OnLoadComplete();
			}
		}
	}
}

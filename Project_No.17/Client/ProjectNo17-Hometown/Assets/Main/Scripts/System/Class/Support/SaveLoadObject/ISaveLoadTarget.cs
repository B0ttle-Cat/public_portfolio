using System;
using System.Collections.Generic;
using System.Linq;

using BC.ODCC;

using UnityEngine;

namespace TFSystem.SaveLoad
{
	public struct SaveLoadKey
	{
		public const char GroupSeparator = '\u001D';
		public const char RecordSeparator = '\u001E';
		public const char UnitSeparator = '\u001F';
		//\u001D (Group Separator)  ▶ 그룹 전체 구분
		//   └▶ \u001E (Record Separator) ▶ 레코드 단위 구분
		//       └▶ \u001F (Unit Separator) ▶ 필드 단위 구분

		public string path;
		public Type type;
		public SaveLoadKey(string path, IOCBehaviour _this)
		{
			this.path = path;
			this.type = _this.GetType();
		}
		public override string ToString()
		{
			return $"{path}{RecordSeparator}{type?.FullName}";
		}
		public static implicit operator SaveLoadKey(string str)
		{
			var parts = str.Split(RecordSeparator);
			return new SaveLoadKey {
				path = parts[0],
				type = Type.GetType(parts[1])
			};
		}
		public bool IsValid => !string.IsNullOrWhiteSpace(path) && type != null;
		public bool IsNotValid => !IsValid;
	}

	public interface ISaveLoadTarget : IOCBehaviour, ISaveLoadData
	{
		public SaveLoadKey GetSaveLoadKey(SaveLoader saveLoader)
		{
			return new SaveLoadKey(GetPath(saveLoader.ThisTransform, ThisTransform), this);
			static string GetPath(Transform root, Transform target)
			{
				if(root == target) return "";
				Stack<string> path = new Stack<string>();

				while(root != target && target != null)
				{
					path.Push(target.name);
					target = target.parent;
				}
				return string.Join('/', path);
			}
		}


		public static void SaveThisDataObject(ContainerObject ThisContainer, ref SaveLoadData data)
		{
			List<IOdccData> dataList = ThisContainer.DataList;
			int length = dataList.Count;
			for(int i = 0 ; i < length ; i++)
			{
				if(dataList[i] is ISaveLoadJson json)
				{
					data.SaveJson(json);
				}
			}
		}
		public static void LoadThisDataObject(ContainerObject ThisContainer, in SaveLoadData data)
		{
			List<IOdccData> dataList = ThisContainer.DataList.Where(i=>i is ISaveLoadTarget).ToList();
			ThisContainer.RemoveDatas(dataList);
			dataList.Clear();
			var saveList = data.SaveList;
			int length = saveList.Count;
			for(int i = 0 ; i < length ; i++)
			{
				var item = saveList[i];
				IOdccData jsonItem = ISaveLoadJson.FromJson<IOdccData>(item);
				dataList.Add(jsonItem);
			}
			ThisContainer.AddDatas(dataList);
		}
	}
	public interface ISaveLoadData
	{
		public void OnSave(ref SaveLoadData data);
		public void OnSaveComplete() { }
		public void OnLoad(in SaveLoadData data);
		public void OnLoadComplete() { }
	}
	public interface ISaveLoadJson
	{
		string ToJson()
		{
			return JsonUtility.ToJson(this);
		}
		void FromJsonOverwrite(string json)
		{
			JsonUtility.FromJsonOverwrite(json, this);
		}
		public T FromJson<T>(string json)
		{
			return JsonUtility.FromJson<T>(json);
		}
		public object FromJson(string json)
		{
			return JsonUtility.FromJson(json, GetType());
		}

		public static object FromJson(SaveLoadData.KeyValue keyValue)
		{
			Type interfaceType = typeof(ISaveLoadJson); // 확인하고 싶은 인터페이스
			Type type = Type.GetType(keyValue.key);
			if(type == null || !interfaceType.IsAssignableFrom(type)) return null;
			if(keyValue.value is not string jsonValue) return null;

			return JsonUtility.FromJson(jsonValue, type);
		}
		public static T FromJson<T>(SaveLoadData.KeyValue keyValue)
		{
			Type interfaceType = typeof(ISaveLoadJson); // 확인하고 싶은 인터페이스
			Type type = Type.GetType(keyValue.key);
			if(type == null || !interfaceType.IsAssignableFrom(type)) return default;
			if(keyValue.value is not string jsonValue) return default;
			object jsonObject = JsonUtility.FromJson(jsonValue, type);
			if(jsonObject == null || jsonObject is not T tObject) return default;
			return tObject;
		}
	}
}

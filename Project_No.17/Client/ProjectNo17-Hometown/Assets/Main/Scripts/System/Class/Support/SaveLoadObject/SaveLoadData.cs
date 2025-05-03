using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using Sirenix.OdinInspector;

using UnityEngine;

namespace TFSystem.SaveLoad
{
	[Serializable]
	public struct SaveLoadData
	{
		[SerializeField]
		private List<KeyValue> saveList;
		public List<KeyValue> SaveList {
			get => saveList??=new();
			set => saveList=value;
		}

		public bool IsEmpty => saveList == null || saveList.Count == 0;
		[Serializable]
		public struct KeyValue
		{
			public string key;
			[ShowInInspector]
			public object value;
		}
		private bool LoadValue(string key, out object value)
		{
			int count = SaveList.Count;
			for(int i = 0 ; i < SaveList.Count ; i++)
			{
				var item = SaveList[i];
				if(item.key.Equals(key))
				{
					value = item.value;
				}
			}
			value = null;
			return value != null;
		}
		public bool LoadValue(string key, out string value)
		{
			if(LoadValue(key, out object _value) && _value is string strValue)
			{
				value = strValue;
				return true;
			}
			value = null;
			return false;
		}
		public bool LoadValue<T>(string key, out T value) where T : unmanaged
		{
			return LoadValue<T>(key, out value, default);
		}
		public bool LoadValue<T>(string key, out T value, T defaultValue = default) where T : unmanaged
		{
			if(LoadValue(key, out object _value))
			{
				if(_value is T tValue)
				{
					value = tValue;
					return true;
				}
				else if(_value is JObject jObj)
				{
					value = jObj.ToObject<T>();
					return true;
				}
				else if(_value is JArray jArray)
				{
					value = jArray.ToObject<T>();
					return true;
				}
				else if(_value is JValue jValue)
				{
					value = jValue.ToObject<T>();
					return true;
				}
			}
			value = defaultValue;
			return false;
		}
		private void SaveValue(string key, object value)
		{
			if(value == null) return;

			int count = SaveList.Count;
			for(int i = 0 ; i < count ; i++)
			{
				var item = SaveList[i];
				if(item.key == key)
				{
					item.value = value;
					SaveList[i] = item;
					return;
				}
			}
			SaveList.Add(new KeyValue { key = key, value = value });
		}
		public void SaveValue<T>(string key, T value) where T : unmanaged
		{
			int count = SaveList.Count;
			for(int i = 0 ; i < count ; i++)
			{
				var item = SaveList[i];
				if(item.key == key)
				{
					item.value = value;
					SaveList[i] = item;
					return;
				}
			}
			SaveList.Add(new KeyValue { key = key, value = value });
		}
		public void SaveValue(string key, string value)
		{
			SaveValue(key, (object)value);
		}


		public bool LoadData(SaveLoadKey key, out SaveLoadData value)
		{
			return LoadData(key.ToString(), out value);
		}
		public void SaveData(SaveLoadKey key, SaveLoadData value)
		{
			SaveData(key.ToString(), value);
		}
		public bool LoadData(string key, out SaveLoadData value)
		{
			if(LoadValue(key, out object _value))
			{
				if(_value is SaveLoadData data)
				{
					value = data;
					return true;
				}
			}
			value = default;
			return false;
		}
		public void SaveData(string key, SaveLoadData value)
		{
			SaveValue(key, (object)value);
		}

		public void LoadIData(string key, ISaveLoadData iValue)
		{
			if(LoadData(key, out SaveLoadData value))
			{
				iValue.OnLoad(in value);
			}
		}
		public void SaveIData(string key, ISaveLoadData iValue)
		{
			SaveLoadData data = new SaveLoadData();
			iValue.OnSave(ref data);
			SaveData(key, data);
		}

		public void SaveJson<T>(ISaveLoadJson target)
		{
			SaveValue(typeof(T).FullName, target.ToJson());
		}
		public void SaveJson(ISaveLoadJson target)
		{
			SaveValue(target.GetType().FullName, target.ToJson());
		}
		public void SaveJson(string key, ISaveLoadJson target)
		{
			SaveValue(key, target.ToJson());
		}
		public void LoadJsonOverwrite(ISaveLoadJson target)
		{
			LoadJsonOverwrite(target.GetType().FullName, target);
		}
		public void LoadJsonOverwrite<T>(ISaveLoadJson target)
		{
			LoadJsonOverwrite(typeof(T).FullName, target);
		}
		public void LoadJsonOverwrite(string key, ISaveLoadJson target)
		{
			if(LoadValue(key, out object load) && load is string json)
			{
				target.FromJsonOverwrite(json);
			}
		}
		public T LoadJson<T>(ISaveLoadJson target)
		{
			return LoadJson<T>(typeof(T).FullName, target);
		}
		public T LoadJson<T>(string key, ISaveLoadJson target)
		{
			if(LoadValue(key, out object load) && load is string json)
			{
				return target.FromJson<T>(json);
			}
			return default;
		}
		public object LoadJson(ISaveLoadJson target)
		{
			return LoadJson(target.GetType().FullName, target);
		}
		public object LoadJson(string key, ISaveLoadJson target)
		{
			if(LoadValue(key, out object load) && load is string json)
			{
				return target.FromJson(json);
			}
			return default;
		}
	}
}

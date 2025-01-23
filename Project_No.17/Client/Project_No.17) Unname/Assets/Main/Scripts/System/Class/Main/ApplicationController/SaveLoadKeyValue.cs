using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace TFSystem
{
	public class SaveLoadKeyValue : ISerializable
	{
		public SaveLoadKeyValue()
		{
			data = new Dictionary<string, object>();
		}
		private Dictionary<string, object> data = new Dictionary<string, object>();
		public Dictionary<string, object> Data {
			get {
				if(data== null) data = new Dictionary<string, object>();
				return data;
			}
			private set {
				data = value;
			}
		}
		public void SetData<T>(string key, T value)
		{
			Data[key] = value;
		}
		public bool HasData(string key)
		{
			return Data.ContainsKey(key);
		}
		public bool DeleteData(string key)
		{
			return Data.Remove(key);
		}
		public void ClearData()
		{
			Data.Clear();
		}
		public T GetData<T>(string key, T defaultValue = default)
		{
			if(Data.TryGetValue(key, out var value) && value is T dataValue)
			{
				return dataValue;
			}
			return defaultValue;
		}
		public bool TryGetData<T>(string key, out T tValue)
		{
			if(Data.TryGetValue(key, out var value) && value is T dataValue)
			{
				tValue = dataValue;
				return true;
			}
			tValue = default;
			return false;
		}
		public bool TryNotGetData<T>(string key, out T tValue)
		{
			if(Data.TryGetValue(key, out var value) && value is T dataValue)
			{
				tValue = dataValue;
				return false;
			}
			tValue = default;
			return true;
		}
		public T PopData<T>(string key, T defaultValue = default)
		{
			if(Data.TryGetValue(key, out var value) && value is T dataValue)
			{
				Data.Remove(key);
				return dataValue;
			}
			return defaultValue;
		}
		public bool TryPopData<T>(string key, out T tValue)
		{
			if(Data.TryGetValue(key, out var value) && value is T dataValue)
			{
				Data.Remove(key);
				tValue = dataValue;
				return true;
			}
			tValue = default;
			return false;
		}
		public bool TryNotPopData<T>(string key, out T tValue)
		{
			if(Data.TryGetValue(key, out var value) && value is T dataValue)
			{
				Data.Remove(key);
				tValue = dataValue;
				return false;
			}
			tValue = default;
			return true;
		}
		public string SaveJson()
		{
			string json = JsonConvert.SerializeObject(Data, new JsonSerializerSettings{
				Formatting = Formatting.Indented
			});
			return json;
		}
		public void LoadJson(string jsonData)
		{
			Data = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData);
		}

		public void SetData<T>(T value) => SetData(typeof(T).FullName, value);
		public bool HasData<T>() => HasData(typeof(T).FullName);
		public bool DeleteData<T>() => DeleteData(typeof(T).FullName);
		public T GetData<T>(T defaultValue = default) => GetData<T>(typeof(T).FullName, defaultValue);
		public bool TryGetData<T>(out T tValue) => TryGetData(typeof(T).FullName, out tValue);
		public bool TryNotGetData<T>(out T tValue) => TryNotGetData(typeof(T).FullName, out tValue);
		public T PopData<T>(T defaultValue = default) => PopData<T>(typeof(T).FullName, defaultValue);
		public bool TryPopData<T>(out T tValue) => TryPopData(typeof(T).FullName, out tValue);
		public bool TryNotPopData<T>(out T tValue) => TryNotPopData(typeof(T).FullName, out tValue);

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if(info == null)
				throw new ArgumentNullException(nameof(info));

			// 데이터 Dictionary를 JSON 문자열로 변환하여 SerializationInfo에 추가
			string json = SaveJson();
			info.AddValue("Data", json);
		}
		protected SaveLoadKeyValue(SerializationInfo info, StreamingContext context)
		{
			if(info == null)
				throw new ArgumentNullException(nameof(info));

			// JSON 데이터를 복원
			string jsonData = info.GetString("Data");
			LoadJson(jsonData);
		}

	}
}

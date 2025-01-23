using System.Collections.Generic;
namespace TFSystem
{
	public static class DataCarrier
	{
		private static SaveLoadKeyValue data = new SaveLoadKeyValue();
		public static List<SeparateData> SeparateDataList;

		public static void SetData<T>(string key, T value) => data.SetData<T>(key, value);
		public static bool HasData(string key) => data.HasData(key);
		public static bool DeleteData(string key) => data.DeleteData(key);
		public static void ClearData() => data.ClearData();
		public static T GetData<T>(string key, T defaultValue = default) => data.GetData(key, defaultValue);
		public static bool TryGetData<T>(string key, out T tValue) => data.TryGetData(key, out tValue);
		public static bool TryNotGetData<T>(string key, out T tValue) => data.TryNotGetData(key, out tValue);
		public static T PopData<T>(string key, T defaultValue = default) => data.PopData(key, defaultValue);
		public static bool TryPopData<T>(string key, out T tValue) => data.TryPopData(key, out tValue);
		public static bool TryNotPopData<T>(string key, out T tValue) => data.TryNotPopData(key, out tValue);

		public static void SetData<T>(T value) => SetData(typeof(T).FullName, value);
		public static bool HasData<T>() => HasData(typeof(T).FullName);
		public static bool DeleteData<T>() => DeleteData(typeof(T).FullName);
		public static T GetData<T>(T defaultValue = default) => GetData<T>(typeof(T).FullName, defaultValue);
		public static bool TryGetData<T>(out T tValue) => TryGetData(typeof(T).FullName, out tValue);
		public static bool TryNotGetData<T>(out T tValue) => TryNotGetData(typeof(T).FullName, out tValue);
		public static T PopData<T>(T defaultValue = default) => PopData<T>(typeof(T).FullName, defaultValue);
		public static bool TryPopData<T>(out T tValue) => TryPopData(typeof(T).FullName, out tValue);
		public static bool TryNotPopData<T>(out T tValue) => TryNotPopData(typeof(T).FullName, out tValue);


		public static SaveLoadKeyValue GetSeparate(string separateName)
		{
			if(string.IsNullOrWhiteSpace(separateName)) return data;
			if(SeparateDataList == null) SeparateDataList = new List<SeparateData>();

			int index = SeparateDataList.FindIndex(x => x.separateKey == separateName);
			if(index < 0)
			{
				index = SeparateDataList.Count;
				SeparateDataList.Add(new SeparateData(separateName));
			}
			return SeparateDataList[index].data;
		}
		public static void DeleteSeparate(string separateName)
		{
			if(string.IsNullOrWhiteSpace(separateName)) return;
			if(SeparateDataList == null) return;

			int index = SeparateDataList.FindIndex(x => x.separateKey == separateName);
			if(index >= 0)
			{
				SeparateDataList[index].data.ClearData();
				SeparateDataList.RemoveAt(index);
			}
		}

		public static SaveLoadKeyValue GetSeparate<T>() => GetSeparate(typeof(T).FullName);
		public static void DeleteSeparate<T>() => DeleteSeparate(typeof(T).FullName);

		public struct SeparateData
		{
			public string separateKey;
			public SaveLoadKeyValue data;
			public SeparateData(string key)
			{
				separateKey = key;
				data = new SaveLoadKeyValue();
			}
		}
	}
}